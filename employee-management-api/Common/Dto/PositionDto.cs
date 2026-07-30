namespace Common.Dto
{
    public class PositionDto
    {
        public int Id { get; set; }
        public string NameEn { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public string? CreatedBy { get; set; }
        public DateTime CreationDate { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModificationDate { get; set; }
    }
}