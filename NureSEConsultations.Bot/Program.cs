using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using NureSEConsultations.Bot.Model;
using NureSEConsultations.Bot.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;

namespace NureSEConsultations.Bot
{
    public class Program
    {
        private static IServiceProvider services;

        private static ITelegramBotClient botClient;

        private const string CONSULTATION_SHEET_ID = "";

        private static Router router;


        private static IServiceProvider ServicesConfiguration()
        {
            var builder = new ServiceCollection();

            var appsettings = JObject.Parse(System.IO.File.ReadAllText("appsettings.json"));
            var repoConfig = new RepositoryConfiguration(
                credentialsFile: "credentials.json",
                tokensTempFile: "tokens.json",
                googleSheetId: appsettings["GoogleSheetsId"].Value<string>(),
                worksheetParserName: appsettings["ConsultationsParsers"].ToDictionary(
                    j => (j as JProperty).Name, 
                    j => (j as JProperty).Value<string>()));

            builder.AddSingleton(repoConfig);
            builder.AddSingleton<ConsultationRepository>();

            builder.AddSingleton<IParserResolver, ParserResolver>();
            builder.AddSingleton(botClient);

            // Add controllers
            Router.GetControllerClasses().ToList().ForEach(c => builder.AddSingleton(c));

            return builder.BuildServiceProvider(
                new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = false }
            );
        }

        public static async Task Main(string[] args)
        {
            const string token = "1794463891:AAEtc5EU6CkaTxfBuofZBy7EUKGtj9y8G_o";
            botClient = new TelegramBotClient(token);

            services = ServicesConfiguration();
            router = new Router(services, "/меню");

            var me = await botClient.GetMeAsync();
            Console.WriteLine($"{me.FirstName} is running...");

            botClient.OnMessage += OnMessageReceived;
            botClient.OnInlineQuery += OnInlineQuery;
            botClient.OnInlineResultChosen += OnInlineResultChosen; ;
            botClient.OnCallbackQuery += BotClient_OnCallbackQuery;
            botClient.StartReceiving();

            Console.WriteLine("Press ENTER to shut down.");
            Console.ReadLine();

            botClient.StopReceiving();
        }

        private static async void BotClient_OnCallbackQuery(object sender, CallbackQueryEventArgs e)
        {
            var res = e.CallbackQuery;
            if (e != null)
            {
                string route = res.Data.Substring(0, res.Data.IndexOf(' '));
                await router.HandleAsync(route, res);
            }
            Console.WriteLine($"{res.From.FirstName} {res.From.LastName}: {res.Message} {res.Data}");
        }

        private static async void OnInlineResultChosen(object sender, ChosenInlineResultEventArgs e)
        {
            ChosenInlineResult res = e.ChosenInlineResult;
            if (res != null)
            {
                await router.HandleAsync(res.Query, res);
            }
            Console.WriteLine($"{res.From.FirstName} {res.From.LastName}: {res.Query}");
        }

        private static async void OnInlineQuery(object sender, InlineQueryEventArgs e)
        {
            if (e.InlineQuery != null)
            {
                await router.HandleAsync(e.InlineQuery.Query, e.InlineQuery);
            }
            Console.WriteLine($"{e.InlineQuery.From.FirstName} {e.InlineQuery.From.LastName}: {e.InlineQuery.Query}");
        }

        private static async void OnMessageReceived(object sender, MessageEventArgs e)
        {
            if (e.Message.Text != null)
            {
                await router.HandleAsync(e.Message.Text, e.Message);
            }
            Console.WriteLine($"{e.Message.From.FirstName} {e.Message.From.LastName}: {e.Message.Text}");
        }
    }
}
