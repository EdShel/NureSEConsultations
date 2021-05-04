using NureSEConsultations.Bot.Constants;
using NureSEConsultations.Bot.Services.MessageBuilders;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace NureSEConsultations.Bot.Controllers
{
    public class SearchResultController
    {
        private readonly ITelegramBotClient botClient;

        private readonly SearchResultHandler searchHandler;

        public SearchResultController(ITelegramBotClient botClient, SearchResultHandler searchHandler)
        {
            this.botClient = botClient;
            this.searchHandler = searchHandler;
        }

        [Command(Routes.SEARCH_RESULT)]
        public async Task ShowSearchResult(CallbackQuery message)
        {
            Routes.ParseForSearchResult(message.Data, out string searchQuery, out int pageIndex);

            await this.searchHandler.HandleSearchAsync(
                chatId: message.Message.Chat.Id,
                searchQuery: searchQuery,
                pageIndex: pageIndex
            );

            await this.botClient.DeleteMessageAsync(
                message.Message.Chat.Id, message.Message.MessageId
            );
        }
    }
}
