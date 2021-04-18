using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util.Store;
using NureSEConsultations.Bot.Entities;
using NureSEConsultations.Bot.Parser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace NureSEConsultations.Bot
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            const string spreadsheetUrl = "https://docs.google.com/spreadsheets/d/160pVT-z-OGnpgdlPQFwfJKmsXVb_N1C1NEBTZbcQOGo/edit#gid=886702434";

            const string clientId = "778237814285-s3ku69pua41b4v6fkh4eatg66j4fm4gp.apps.googleusercontent.com";
            const string clientSecret = "nYWD6gXs4weNep5Bqb-eowpR";

            const string credsPath = "tokens.json";
            using var credStream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read);
            var creds = GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.Load(credStream).Secrets,
                new[] { SheetsService.Scope.SpreadsheetsReadonly },
                "user",
                CancellationToken.None,
                new FileDataStore(credsPath, true)
            ).Result;
            Console.WriteLine("Saved creds");

            var sheets = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = creds,
                ApplicationName = nameof(NureSEConsultations)
            });

            String spreadsheetId = "160pVT-z-OGnpgdlPQFwfJKmsXVb_N1C1NEBTZbcQOGo";

            String range = "3 курс, ПЗПІ-18!A6:E";
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
                var consultations = tableParser.ParseTable(values);
                foreach (var cons in consultations)
                {
                    Console.WriteLine($"{cons.Subject} - {cons.Teacher} - {cons.Group} - {cons.Time} - {cons.Link}");
                }
            }
            else
            {
                Console.WriteLine("No data found.");
            }


            //const string token = "1794463891:AAEtc5EU6CkaTxfBuofZBy7EUKGtj9y8G_o";
            //var botClient = new TelegramBotClient(token);
            //var me = await botClient.GetMeAsync();
            //Console.WriteLine($"{me.FirstName}");
        }
    }
}
