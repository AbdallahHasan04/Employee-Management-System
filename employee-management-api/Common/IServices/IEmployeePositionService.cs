using Common.Dto;

namespace Common.IServices
{
    public interface IEmployeePositionService
    {
        Task<PagedResultDto<EmployeePositionDto>> GetHistoryAsync(int pageNumber, int pageSize, string? sortBy, bool sortDescending, string? search);
        Task<EmployeePositionDto> AssignPositionAsync(AssignPositionDto dto, string? createdBy);
    }
}