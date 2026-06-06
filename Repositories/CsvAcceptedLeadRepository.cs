using System.Globalization;
using ExamLeadPortal.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ExamLeadPortal.Repositories
{//this repo is a temporary exam measure, in the real world the repo would handle SQL instead. roughly simulates CRUD (without delete)in SQL
// used to persist data in tabular form, thus emulating a database table
    public class CsvAcceptedLeadRepository : IAcceptedLeadRepository
    {
        private readonly string _csvFilePath;
        private readonly ILogger<CsvAcceptedLeadRepository> _logger;

        public CsvAcceptedLeadRepository(//using app settings to get csv pat
            IConfiguration configuration,
            ILogger<CsvAcceptedLeadRepository> logger)
            {
                _csvFilePath = configuration["AcceptedLeadSettings:FilePath"]
                    ?? throw new Exception("AcceptedLeadSettings:FilePath is not configured");

                _logger = logger;
            }

        public List<AcceptedLead> GetAll()//Loads all accepted leads from CSV
        {
            EnsureFileExists();

            var acceptedLeads = new List<AcceptedLead>();
            var lines = File.ReadAllLines(_csvFilePath);

            if (lines.Length <= 1)
            {
                return acceptedLeads;
            }

            foreach (var line in lines.Skip(1))//for each line in csv constructs a accepted lead object and adds it to a list of accepted leads
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }
                try
                {
                    var columns = ParseCsvLine(line);
                    if (columns.Count < 11)//yeeaaah this is stupid but not meant for examination
                    {
                        _logger.LogWarning("Skipped malformed accepted lead CSV row: {Line}", line);
                        continue;
                    }

                    var acceptedLead = new AcceptedLead//constructs accepted leads for each line loaded
                    {
                        AcceptedLeadId = columns[0],
                        RawLeadId = columns[1],
                        LeadTitle = columns[2],
                        LeadSummary = columns[3],
                        ResponsibleBU = columns[4],
                        LeadValue = columns[5],
                        Probability = columns[6],
                        ResourceLink = columns[7],
                        AcceptedBy = columns[8],
                        AcceptedAt = DateTime.Parse(columns[9], null, DateTimeStyles.RoundtripKind),
                        Status = columns[10]
                    };

                    acceptedLeads.Add(acceptedLead);
                }
                catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to parse accepted lead CSV row: {Line}", line);
                    }
            }

            _logger.LogInformation("Loaded {Count} accepted leads from {FilePath}", acceptedLeads.Count, _csvFilePath);

            return acceptedLeads;
        }

        public AcceptedLead? GetByRawLeadId(string rawLeadId)//fetch function to get a specific accepted lead
        {
            return GetAll().FirstOrDefault(x => x.RawLeadId == rawLeadId);
        }

        public bool ExistsForRawLead(string rawLeadId)//check if raw lead exists
        {
            return GetByRawLeadId(rawLeadId) != null;
        }

        public void Save(AcceptedLead acceptedLead)//saves an accepted lead would be a insert statement in sql 
        {
            EnsureFileExists();

            var line = string.Join(",",
                EscapeCsv(acceptedLead.AcceptedLeadId),
                EscapeCsv(acceptedLead.RawLeadId),
                EscapeCsv(acceptedLead.LeadTitle),
                EscapeCsv(acceptedLead.LeadSummary),
                EscapeCsv(acceptedLead.ResponsibleBU),
                EscapeCsv(acceptedLead.LeadValue),
                EscapeCsv(acceptedLead.Probability),
                EscapeCsv(acceptedLead.ResourceLink),
                EscapeCsv(acceptedLead.AcceptedBy),
                EscapeCsv(acceptedLead.AcceptedAt.ToString("O")),
                EscapeCsv(acceptedLead.Status)
            );

            File.AppendAllLines(_csvFilePath, new[] { line });

            _logger.LogInformation(
                "Saved accepted lead {AcceptedLeadId} for raw lead {RawLeadId}",
                acceptedLead.AcceptedLeadId,
                acceptedLead.RawLeadId);
        }

        private void EnsureFileExists()//basic tool for checking if csv is found
        {
            var directory = Path.GetDirectoryName(_csvFilePath);

            if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            if (!File.Exists(_csvFilePath))
            {
                var header = "AcceptedLeadId,RawLeadId,LeadTitle,LeadSummary,ResponsibleBU,LeadValue,Probability,ResourceLink,AcceptedBy,AcceptedAt,Status";
                File.WriteAllLines(_csvFilePath, new[] { header });

                _logger.LogInformation("Created accepted lead CSV file with header at {FilePath}", _csvFilePath);
            }
        }

        private static string EscapeCsv(string value)//escapement suxx
        {
            if (value.Contains('"'))
            {
                value = value.Replace("\"", "\"\"");
            }

            if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
            {
                value = $"\"{value}\"";
            }

            return value;
        }

        private static List<string> ParseCsvLine(string line) //this also really sucked- but necessary to contain json data in line, will be easier in SQL
        {
            var result = new List<string>();
            var current = string.Empty;
            var inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                var c = line[i];

                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        current += '"';
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(current);
                    current = string.Empty;
                }
                else
                {
                    current += c;
                }
            }

            result.Add(current);
            return result;
        }
    }
}