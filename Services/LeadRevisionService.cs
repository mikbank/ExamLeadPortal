using System.Text.Json;
using ExamLeadPortal.Models;
using ExamLeadPortal.Repositories;

namespace ExamLeadPortal.Services
{//service for handling revisions of leads, used by lead query to get current state of lead, but also used in creation and historic needs
    public class LeadRevisionService : ILeadRevisionService
    {
        private readonly ILeadRevisionRepository _leadRevisionRepository;

        public LeadRevisionService(
            ILeadRevisionRepository leadRevisionRepository)
        {
            _leadRevisionRepository = leadRevisionRepository;
        }

        public LeadRevision? GetLatestRevision(string rawLeadId)//fetches newest lead from repository, important for views
        {
            ValidateRawLeadId(rawLeadId);

            return _leadRevisionRepository
                .GetLatestByRawLeadId(rawLeadId);
        }

        public List<LeadRevision> GetRevisionHistory(string rawLeadId)//fetches version history
        {
            ValidateRawLeadId(rawLeadId);

            return _leadRevisionRepository
                .GetByRawLeadId(rawLeadId);
        }

        public LeadRevision CreateRevision(//create a new revision of a lead, a valid raw ID is required as we enforce one-to-one relation
            string rawLeadId,
            RawLead revisedLead,//inheriting datatype rawlead for the revised lead, only used to make sure the lead remains consistent
            string revisedBy)
        {
            ValidateRawLeadId(rawLeadId);

            if (revisedLead == null)//if a revision is created without the revised lead is constructed we throw error here
                {
                    throw new ArgumentNullException(nameof(revisedLead));
                }

            if (string.IsNullOrWhiteSpace(revisedBy))
                {
                    throw new ArgumentException(
                        "Revised by cannot be empty.",//currently without much value, we need unigue users to have any value from this
                        nameof(revisedBy));
                }

            if (!string.Equals(
                    rawLeadId,
                    revisedLead.Id,
                        StringComparison.OrdinalIgnoreCase))
                {
                    throw new ArgumentException(
                        "Whoops! mismatch on lead ID, from original to new! - Check raw lead creation!",//sanity check to see if the parsed rawlead is kept in revised lead
                        nameof(revisedLead));
                }

            var latestRevision = _leadRevisionRepository
                .GetLatestByRawLeadId(rawLeadId);//fetches latest revision from repo, note: a bit spaghetti code that i dont use GetLatestRevision, but also dont want to do double validation

            var nextVersionNumber =
                latestRevision?.VersionNumber + 1 ?? 1;//creates new revision number, could be parsed in, but the service should auto increment especially if we wanna imitate SQL here

            var revision = new LeadRevision //FINALLY creating the revision object
                {
                    RevisionId = Guid.NewGuid(),
                    RawLeadId = rawLeadId,
                    VersionNumber = nextVersionNumber,

                    LeadTitle = revisedLead.LeadTitle,
                    LeadSummary = revisedLead.LeadSummary,
                    LeadValue = revisedLead.LeadValue,
                    ResourceLink = revisedLead.ResourceLink,

                    AffectedBUs = new List<string>(
                        revisedLead.AffectedBUs),

                    LeadPayload =
                        new Dictionary<string, JsonElement>(
                            revisedLead.LeadPayload),

                    RevisedBy = revisedBy,
                    RevisedAt = DateTime.Now
                };

            _leadRevisionRepository.Save(revision); //using repo to save the revision

            return revision;
        }

        public RawLead GetCurrentLead(RawLead rawLead) //fetches the current state of a lead, and constructs a new one by copying the rawlead if no revision is found
        {
            if (rawLead == null)
            {
                throw new ArgumentNullException(nameof(rawLead));
            }

            var latestRevision = _leadRevisionRepository
                .GetLatestByRawLeadId(rawLead.Id);

            if (latestRevision == null)
            {
                return CopyRawLead(rawLead);
            }

            return new RawLead //constructs a new class of rawlead from the data model, injecting revision or raw lead data
            {
                Id = latestRevision.RawLeadId,
                LeadTitle = latestRevision.LeadTitle,
                LeadSummary = latestRevision.LeadSummary,
                LeadValue = latestRevision.LeadValue,
                ResourceLink = latestRevision.ResourceLink,

                AffectedBUs =
                    new List<string>(
                        latestRevision.AffectedBUs),

                LeadPayload =
                    new Dictionary<string, JsonElement>(
                        latestRevision.LeadPayload)
            };
        }

        private static RawLead CopyRawLead(RawLead rawLead) //used to copy in the values of a raw lead for creating a revision
        {
            return new RawLead
            {
                Id = rawLead.Id,
                LeadTitle = rawLead.LeadTitle,
                LeadSummary = rawLead.LeadSummary,
                LeadValue = rawLead.LeadValue,
                ResourceLink = rawLead.ResourceLink,

                AffectedBUs =
                    new List<string>(rawLead.AffectedBUs),

                LeadPayload =
                    new Dictionary<string, JsonElement>(
                        rawLead.LeadPayload)
            };
        }

        private static void ValidateRawLeadId(string rawLeadId) //defensive function to make sure we can match on raw lead IDs
        {
            if (string.IsNullOrWhiteSpace(rawLeadId))
            {
                throw new ArgumentException(
                    "Raw lead id cannot be empty.",
                    nameof(rawLeadId));
            }
        }
    }
}