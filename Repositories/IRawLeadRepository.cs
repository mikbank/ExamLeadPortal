using ExamLeadPortal.Models;

namespace ExamLeadPortal.Repositories
{//interface for fetching raw leads from persistent data source, quite simple, might change on fabric integration
    public interface IRawLeadRepository
    {
        List<RawLead> GetAll();
        RawLead? GetById(string id);
    }
}