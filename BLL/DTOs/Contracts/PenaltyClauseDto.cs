namespace BLL.DTOs.Contracts
{
    public class PenaltyClauseDto
    {
        public string Title { get; set; } = null!;
        public string ArabicText { get; set; } = null!;
        public decimal? Rate { get; set; }
        public decimal? MaxCapRate { get; set; }
        public int? TerminationDays { get; set; }
    }
}
