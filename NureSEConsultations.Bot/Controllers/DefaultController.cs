using NureSEConsultations.Bot.Constants;
using NureSEConsultations.Bot.Services;
using NureSEConsultations.Bot.Services.MessageBuilders;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace NureSEConsultations.Bot.Controllers
{
    public class DefaultController
    {
        private readonly ITelegramBotClient botClient;

        private readonly SearchResultHandler searchHandler;

        public DefaultController(ITelegramBotClient botClient, SearchResultHandler searchHandler)
        {
            this.botClient = botClient;
            this.searchHandler = searchHandler;
        }

        [Command(Routes.DEFAULT)]
        public Task HandleNewUser(Message message)
        {
            string searchQuery = message.Text;

            return this.searchHandler.HandleSearchAsync(
                chatId: message.Chat.Id,
                searchQuery: searchQuery,
                pageIndex: 0);
        }
    }
}
