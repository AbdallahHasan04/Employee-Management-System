namespace Common.Dto
{
    public class EmployeeDto
    {
        public int Id { get; set; }
        public string EmployeeNo { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public DateTime? Birthdate { get; set; }
        public string NationalNo { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public string Status { get; set; } = "Active";
        public string? MobileNumber { get; set; }
        public string? Email { get; set; }
        public DateTime? StartWorkingDate { get; set; }
        public int DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public int? PositionId { get; set; }
        public string? PositionName { get; set; }
        public string? ProfileImagePath { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreationDate { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModificationDate { get; set; }
        public string? GeneratedPassword { get; set; }
    }
}