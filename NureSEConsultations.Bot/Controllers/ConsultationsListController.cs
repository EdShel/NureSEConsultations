using System;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace NureSEConsultations.Bot.Controllers
{
    public static class Routes
    {
        public const string CONSULTATIONS_LIST = "Список консультацій";

        public const string STATISTICS = "Статистика";
    }

    public static class Emoji
    {
        public const string SMIRK = "\ud83d\ude0f";
    }

    public class ConsultationsListController
    {
        private static string[] consultationsTypes = {
            "Зустрічі з куратором (весна)",
            "1 курс, ПЗПІ-20",
            "2 курс, ПЗПІ-19",
            "3 курс, ПЗПІ-18",
            "4 курс, ПЗПІ-17",
            "1 курс магістри(5 курс)",
        };

        private readonly ITelegramBotClient botClient;

        public ConsultationsListController(ITelegramBotClient botClient)
        {
            this.botClient = botClient;
        }

        [Command("/меню")]
        public async Task HandleMenu(Message message)
        {
            await this.botClient.SendTextMessageAsync(
                chatId: message.Chat,
                text: "You said:\n" + message.Text,
                replyMarkup: new ReplyKeyboardMarkup(
                keyboardRow: new KeyboardButton[]
                {
                    new KeyboardButton(Routes.CONSULTATIONS_LIST),
                    new KeyboardButton(Routes.STATISTICS),
                }, resizeKeyboard: true)
            );
        }

        [Command(Routes.CONSULTATIONS_LIST)]
        public async Task HandleConsultationsAsync(Message message)
        {
            const int buttonsInRow = 2;
            var rows = Enumerable.Range(0, (int)Math.Ceiling(consultationsTypes.Length / 2d))
                .Select(rowIndex => consultationsTypes
                    .Skip(rowIndex * buttonsInRow)
                    .Take(buttonsInRow)
                    .Select(type => new InlineKeyboardButton
                    {
                        Text = type,
                        CallbackData = "list " + type
                    }));

            await this.botClient.SendTextMessageAsync(
                chatId: message.Chat,
                text: $"Тримай {Emoji.SMIRK}",
                replyMarkup: new InlineKeyboardMarkup(rows)
            );
        }

        [Command(Routes.STATISTICS)]
        public async Task ShowStatistics(Message message)
        {
            await this.botClient.SendTextMessageAsync(
                chatId: message.Chat,
                text: "NO"
            );
        }

        [Command("list")]
        public async Task ShowStatistics(CallbackQuery message)
        {
            await this.botClient.AnswerCallbackQueryAsync(
                callbackQueryId: message.Id
            );

            await this.botClient.SendTextMessageAsync(
                chatId: message.Message.Chat,
                text: $"Тримай {Emoji.SMIRK}"
            );
        }
    }
}
