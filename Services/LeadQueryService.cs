using ExamLeadPortal.Models;
using ExamLeadPortal.Repositories;
using ExamLeadPortal.ViewModels;

namespace ExamLeadPortal.Services
{
    public class LeadQueryService : ILeadQueryService
    {
        private readonly ILeadRepository _leadRepository;

        public LeadQueryService(ILeadRepository leadRepository)
        {
            _leadRepository = leadRepository;
        }

        public List<Lead> GetFilteredLeads(LeadFilterOptions filterOptions)
        {
            var query = _leadRepository.GetAll().AsEnumerable();

            if (!string.IsNullOrWhiteSpace(filterOptions.TitleFilter))
            {
                query = query.Where(l =>
                    l.LeadTitle.Contains(filterOptions.TitleFilter, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(filterOptions.ValueFilter))
            {
                query = query.Where(l =>
                    l.LeadValue.Equals(filterOptions.ValueFilter, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(filterOptions.BuFilter))
            {
                query = query.Where(l =>
                    l.AffectedBUs.Any(bu =>
                        bu.Contains(filterOptions.BuFilter, StringComparison.OrdinalIgnoreCase)));
            }

            return query.ToList();
        }

        public Lead? GetLeadById(string id)
        {
            return _leadRepository.GetById(id);
        }
    }
}