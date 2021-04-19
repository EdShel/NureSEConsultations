using System.Collections.Generic;

namespace NureSEConsultations.Bot.Model
{
    public record WorksheetConfig(
        string ParserType,
        string TableRange
    );

    public class RepositoryConfiguration
    {
        public RepositoryConfiguration(
            string googleSheetId, 
            string credentialsFile,
            string tokensTempFile,
            IDictionary<string, WorksheetConfig> worksheetConfig)
        {
            this.GoogleSheetId = googleSheetId;
            this.CredentialsFile = credentialsFile;
            this.TokensTempFile = tokensTempFile;
            this.WorksheetConfig = worksheetConfig;
        }

        public string GoogleSheetId { get; set; }

        public string CredentialsFile { get; }

        public string TokensTempFile { get; }

        public IDictionary<string, WorksheetConfig> WorksheetConfig { get; set; }
    }
}
