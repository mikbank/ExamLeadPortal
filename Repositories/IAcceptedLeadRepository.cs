using ExamLeadPortal.Models;

namespace ExamLeadPortal.Repositories
{//Interface for accepted lead repo or datasource, contract on actions available to the rest of the app - seems OKAY for SQL migration
    public interface IAcceptedLeadRepository
    {
        List<AcceptedLead> GetAll();
        AcceptedLead? GetByRawLeadId(string rawLeadId);
        void Save(AcceptedLead acceptedLead);
        bool ExistsForRawLead(string rawLeadId);
    }
}