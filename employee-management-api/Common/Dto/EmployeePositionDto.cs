namespace Common.Dto
{
    public class EmployeePositionDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public int PositionId { get; set; }
        public string? PositionName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreationDate { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModificationDate { get; set; }
    }
}