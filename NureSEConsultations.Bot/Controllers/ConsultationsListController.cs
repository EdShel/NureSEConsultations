using NureSEConsultations.Bot.Model;
using System;
using System.Linq;
using System.Text;
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

        private readonly ConsultationRepository consultationRepository;

        public ConsultationsListController(ITelegramBotClient botClient, ConsultationRepository consultationRepository)
        {
            this.botClient = botClient;
            this.consultationRepository = consultationRepository;
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

            string consultationType = message.Data.Substring("list ".Length);
            var consultations = consultationRepository.GetAllByType(consultationType).OrderBy(r => new Random().Next()).Take(10);

            var sb = new StringBuilder($"Тримай {Emoji.SMIRK}");
            foreach(var cons in consultations)
            {
                sb.AppendLine();
                sb.Append($"<b>{cons.Subject}</b> {cons.Teacher} - {cons.Group} - {cons.Time}");
            }

            await this.botClient.SendTextMessageAsync(
                chatId: message.Message.Chat,
                text: sb.ToString(),
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Html
            );
        }
    }
}
