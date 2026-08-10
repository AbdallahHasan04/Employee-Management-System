using Common.Dto;
using Common.IRepository;
using Common.IServices;

namespace Infrastructure.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IPositionRepository _positionRepository;
        private readonly IEmployeeRepository _employeeRepository;

        public DashboardService(
            IDepartmentRepository departmentRepository,
            IPositionRepository positionRepository,
            IEmployeeRepository employeeRepository)
        {
            _departmentRepository = departmentRepository;
            _positionRepository = positionRepository;
            _employeeRepository = employeeRepository;
        }

        public async Task<DashboardSummaryDto> GetSummaryAsync()
        {
            var totalDepartments = await _departmentRepository.GetTotalCountAsync();
            var totalPositions = await _positionRepository.GetTotalCountAsync();
            var (activeCount, maleCount, femaleCount) = await _employeeRepository.GetEmployeeStatsAsync();

            return new DashboardSummaryDto
            {
                TotalDepartments = totalDepartments,
                TotalPositions = totalPositions,
                TotalActiveEmployees = activeCount,
                TotalMaleEmployees = maleCount,
                TotalFemaleEmployees = femaleCount
            };
        }
    }
}