using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util.Store;
using NureSEConsultations.Bot.Parser;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace NureSEConsultations.Bot.Model
{
    public class ConsultationRepository : IConsultationRepository
    {
        private readonly IParserResolver parserResolver;

        private readonly SheetsService sheets;

        private readonly RepositoryConfiguration config;

        public ConsultationRepository(RepositoryConfiguration config, IParserResolver parserResolver)
        {
            using var credStream = new FileStream(config.CredentialsFile, FileMode.Open, FileAccess.Read);
            var creds = GoogleWebAuthorizationBroker.AuthorizeAsync(
                clientSecrets: GoogleClientSecrets.Load(credStream).Secrets,
                scopes: new[] { SheetsService.Scope.SpreadsheetsReadonly },
                user: "user",
                taskCancellationToken: CancellationToken.None,
                dataStore: new FileDataStore(config.TokensTempFile, true)
            ).Result;

            this.sheets = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = creds,
                ApplicationName = nameof(NureSEConsultations)
            });

            this.config = config;
            this.parserResolver = parserResolver;
        }

        public IEnumerable<Consultation> GetAllByType(string type)
        {
            string range = $"{type}!{this.config.WorksheetConfig[type].TableRange}";
            SpreadsheetsResource.ValuesResource.GetRequest request =
                this.sheets.Spreadsheets.Values.Get(this.config.GoogleSheetId,
                range
            );
            ValueRange response = request.Execute();
            IList<IList<object>> values = response.Values;
            if (values != null)
            {
                TableParser<Consultation> tableParser = this.parserResolver.GetTableParserBySheetName(
                    type, this.config.WorksheetConfig[type].ParserType
                );
                return tableParser.ParseTable(values);
            }
            return Enumerable.Empty<Consultation>();
        }

        public IEnumerable<string> GetConsultationsNames()
        {
            return this.config.WorksheetConfig.Keys.OrderBy(c => c);
        }
    }
}
