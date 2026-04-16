using ExamLeadPortal.Models;
using ExamLeadPortal.Repositories;
using ExamLeadPortal.ViewModels;

namespace ExamLeadPortal.Services
{
    public class LeadQueryService : ILeadQueryService
    {
        private readonly IRawLeadRepository _leadRepository;

        public LeadQueryService(IRawLeadRepository leadRepository)
        {
            _leadRepository = leadRepository;
        }

        public List<LeadListItemViewModel> GetFilteredLeads(LeadFilterOptions filterOptions)
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

            return query
                .Select(l => new LeadListItemViewModel
                {
                    Lead = l,
                    IsIncomplete = IsIncomplete(l)
                })
                .ToList();
        }

        public RawLead? GetLeadById(string id)
        {
            return _leadRepository.GetById(id);
        }

        private static bool IsIncomplete(RawLead lead)
        {
            return string.IsNullOrWhiteSpace(lead.LeadValue)
                || lead.AffectedBUs == null
                || !lead.AffectedBUs.Any()
                || string.IsNullOrWhiteSpace(lead.ResourceLink);
        }
    }
}