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
    public class Consultation
    {
        public string Subject { get; set; }

        public string Teacher { get; set; }

        public string Group { get; set; }

        public string Time { get; set; }

        public string Link { get; set; }
    }

    public class ConsultationRepository
    {
        private readonly IDictionary<string, IEnumerable<Consultation>> consultations;

        public ConsultationRepository(string spreadsheetId, IDictionary<string, TableParser<Consultation>> parsers)
        {
            const string credsPath = "tokens.json";
            using var credStream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read);
            var creds = GoogleWebAuthorizationBroker.AuthorizeAsync(
                clientSecrets: GoogleClientSecrets.Load(credStream).Secrets,
                scopes: new[] { SheetsService.Scope.SpreadsheetsReadonly },
                user: "user",
                taskCancellationToken: CancellationToken.None,
                dataStore: new FileDataStore(credsPath, true)
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
                //tableParser = new TableParser<Consultation>(new CuratorMeetingConsultation("Зустріч з куратором"))
                //{
                //    RowExtenders = new IRowsExtender[] { new GroupNameRowsExtender(2), new ConsultationTimeRowsExtender(3) },
                //};
                tableParser = new TableParser<Consultation>(new SubjectConsultationMapper())
                {
                    RowExtenders = new IRowsExtender[] { new GroupNameRowsExtender(2), new ConsultationTimeRowsExtender(3) },
                };
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
