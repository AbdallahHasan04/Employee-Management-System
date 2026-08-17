namespace Common.Dto
{
    public class EmployeeDocumentUploadDto
    {
        public int EmployeeId { get; set; }
        public string DocumentName { get; set; } = string.Empty;
        public DateTime IssueDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string? Notes { get; set; }
    }
}