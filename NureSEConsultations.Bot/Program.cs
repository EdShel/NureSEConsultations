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

        private static string[] consultationsTypes = {
            "Зустрічі з куратором (весна)",
            "1 курс, ПЗПІ-20",
            "2 курс, ПЗПІ-19",
            "3 курс, ПЗПІ-18",
            "4 курс, ПЗПІ-17",
            "1 курс магістри(5 курс)",
        };

        private static IDictionary<string, IEnumerable<Consultation>> consultations;

        private static ConsultationRepository consultationsRepository;

        private static void ServicesConfiguration()
        {
            var builder = new ServiceCollection();

            builder.AddSingleton<Consultation>();

            services = builder.BuildServiceProvider(
                new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = false}
            );
        }

        public static async Task Main(string[] args)
        {
            ServicesConfiguration();
            //consultationsRepository = new ConsultationRepository(CONSULTATION_SHEET_ID, null);


            const string token = "1794463891:AAEtc5EU6CkaTxfBuofZBy7EUKGtj9y8G_o";
            botClient = new TelegramBotClient(token);
            var me = await botClient.GetMeAsync();
            Console.WriteLine($"{me.FirstName} is running...");

            botClient.OnMessage += OnMessageReceived;
            botClient.StartReceiving();

            Console.WriteLine("Press ENTER to shut down.");
            Console.ReadLine();

            botClient.StopReceiving();
        }

        private static async void OnMessageReceived(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            if (e.Message.Text != null)
            {
                Console.WriteLine($"{e.Message.From.FirstName} {e.Message.From.LastName}: {e.Message.Text}");

                if (e.Message.Text == "Список консультацій")
                {
                    var row = consultationsTypes.Select(type => new InlineKeyboardButton
                    {
                        Text = type,
                        CallbackData = type + "callback"
                    }).ToArray();

                    await botClient.SendTextMessageAsync(
                        chatId: e.Message.Chat,
                        text: "Тримай ;)",
                        replyMarkup: new InlineKeyboardMarkup(
                            inlineKeyboardRow: row
                        )
                    );
                }
                else
                {
                    await botClient.SendTextMessageAsync(
                      chatId: e.Message.Chat,
                      text: "You said:\n" + e.Message.Text,
                      replyMarkup: new ReplyKeyboardMarkup(
                        keyboardRow: new KeyboardButton[]
                        {
                            new KeyboardButton("Список консультацій"),
                            new KeyboardButton("Статистика"),
                        }, resizeKeyboard: true)
                    );
                }

            }
        }
    }
}
