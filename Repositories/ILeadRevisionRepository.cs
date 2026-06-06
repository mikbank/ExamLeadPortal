using ExamLeadPortal.Models;

namespace ExamLeadPortal.Repositories
{//interface for lead revision repo, main contract here, needs minor tweaking if moving to SQL
    public interface ILeadRevisionRepository 
    {
        List<LeadRevision> GetAll();

        List<LeadRevision> GetByRawLeadId(string rawLeadId);

        LeadRevision? GetLatestByRawLeadId(string rawLeadId);

        LeadRevision? GetByRevisionId(Guid revisionId);

        void Save(LeadRevision revision);
    }
}