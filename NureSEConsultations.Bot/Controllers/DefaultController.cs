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

        private int stickerCounter;

        public DefaultController(ITelegramBotClient botClient, SearchResultHandler searchHandler)
        {
            this.botClient = botClient;
            this.searchHandler = searchHandler;
        }

        [Command(Routes.DEFAULT)]
        public Task HandleNewUser(Message message)
        {
            //// Actually, race conditions does not matter 
            //string stickerName = this.stickerCounter++ % 2 == 0
            //    ? Stickers.DO_NOT_UNDERSTAND0
            //    : Stickers.DO_NOT_UNDERSTAND1;
            //var stickerFileStream = new FileStream(stickerName, FileMode.Open, FileAccess.Read);
            //await this.botClient.SendStickerAsync(
            //    message.Chat.Id,
            //    new InputOnlineFile(stickerFileStream)
            //);

            string searchQuery = message.Text;

            return this.searchHandler.HandleSearchAsync(
                chatId: message.Chat.Id,
                searchQuery: searchQuery,
                pageIndex: 0);
        }
    }
}
