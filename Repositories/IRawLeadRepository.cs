using ExamLeadPortal.Models;

namespace ExamLeadPortal.Repositories
{
    public interface IRawLeadRepository
    {
        List<RawLead> GetAll();
        RawLead? GetById(string id);
    }
}