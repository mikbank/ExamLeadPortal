using System.Text.Json;

namespace ExamLeadPortal.Models
{
    public class LeadCorrection
    {
        public string RawLeadId { get; set; } = string.Empty;

        public string? LeadTitle { get; set; }
        public string? LeadSummary { get; set; }
        public string? LeadValue { get; set; }
        public string? ResourceLink { get; set; }

        public List<string>? AffectedBUs { get; set; }

        public Dictionary<string, JsonElement> CorrectedPayload { get; set; } = new();

        public DateTime CorrectedAt { get; set; } = DateTime.Now;
    }
}