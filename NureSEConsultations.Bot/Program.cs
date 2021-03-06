using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using NureSEConsultations.Bot.Constants;
using NureSEConsultations.Bot.Model;
using NureSEConsultations.Bot.Parser;
using NureSEConsultations.Bot.Services;
using NureSEConsultations.Bot.Services.MessageBuilders;
using System;
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

        private static Router router;

        private static IServiceProvider ServicesConfiguration()
        {
            var builder = new ServiceCollection();

            var appsettings = JObject.Parse(System.IO.File.ReadAllText("appsettings.json"));
            var worksheetConfig = appsettings["ConsultationsParsers"].ToDictionary(
                k => (k as JProperty).Name,
                v => (v as JProperty).Value.ToObject<WorksheetConfig>()
            );
            var repoConfig = new RepositoryConfiguration(
                credentialsFile: "googleCredentials.json",
                tokensTempFile: "tokens.json",
                googleSheetId: appsettings.Value<string>("GoogleSheetId"),
                worksheetConfig: worksheetConfig);

            builder.AddSingleton(repoConfig);
            builder.AddSingleton<IParserResolver, ParserResolver>();
            builder.AddSingleton<ConsultationRepository>();
            builder.AddSingleton<IConsultationRepository, CachingRepository>();

            builder.AddSingleton(new DbContextFactory(appsettings.Value<string>("DatabaseConnectionString")));

            string tempFolderPath = appsettings.Value<string>("VoiceTempFolder");
            builder.AddSingleton<IOggToWavConverter, OggToWavConverter>();
            builder.AddSingleton<ISpeechTranscriptor, GoogleSpeechToText>();
            builder.AddSingleton<IConsultationSearcher, ConsultationSearcher>();
            builder.AddSingleton<ConsultationPageMessageBuilderFactory>();
            builder.AddSingleton<SearchResultMessageBuilderFactory>();
            builder.AddSingleton<PagesListGenerator>();
            builder.AddSingleton<SearchQueryNormalizer>();
            builder.AddSingleton<SearchResultHandler>();

            string telegramBotId = JObject.Parse(
                System.IO.File.ReadAllText("credentials.json"))["TelegramBotId"].Value<string>();
            builder.AddSingleton<ITelegramBotClient>(new TelegramBotClient(telegramBotId));

            // Add controllers
            Router.GetControllerClasses().ToList().ForEach(c => builder.AddSingleton(c));

            return builder.BuildServiceProvider(
                new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = false }
            );
        }

        public static async Task Main(string[] args)
        {
            services = ServicesConfiguration();
            var botClient = services.GetRequiredService<ITelegramBotClient>();

            router = new Router(services, Routes.DEFAULT);

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
            try
            {
                var res = e.CallbackQuery;
                Console.WriteLine($"{res.From.FirstName} {res.From.LastName}: {res.Data}");
                if (e != null)
                {
                    string route = res.Data.Substring(0, res.Data.IndexOf(' '));
                    await router.HandleAsync(route, res);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
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
            string route = e.Message.Text ?? e.Message.Type.ToString();
            await router.HandleAsync(route, e.Message);

            Console.WriteLine($"{e.Message.From.FirstName} {e.Message.From.LastName}: {route}");
        }
    }
}
