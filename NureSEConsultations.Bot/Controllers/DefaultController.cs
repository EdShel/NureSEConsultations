using NureSEConsultations.Bot.Constants;
using NureSEConsultations.Bot.Services;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;

namespace NureSEConsultations.Bot.Controllers
{
    public class DefaultController
    {
        private readonly ITelegramBotClient botClient;

        private readonly IConsultationSearcher searcher;

        private int stickerCounter;

        public DefaultController(ITelegramBotClient botClient, IConsultationSearcher searcher)
        {
            this.botClient = botClient;
            this.searcher = searcher;
        }

        [Command(Routes.DEFAULT)]
        public async Task HandleNewUser(Message message)
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

            var c = searcher.Search(message.Text).ToArray();
            await botClient.SendTextMessageAsync(message.Chat.Id, "Count " + c.Length + string.Concat(c.Take(10).Select(c => "\n " + c.Teacher).ToArray()));
        }
    }
}
