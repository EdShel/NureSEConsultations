using NureSEConsultations.Bot.Constants;
using System.IO;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;

namespace NureSEConsultations.Bot.Controllers
{
    public class DefaultController
    {
        private readonly ITelegramBotClient botClient;

        private int stickerCounter;

        public DefaultController(ITelegramBotClient botClient)
        {
            this.botClient = botClient;
        }

        [Command(Routes.DEFAULT)]
        public async Task HandleNewUser(Message message)
        {
            // Actually, race conditions does not matter 
            string stickerName = this.stickerCounter++ % 2 == 0
                ? Stickers.DO_NOT_UNDERSTAND0
                : Stickers.DO_NOT_UNDERSTAND1;
            var stickerFileStream = new FileStream(stickerName, FileMode.Open, FileAccess.Read);
            await this.botClient.SendStickerAsync(
                message.Chat.Id,
                new InputOnlineFile(stickerFileStream)
            );
        }
    }
}
