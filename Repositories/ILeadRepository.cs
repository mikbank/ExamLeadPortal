using ExamLeadPortal.Models;

namespace ExamLeadPortal.Repositories
{
    public interface ILeadRepository
    {
        List<Lead> GetAll();
        Lead? GetById(string id);
    }
}