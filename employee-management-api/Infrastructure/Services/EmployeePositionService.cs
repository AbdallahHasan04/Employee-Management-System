using Common.Dto;
using Common.IRepository;
using Common.IServices;
using Core.Entities;

namespace Infrastructure.Services
{
    public class EmployeePositionService : IEmployeePositionService
    {
        private readonly IEmployeePositionRepository _employeePositionRepository;
        private readonly IUnitOfWork _unitOfWork;

        public EmployeePositionService(IEmployeePositionRepository employeePositionRepository, IUnitOfWork unitOfWork)
        {
            _employeePositionRepository = employeePositionRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<PagedResultDto<EmployeePositionDto>> GetHistoryAsync(int pageNumber, int pageSize, string? sortBy, bool sortDescending, string? search)
        {
            var (items, totalCount) = await _employeePositionRepository.GetPagedAsync(pageNumber, pageSize, sortBy, sortDescending, search);
            return new PagedResultDto<EmployeePositionDto>
            {
                Items = items.Select(ToDto).ToList(),
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<EmployeePositionDto> AssignPositionAsync(AssignPositionDto dto, string? createdBy)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // close out whichever position the employee currently holds, if any
                var current = await _employeePositionRepository.GetCurrentByEmployeeIdAsync(dto.EmployeeId);
                if (current != null)
                {
                    current.EndDate = dto.StartDate;
                    current.ModifiedBy = createdBy;
                    current.ModificationDate = DateTime.UtcNow;
                    await _employeePositionRepository.UpdateAsync(current);
                }

                var newRecord = new EmployeePosition
                {
                    EmployeeId = dto.EmployeeId,
                    PositionId = dto.PositionId,
                    StartDate = dto.StartDate,
                    EndDate = null,
                    CreatedBy = createdBy,
                    CreationDate = DateTime.UtcNow
                };
                await _employeePositionRepository.AddAsync(newRecord);

                await _unitOfWork.CommitTransactionAsync();
                return ToDto(newRecord);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        private static EmployeePositionDto ToDto(EmployeePosition ep)
        {
            return new EmployeePositionDto
            {
                Id = ep.Id,
                EmployeeId = ep.EmployeeId,
                EmployeeName = ep.Employee?.NameEn,
                PositionId = ep.PositionId,
                PositionName = ep.Position?.NameEn,
                StartDate = ep.StartDate,
                EndDate = ep.EndDate,
                CreatedBy = ep.CreatedBy,
                CreationDate = ep.CreationDate,
                ModifiedBy = ep.ModifiedBy,
                ModificationDate = ep.ModificationDate
            };
        }
    }
}