using ExamLeadPortal.Models;

namespace ExamLeadPortal.Services
{//interface for lead revisions - jhandles fetching latest revision, rev history, creation and fetching the current state of a lead
    public interface ILeadRevisionService
    {
        LeadRevision? GetLatestRevision(string rawLeadId);

        List<LeadRevision> GetRevisionHistory(string rawLeadId);

        LeadRevision CreateRevision(
            string rawLeadId,
            RawLead revisedLead,
            string revisedBy);
        RawLead GetCurrentLead(RawLead rawLead);
    }
}