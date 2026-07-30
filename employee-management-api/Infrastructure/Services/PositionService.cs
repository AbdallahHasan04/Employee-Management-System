using Common.Dto;
using Common.IRepository;
using Common.IServices;
using Core.Entities;

namespace Infrastructure.Services
{
    public class PositionService : IPositionService
    {
        private readonly IPositionRepository _positionRepository;
        private readonly IEmployeePositionRepository _employeePositionRepository;

        public PositionService(IPositionRepository positionRepository, IEmployeePositionRepository employeePositionRepository)
        {
            _positionRepository = positionRepository;
            _employeePositionRepository = employeePositionRepository;
        }

        public async Task<PagedResultDto<PositionDto>> GetAllPositionsAsync(int pageNumber, int pageSize, string? sortBy, bool sortDescending, string? search)
        {
            var (items, totalCount) = await _positionRepository.GetPagedAsync(pageNumber, pageSize, sortBy, sortDescending, search);
            return new PagedResultDto<PositionDto>
            {
                Items = items.Select(ToDto).ToList(),
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<PositionDto?> GetPositionByIdAsync(int id)
        {
            var position = await _positionRepository.GetByIdAsync(id);
            return position == null ? null : ToDto(position);
        }

        public async Task<PositionDto> CreatePositionAsync(PositionDto dto, string? createdBy)
        {
            var position = new Position
            {
                NameEn = dto.NameEn,
                NameAr = dto.NameAr,
                CreatedBy = createdBy,
                CreationDate = DateTime.UtcNow
            };

            await _positionRepository.AddAsync(position);
            return ToDto(position);
        }

        public async Task<bool> UpdatePositionAsync(PositionDto dto, string? modifiedBy)
        {
            var existing = await _positionRepository.GetByIdAsync(dto.Id);
            if (existing == null)
            {
                return false;
            }

            existing.NameEn = dto.NameEn;
            existing.NameAr = dto.NameAr;
            existing.ModifiedBy = modifiedBy;
            existing.ModificationDate = DateTime.UtcNow;

            await _positionRepository.UpdateAsync(existing);
            return true;
        }

        public async Task<PositionDeleteResult> DeletePositionAsync(int id, string? deletedBy)
        {
            var existing = await _positionRepository.GetByIdAsync(id);
            if (existing == null)
            {
                return PositionDeleteResult.NotFound;
            }

            var inUse = await _employeePositionRepository.ExistsByPositionIdAsync(id);
            if (inUse)
            {
                return PositionDeleteResult.InUse;
            }

            await _positionRepository.DeleteAsync(id, deletedBy);
            return PositionDeleteResult.Success;
        }

        private static PositionDto ToDto(Position position)
        {
            return new PositionDto
            {
                Id = position.Id,
                NameEn = position.NameEn,
                NameAr = position.NameAr,
                CreatedBy = position.CreatedBy,
                CreationDate = position.CreationDate,
                ModifiedBy = position.ModifiedBy,
                ModificationDate = position.ModificationDate
            };
        }
    }
}