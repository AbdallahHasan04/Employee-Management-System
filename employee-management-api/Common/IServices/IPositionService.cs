using Common.Dto;

namespace Common.IServices
{
    public enum PositionDeleteResult
    {
        Success,
        NotFound,
        InUse
    }

    public interface IPositionService
    {
        Task<PagedResultDto<PositionDto>> GetAllPositionsAsync(int pageNumber, int pageSize, string? sortBy, bool sortDescending, string? search);
        Task<PositionDto?> GetPositionByIdAsync(int id);
        Task<PositionDto> CreatePositionAsync(PositionDto positionDto, string? createdBy);
        Task<bool> UpdatePositionAsync(PositionDto positionDto, string? modifiedBy);
        Task<PositionDeleteResult> DeletePositionAsync(int id, string? deletedBy);
    }
}