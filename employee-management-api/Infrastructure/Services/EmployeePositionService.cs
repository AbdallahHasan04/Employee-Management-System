using AutoMapper;
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
        private readonly IMapper _mapper;

        public EmployeePositionService(IEmployeePositionRepository employeePositionRepository, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _employeePositionRepository = employeePositionRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PagedResultDto<EmployeePositionDto>> GetHistoryAsync(int pageNumber, int pageSize, string? sortBy, bool sortDescending, string? search)
        {
            var (items, totalCount) = await _employeePositionRepository.GetPagedAsync(pageNumber, pageSize, sortBy, sortDescending, search);
            return new PagedResultDto<EmployeePositionDto>
            {
                Items = items.Select(ep => _mapper.Map<EmployeePositionDto>(ep)).ToList(),
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

                var newRecord = _mapper.Map<EmployeePosition>(dto);
                newRecord.CreatedBy = createdBy;
                newRecord.CreationDate = DateTime.UtcNow;
                await _employeePositionRepository.AddAsync(newRecord);

                await _unitOfWork.CommitTransactionAsync();
                return _mapper.Map<EmployeePositionDto>(newRecord);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
    }
}