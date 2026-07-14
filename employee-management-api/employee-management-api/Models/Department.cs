using System;
namespace EmployeeManagementAPI.Models
{
    public class Department
    {
        public int Id { get; set; }
        public string NameEn { get; set; }
        public string NameAr { get; set; }
        public string DepartmentCode { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Status { get; set; } = "Active";
        public string? CreatedBy { get; set; }
        public DateTime CreationDate { get; set; } = DateTime.UtcNow;
        public string? ModifiedBy { get; set; }
        public DateTime? ModificationDate { get; set; }
    }
}
