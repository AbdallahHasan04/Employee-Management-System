using System;

namespace Core.Entities
{
    public class EmployeePosition
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public Employee? Employee { get; set; }
        public int PositionId { get; set; }
        public Position? Position { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsDeleted { get; set; } = false;
        public string? CreatedBy { get; set; }
        public DateTime CreationDate { get; set; } = DateTime.UtcNow;
        public string? ModifiedBy { get; set; }
        public DateTime? ModificationDate { get; set; }
    }
}