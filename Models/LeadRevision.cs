using System.Text.Json;

namespace ExamLeadPortal.Models
{
    public class LeadRevision //datamodel for Lead revision object
    { //contains revision references
        public Guid RevisionId { get; set; }
        public string RawLeadId { get; set; } = string.Empty;
        public int VersionNumber { get; set; }

        //contains basic lead info like rawleads but does not inherit rawleads, as this object carries its own rules and also we dont want to create a raw lead object by accident
        public string LeadTitle { get; set; } = string.Empty;
        public string LeadSummary { get; set; } = string.Empty;
        public string LeadValue { get; set; } = string.Empty;
        public string ResourceLink { get; set; } = string.Empty;
        public List<string> AffectedBUs { get; set; } = new();
        public Dictionary<string, JsonElement> LeadPayload { get; set; } = new();

        //post revision data, timestamp and user audit
        public string RevisedBy { get; set; } = string.Empty;
        public DateTime RevisedAt { get; set; }
    }
}