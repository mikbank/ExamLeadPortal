using System.Globalization;
using System.Text.Json;
using ExamLeadPortal.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ExamLeadPortal.Repositories
{//repo to get revisions, same as accepted leads; will migrate to SQL in the real world
//// used to persist data in tabular form, thus emulating a database table
    public class CsvLeadRevisionRepository : ILeadRevisionRepository
    {
        private readonly string _csvFilePath;
        private readonly ILogger<CsvLeadRevisionRepository> _logger;

        public CsvLeadRevisionRepository(//using app settings to get csv pat
            IConfiguration configuration,
            ILogger<CsvLeadRevisionRepository> logger)
            {
                _csvFilePath = configuration["LeadRevisionSettings:FilePath"]
                    ?? throw new Exception(
                        "LeadRevisionSettings:FilePath is not configured.");
                _logger = logger;
            }

        public List<LeadRevision> GetAll()//Loads all lead revisions from CSV
        {
            EnsureFileExists();

            var revisions = new List<LeadRevision>();
            var lines = File.ReadAllLines(_csvFilePath);

            if (lines.Length <= 1)
            {
                return revisions;
            }

            foreach (var line in lines.Skip(1))
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }
                try
                {
                    var columns = ParseCsvLine(line);

                    if (columns.Count < 11)
                    {
                        _logger.LogWarning(
                            "Skipped malformed lead revision CSV row: {Line}",
                            line);

                        continue;
                    }

                    var revision = ParseRevision(columns);

                    revisions.Add(revision);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Failed to parse lead revision CSV row: {Line}",
                        line);
                }
            }

            _logger.LogInformation(
                "Loaded {Count} lead revisions from {FilePath}",
                revisions.Count,
                _csvFilePath);

            return revisions;
        }

        public List<LeadRevision> GetByRawLeadId(string rawLeadId)//return list of revision by raw lead ID
        {
            if (string.IsNullOrWhiteSpace(rawLeadId))
            {
                throw new ArgumentException(
                    "Raw lead id cannot be empty.",
                    nameof(rawLeadId));
            }

            return GetAll()
                .Where(x => string.Equals(
                    x.RawLeadId,
                    rawLeadId,
                    StringComparison.OrdinalIgnoreCase))
                .OrderBy(x => x.VersionNumber)
                .ToList();
        }

        public LeadRevision? GetLatestByRawLeadId(string rawLeadId)
        {
            if (string.IsNullOrWhiteSpace(rawLeadId))
            {
                throw new ArgumentException(
                    "Raw lead id cannot be empty.",
                    nameof(rawLeadId));
            }

            return GetAll()
                .Where(x => string.Equals(
                    x.RawLeadId,
                    rawLeadId,
                    StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(x => x.VersionNumber)
                .FirstOrDefault();
        }

        public LeadRevision? GetByRevisionId(Guid revisionId)
        {
            if (revisionId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Revision id cannot be empty.",
                    nameof(revisionId));
            }

            return GetAll()
                .FirstOrDefault(x => x.RevisionId == revisionId);
        }

        public void Save(LeadRevision revision)//save the revision
        {
            if (revision == null)
            {
                throw new ArgumentNullException(nameof(revision));
            }

            ValidateRevision(revision);
            EnsureFileExists();
            EnsureRevisionIsUnique(revision);

            var affectedBUsJson =
                JsonSerializer.Serialize(revision.AffectedBUs);

            var leadPayloadJson =
                JsonSerializer.Serialize(revision.LeadPayload);

            var line = string.Join(",",
                    EscapeCsv(revision.RevisionId.ToString()),
                    EscapeCsv(revision.RawLeadId),
                    EscapeCsv(revision.VersionNumber.ToString(
                        CultureInfo.InvariantCulture)),
                    EscapeCsv(revision.LeadTitle),
                    EscapeCsv(revision.LeadSummary),
                    EscapeCsv(revision.LeadValue),
                    EscapeCsv(revision.ResourceLink),
                    EscapeCsv(affectedBUsJson),
                    EscapeCsv(leadPayloadJson),
                    EscapeCsv(revision.RevisedBy),
                    EscapeCsv(revision.RevisedAt.ToString("O"))
                
            );

            File.AppendAllLines(
                _csvFilePath,
                new[] { line });

            _logger.LogInformation(
                "Saved revision {VersionNumber} with id {RevisionId} " +
                "for raw lead {RawLeadId}",
                revision.VersionNumber,
                revision.RevisionId,
                revision.RawLeadId);
        }

        private LeadRevision ParseRevision(List<string> columns)
        {
            var affectedBUs =
                JsonSerializer.Deserialize<List<string>>(columns[7])
                ?? new List<string>();

            var leadPayload =
                JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(
                    columns[8])
                ?? new Dictionary<string, JsonElement>();

            return new LeadRevision
            {
                RevisionId = Guid.Parse(columns[0]),
                RawLeadId = columns[1],
                VersionNumber = int.Parse(
                    columns[2],
                    CultureInfo.InvariantCulture),

                LeadTitle = columns[3],
                LeadSummary = columns[4],
                LeadValue = columns[5],
                ResourceLink = columns[6],

                AffectedBUs = affectedBUs,
                LeadPayload = leadPayload,

                RevisedBy = columns[9],
                RevisedAt = DateTime.Parse(
                    columns[10],
                    null,
                    DateTimeStyles.RoundtripKind)//wierd fix to handle datetime causing issues in csv
            };
        }

        private void EnsureRevisionIsUnique(LeadRevision revision) //!!!emulates constraints on SQL, this sucked to write!!
        {
            var existingRevisions = GetAll();//fetches revisions

            if (existingRevisions.Any(
                    x => x.RevisionId == revision.RevisionId)) //tries matchin on revision ID, ID is a guid so very unlikely!
            {
                throw new InvalidOperationException(
                    $"A revision with id {revision.RevisionId} " +
                    "already exists.");
            }

            var duplicateVersion = existingRevisions.Any(
                x => string.Equals(
                         x.RawLeadId,
                         revision.RawLeadId,
                         StringComparison.OrdinalIgnoreCase)
                     && x.VersionNumber == revision.VersionNumber);//if the same version number has already been used throw an error!

            if (duplicateVersion)
            {
                throw new InvalidOperationException(
                    $"Revision version {revision.VersionNumber} already " +
                    $"exists for raw lead {revision.RawLeadId}.");
            }
        }

        private static void ValidateRevision(LeadRevision revision)
        {
            if (revision.RevisionId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Revision must have a revision id.",
                    nameof(revision));
            }

            if (string.IsNullOrWhiteSpace(revision.RawLeadId))
            {
                throw new ArgumentException(
                    "Revision must reference a raw lead id.",
                    nameof(revision));
            }

            if (revision.VersionNumber < 1)
            {
                throw new ArgumentException(
                    "Revision version number must be greater than zero.",
                    nameof(revision));
            }

            if (string.IsNullOrWhiteSpace(revision.RevisedBy))
            {
                throw new ArgumentException(
                    "Revision must specify who created it.",
                    nameof(revision));
            }

            if (revision.RevisedAt == default)
            {
                throw new ArgumentException(
                    "Revision must have a revision timestamp.",
                    nameof(revision));
            }
        }

        private void EnsureFileExists()
        {
            var directory = Path.GetDirectoryName(_csvFilePath);

            if (!string.IsNullOrWhiteSpace(directory)
                && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            if (!File.Exists(_csvFilePath))
            {
                const string header =
                    "RevisionId," +
                    "RawLeadId," +
                    "VersionNumber," +
                    "LeadTitle," +
                    "LeadSummary," +
                    "LeadValue," +
                    "ResourceLink," +
                    "AffectedBUs," +
                    "LeadPayload," +
                    "RevisedBy," +
                    "RevisedAt,";
                    

                File.WriteAllLines(
                    _csvFilePath,
                    new[] { header });

                _logger.LogInformation(
                    "Created lead revision CSV file with header at {FilePath}",
                    _csvFilePath);
            }
        }

        private static string EscapeCsv(string? value) //escapement 
        {
            value ??= string.Empty;

            if (value.Contains('"'))
            {
                value = value.Replace("\"", "\"\"");
            }

            if (value.Contains(',')
                || value.Contains('"')
                || value.Contains('\n')
                || value.Contains('\r'))
            {
                value = $"\"{value}\"";
            }

            return value;
        }

        private static List<string> ParseCsvLine(string line)// json REALLY sucks on csv!
        {
            var result = new List<string>();
            var current = string.Empty;
            var inQuotes = false;

            for (var i = 0; i < line.Length; i++)
            {
                var character = line[i];

                if (character == '"')
                {
                    if (inQuotes
                        && i + 1 < line.Length
                        && line[i + 1] == '"')
                    {
                        current += '"';
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (character == ',' && !inQuotes)
                {
                    result.Add(current);
                    current = string.Empty;
                }
                else
                {
                    current += character;
                }
            }

            result.Add(current);

            return result;
        }
    }
}