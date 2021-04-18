using System.Collections.Generic;

namespace NureSEConsultations.Bot.Model
{
    public class RepositoryConfiguration
    {
        public RepositoryConfiguration(
            string googleSheetId, 
            string credentialsFile,
            string tokensTempFile,
            IDictionary<string, string> worksheetParserName)
        {
            this.GoogleSheetId = googleSheetId;
            this.CredentialsFile = credentialsFile;
            this.TokensTempFile = tokensTempFile;
            this.WorksheetParserName = worksheetParserName;
        }

        public string GoogleSheetId { get; set; }

        public string CredentialsFile { get; }

        public string TokensTempFile { get; }

        public IDictionary<string, string> WorksheetParserName { get; set; }
    }
}
