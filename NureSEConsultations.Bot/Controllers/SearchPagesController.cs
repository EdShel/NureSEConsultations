using NureSEConsultations.Bot.Constants;
using NureSEConsultations.Bot.Services.MessageBuilders;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace NureSEConsultations.Bot.Controllers
{
    public class SearchPagesController
    {
        private readonly ITelegramBotClient botClient;

        private readonly PagesListGenerator pagesListGenerator;

        public SearchPagesController(ITelegramBotClient botClient, PagesListGenerator pagesListGenerator)
        {
            this.botClient = botClient;
            this.pagesListGenerator = pagesListGenerator;
        }

        [Command(Routes.SEARCH_PAGES)]
        public async Task ShowSearchPages(CallbackQuery message)
        {
            await this.botClient.AnswerCallbackQueryAsync(
                callbackQueryId: message.Id
            );

            Routes.ParseForSearchPages(message.Data, out string searchQuery, out int pagesCount);

            var textMessage = $"Для <i>{searchQuery}</i> знайшов {pagesCount} сторінок {Emoji.NERD_FACE}";
            var buttons = pagesListGenerator.GetPageButtons(
                pagesCount: pagesCount,
                routeForPage: pageIndex => Routes.ForSearchResult(searchQuery, pageIndex));

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
    }
}
