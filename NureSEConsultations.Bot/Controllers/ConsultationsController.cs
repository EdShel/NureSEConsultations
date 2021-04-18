using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace NureSEConsultations.Bot.Controllers
{
    public class ConsultationsController
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

        public ConsultationsController(ITelegramBotClient botClient)
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
                    new KeyboardButton("Список консультацій"),
                    new KeyboardButton("Статистика"),
                }, resizeKeyboard: true)
            );
        }

        [Command("Список консультацій")]
        public async Task HandleConsultationsAsync(Message message)
        {
            var row = consultationsTypes.Select(type => new InlineKeyboardButton
            {
                Text = type,
                CallbackData = type + "callback"
            }).ToArray();

            await this.botClient.SendTextMessageAsync(
                chatId: message.Chat,
                text: "Тримай ;)",
                replyMarkup: new InlineKeyboardMarkup(
                    inlineKeyboardRow: row
                )
            );
        }
    }
}
