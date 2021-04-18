using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util.Store;
using Microsoft.Extensions.DependencyInjection;
using NureSEConsultations.Bot.Model;
using NureSEConsultations.Bot.Parser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace NureSEConsultations.Bot
{
    public class Program
    {
        private static IServiceProvider services;

        private static ITelegramBotClient botClient;

        private const string CONSULTATION_SHEET_ID = "160pVT-z-OGnpgdlPQFwfJKmsXVb_N1C1NEBTZbcQOGo";

        private static Router router;

        
        private static IServiceProvider ServicesConfiguration()
        {
            var builder = new ServiceCollection();

            builder.AddSingleton<Consultation>();
            builder.AddSingleton(botClient);
            builder.AddSingleton(new ConsultationRepository(CONSULTATION_SHEET_ID, null));

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
            botClient.StartReceiving();

            Console.WriteLine("Press ENTER to shut down.");
            Console.ReadLine();

            botClient.StopReceiving();
        }

        private static async void OnInlineQuery(object sender, Telegram.Bot.Args.InlineQueryEventArgs e)
        {
            if (e.InlineQuery != null)
            {
                await router.HandleAsync(e.InlineQuery.Query, e.InlineQuery);
            }
            Console.WriteLine($"{e.InlineQuery.From.FirstName} {e.InlineQuery.From.LastName}: {e.InlineQuery.Query}");
        }

        private static async void OnMessageReceived(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            if (e.Message.Text != null)
            {
                await router.HandleAsync(e.Message.Text, e.Message);
            }
            Console.WriteLine($"{e.Message.From.FirstName} {e.Message.From.LastName}: {e.Message.Text}");
        }
    }
}
