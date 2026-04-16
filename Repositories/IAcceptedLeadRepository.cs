using ExamLeadPortal.Models;

namespace ExamLeadPortal.Repositories
{
    public interface IAcceptedLeadRepository
    {
        List<AcceptedLead> GetAll();
        AcceptedLead? GetByRawLeadId(string rawLeadId);
        void Save(AcceptedLead acceptedLead);
        bool ExistsForRawLead(string rawLeadId);
    }
}