using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util.Store;
using NureSEConsultations.Bot.Parser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace NureSEConsultations.Bot.Model
{
    public class ConsultationRepository
    {
        private readonly IDictionary<string, IEnumerable<Consultation>> consultations;

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

            var sheets = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = creds,
                ApplicationName = nameof(NureSEConsultations)
            });

            string range = "3 курс, ПЗПІ-18!A6:E";
            SpreadsheetsResource.ValuesResource.GetRequest request =
                    sheets.Spreadsheets.Values.Get(spreadsheetId, range);
            ValueRange response = request.Execute();
            IList<IList<object>> values = response.Values;
            if (values != null && values.Count > 0)
            {
                TableParser<Consultation> tableParser;
                tableParser = 
                this.consultations = new Dictionary<string, IEnumerable<Consultation>>{
                    { range, tableParser.ParseTable(values) }
                };
                foreach (var cons in this.consultations[range])
                {
                    Console.WriteLine($"{cons.Subject} - {cons.Teacher} - {cons.Group} - {cons.Time} - {cons.Link}");
                }
            }
            else
            {
                Console.WriteLine("No data found.");
            }
        }

        public IEnumerable<Consultation> GetAllByType(string type)
        {
            return this.consultations[type];
        }
    }
}
