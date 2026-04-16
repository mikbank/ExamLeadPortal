using System.Globalization;
using ExamLeadPortal.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ExamLeadPortal.Repositories
{
    public class CsvAcceptedLeadRepository : IAcceptedLeadRepository
    {
        private readonly string _csvFilePath;
        private readonly ILogger<CsvAcceptedLeadRepository> _logger;

        public CsvAcceptedLeadRepository(
            IConfiguration configuration,
            ILogger<CsvAcceptedLeadRepository> logger)
        {
            _csvFilePath = configuration["AcceptedLeadSettings:FilePath"]
                ?? throw new Exception("AcceptedLeadSettings:FilePath is not configured");

            _logger = logger;
        }

        public List<AcceptedLead> GetAll()
        {
            EnsureFileExists();

            var acceptedLeads = new List<AcceptedLead>();
            var lines = File.ReadAllLines(_csvFilePath);

            if (lines.Length <= 1)
            {
                return acceptedLeads;
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
                        _logger.LogWarning("Skipped malformed accepted lead CSV row: {Line}", line);
                        continue;
                    }

                    var acceptedLead = new AcceptedLead
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

        public AcceptedLead? GetByRawLeadId(string rawLeadId)
        {
            return GetAll().FirstOrDefault(x => x.RawLeadId == rawLeadId);
        }

        public bool ExistsForRawLead(string rawLeadId)
        {
            return GetByRawLeadId(rawLeadId) != null;
        }

        public void Save(AcceptedLead acceptedLead)
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

        private void EnsureFileExists()
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

        private static string EscapeCsv(string value)
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

        private static List<string> ParseCsvLine(string line)
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