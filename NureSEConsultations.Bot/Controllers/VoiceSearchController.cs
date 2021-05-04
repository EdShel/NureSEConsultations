using NureSEConsultations.Bot.Constants;
using NureSEConsultations.Bot.Services;
using System.IO;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace NureSEConsultations.Bot.Controllers
{
    public class VoiceSearchController
    {
        private const int MAX_VOICE_LENGTH_SECONDS = 5;

        private readonly ITelegramBotClient botClient;

        private readonly IOggToWavConverter oggToWavConverter;

        private readonly ISpeechTranscriptor speechTranscriptor;

        public VoiceSearchController(ITelegramBotClient botClient, IOggToWavConverter oggToWavConverter, ISpeechTranscriptor speechTranscriptor)
        {
            this.botClient = botClient;
            this.oggToWavConverter = oggToWavConverter;
            this.speechTranscriptor = speechTranscriptor;
        }

        [Command(Routes.VOICE_SEARCH)]
        public async Task SearchByVoice(Message message)
        {
            if (message.Voice.Duration > MAX_VOICE_LENGTH_SECONDS)
            {
                await this.botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: $"Занадто довге повідомлення {Emoji.HEAR_NO_EVIL}. " +
                          $"Спробуй вкластися у {MAX_VOICE_LENGTH_SECONDS} секунд."
                );
                return;
            }

            string voiceId = message.Voice.FileId;
            var voiceFileInfo = await this.botClient.GetFileAsync(voiceId);
            using var ogg = new MemoryStream(voiceFileInfo.FileSize);
            await this.botClient.DownloadFileAsync(voiceFileInfo.FilePath, ogg);
            ogg.Position = 0;

            using var wav = new MemoryStream();
            this.oggToWavConverter.Convert(ogg, wav);
            wav.Position = 0;

            string voiceText = await this.speechTranscriptor.TranscriptAsync(wav);

            await this.botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: voiceText
            );
        }
    }
}
