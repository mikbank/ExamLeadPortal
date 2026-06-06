using ExamLeadPortal.Models;
using ExamLeadPortal.Repositories;
using ExamLeadPortal.ViewModels;

namespace ExamLeadPortal.Services
{//Query service for the application, if working on a different scale this should be refactored
//the query service handles substituting raw leads with revisions if applicable using the lead revision service, thus seperating concerns
    public class LeadQueryService : ILeadQueryService 
    {
        private readonly IRawLeadRepository _leadRepository;
        private readonly ILeadRevisionService _leadRevisionService;

        public LeadQueryService(
            IRawLeadRepository leadRepository,
            ILeadRevisionService leadRevisionService)
        {
            _leadRepository = leadRepository;
            _leadRevisionService = leadRevisionService;
        }

        public List<LeadListItemViewModel> GetFilteredLeads(
            LeadFilterOptions filterOptions)
        {
            var query = _leadRepository
                .GetAll() //fetches all raw leads
                .Select(rawLead =>
                    _leadRevisionService.GetCurrentLead(rawLead))//using linq selects raw leads and for each of them uses the lead revision service to fetch newest lead revision
                .AsEnumerable();//outputs a list that can be used for filtering - Please note for this exam this is okay but if we assume changing over to a platform of millions of leads we should change here and in repo

            if (!string.IsNullOrWhiteSpace(
                    filterOptions.TitleFilter)) //Filter section, they are applied on the enumerable list of raw/revised leads
            {
                query = query.Where(l =>
                    l.LeadTitle.Contains(
                        filterOptions.TitleFilter,
                        StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(
                    filterOptions.ValueFilter))
            {
                query = query.Where(l =>
                    l.LeadValue.Equals(
                        filterOptions.ValueFilter,
                        StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(
                    filterOptions.BuFilter))
            {
                query = query.Where(l =>
                    l.AffectedBUs.Any(bu =>
                        bu.Contains(
                            filterOptions.BuFilter,
                            StringComparison.OrdinalIgnoreCase)));
            }

            return query
                .Select(l => new LeadListItemViewModel
                {
                    Lead = l,
                    IsIncomplete = IsIncomplete(l)
                })
                .ToList();
        }

        public RawLead? GetLeadById(string id) //used to get a specific Lead 
        {
            var rawLead = _leadRepository.GetById(id);

            if (rawLead == null)
            {
                return null;
            }

            return _leadRevisionService.GetCurrentLead(rawLead);
        }

        private static bool IsIncomplete(RawLead lead) //used to check if a raw lead is missing information for being eligible for acceptance
        {
            return string.IsNullOrWhiteSpace(lead.LeadValue)
                || lead.AffectedBUs == null
                || !lead.AffectedBUs.Any()
                || string.IsNullOrWhiteSpace(lead.ResourceLink);
        }
    }
}