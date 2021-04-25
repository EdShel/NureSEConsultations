using NureSEConsultations.Bot.Constants;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace NureSEConsultations.Bot.Controllers
{
    public class PagesController
    {
        private readonly ITelegramBotClient botClient;

        public PagesController(ITelegramBotClient botClient)
        {
            this.botClient = botClient;
        }

        [Command(Routes.PAGES)]
        public async Task ShowPagesConsultation(CallbackQuery message)
        {
            await this.botClient.AnswerCallbackQueryAsync(
                callbackQueryId: message.Id
            );

            Routes.ParseRouteForPages(message.Data, out string consultationType, out int pagesCount);

            var textMessage = $"Сторінки для <i>{consultationType}</i> {Emoji.NERD_FACE}";
            var buttons = GetPageButtons(consultationType, pagesCount);

            await this.botClient.SendTextMessageAsync(
                chatId: message.Message.Chat,
                text: textMessage,
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                replyMarkup: new InlineKeyboardMarkup(buttons)
            );

            await this.botClient.DeleteMessageAsync(
                message.Message.Chat.Id, message.Message.MessageId
            );
        }

        private static IEnumerable<IEnumerable<InlineKeyboardButton>> GetPageButtons(
            string consultationType, int pagesCount)
        {
            const int buttonsInRow = 5;
            return Enumerable.Range(1, pagesCount)
                .GroupBy(pageNumber => (pageNumber - 1) / buttonsInRow)
                .Select(rowOfPageNumbers => rowOfPageNumbers.Select(pageNumber => new InlineKeyboardButton
                {
                    Text = $"{pageNumber}",
                    CallbackData = Routes.ForConcreteConsultation(consultationType, pageNumber - 1)
                })).ToList();
        }
    }
}
