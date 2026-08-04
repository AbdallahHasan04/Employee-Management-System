using AutoMapper;
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
        private readonly IMapper _mapper;

        public PositionService(IPositionRepository positionRepository, IEmployeePositionRepository employeePositionRepository, IMapper mapper)
        {
            _positionRepository = positionRepository;
            _employeePositionRepository = employeePositionRepository;
            _mapper = mapper;
        }

        public async Task<PagedResultDto<PositionDto>> GetAllPositionsAsync(int pageNumber, int pageSize, string? sortBy, bool sortDescending, string? search)
        {
            var (items, totalCount) = await _positionRepository.GetPagedAsync(pageNumber, pageSize, sortBy, sortDescending, search);

            var positionIds = items.Select(p => p.Id).ToList();
            var counts = await _employeePositionRepository.GetCurrentEmployeeCountsForPositionIdsAsync(positionIds);

            return new PagedResultDto<PositionDto>
            {
                Items = items.Select(p => MapToDto(p, counts.GetValueOrDefault(p.Id, 0))).ToList(),
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<PositionDto?> GetPositionByIdAsync(int id)
        {
            var position = await _positionRepository.GetByIdAsync(id);
            if (position == null)
            {
                return null;
            }

            var counts = await _employeePositionRepository.GetCurrentEmployeeCountsForPositionIdsAsync(new[] { id });
            return MapToDto(position, counts.GetValueOrDefault(id, 0));
        }

        public async Task<PositionDto> CreatePositionAsync(PositionDto dto, string? createdBy)
        {
            var position = _mapper.Map<Position>(dto);
            position.CreatedBy = createdBy;
            position.CreationDate = DateTime.UtcNow;

            await _positionRepository.AddAsync(position);
            return MapToDto(position, 0);
        }

        public async Task<bool> UpdatePositionAsync(PositionDto dto, string? modifiedBy)
        {
            var existing = await _positionRepository.GetByIdAsync(dto.Id);
            if (existing == null)
            {
                return false;
            }

            _mapper.Map(dto, existing);
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

        private PositionDto MapToDto(Position position, int employeeCount)
        {
            var dto = _mapper.Map<PositionDto>(position);
            dto.EmployeeCount = employeeCount;
            return dto;
        }
    }
}