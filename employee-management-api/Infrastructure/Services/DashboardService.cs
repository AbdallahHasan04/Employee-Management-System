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
        private readonly IEmployeePositionRepository _employeePositionRepository;

        public DashboardService(
            IDepartmentRepository departmentRepository,
            IPositionRepository positionRepository,
            IEmployeeRepository employeeRepository,
            IEmployeePositionRepository employeePositionRepository)
        {
            _departmentRepository = departmentRepository;
            _positionRepository = positionRepository;
            _employeeRepository = employeeRepository;
            _employeePositionRepository = employeePositionRepository;
        }

        public async Task<DashboardSummaryDto> GetSummaryAsync()
        {
            // single grouped query for the active/male/female employee stats.
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

        public async Task<List<DepartmentEmployeeCountDto>> GetEmployeesByDepartmentAsync()
        {
            var departments = await _departmentRepository.GetAllAsync();
            var departmentIds = departments.Select(d => d.Id).ToList();
            var counts = await _employeeRepository.GetEmployeeCountsForDepartmentIdsAsync(departmentIds);

            return departments
                .Select(d => new DepartmentEmployeeCountDto
                {
                    DepartmentNameEn = d.NameEn,
                    DepartmentNameAr = d.NameAr,
                    EmployeeCount = counts.GetValueOrDefault(d.Id, 0)
                })
                .OrderBy(d => d.DepartmentNameEn)
                .ToList();
        }

        public async Task<List<PositionEmployeeCountDto>> GetEmployeesByPositionAsync()
        {
            var positions = await _positionRepository.GetAllAsync();
            var positionIds = positions.Select(p => p.Id).ToList();
            var counts = await _employeePositionRepository.GetCurrentEmployeeCountsForPositionIdsAsync(positionIds);

            return positions
                .Select(p => new PositionEmployeeCountDto
                {
                    PositionNameEn = p.NameEn,
                    PositionNameAr = p.NameAr,
                    EmployeeCount = counts.GetValueOrDefault(p.Id, 0)
                })
                .OrderBy(p => p.PositionNameEn)
                .ToList();
        }
    }
}