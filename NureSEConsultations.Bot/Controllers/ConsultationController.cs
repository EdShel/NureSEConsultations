using NAudio.Wave;
using NureSEConsultations.Bot.Constants;
using NureSEConsultations.Bot.Model;
using NureSEConsultations.Bot.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;
using File = System.IO.File;

namespace NureSEConsultations.Bot.Controllers
{
    public class VoiceSearchController
    {
        private const int MAX_VOICE_LENGTH_SECONDS = 5;

        private readonly ITelegramBotClient botClient;

        private readonly IConsultationRepository consultationRepository;

        private readonly ITempFileProvider tempFileProvider;

        public VoiceSearchController(ITelegramBotClient botClient, IConsultationRepository consultationRepository, ITempFileProvider tempFileProvider)
        {
            this.botClient = botClient;
            this.consultationRepository = consultationRepository;
            this.tempFileProvider = tempFileProvider;
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

            var oggFilePath = Path.Combine(@"C:\Users\Admin\Desktop\Audio", Guid.NewGuid() + ".ogg");
            File.WriteAllBytes(oggFilePath, ogg.ToArray());

            using var wav = new MemoryStream();
            ogg.Position = 0;
            new OggToWavConverter().Convert(ogg, wav);

            var filePathOnDisk = Path.Combine(@"C:\Users\Admin\Desktop\Audio", Guid.NewGuid() + ".wav");
            await File.WriteAllBytesAsync(filePathOnDisk, wav.ToArray());

            wav.Position = 0;
            await botClient.SendVoiceAsync(
                chatId: message.Chat.Id,
                voice: new InputOnlineFile(wav)
            );
        }
    }

    public class ConsultationController
    {
        private readonly ITelegramBotClient botClient;

        private readonly IConsultationRepository consultationRepository;

        public ConsultationController(ITelegramBotClient botClient, IConsultationRepository consultationRepository)
        {
            this.botClient = botClient;
            this.consultationRepository = consultationRepository;
        }

        [Command(Routes.CONCRETE_CONSULTATION)]
        public async Task ShowConsultation(CallbackQuery message)
        {
            await this.botClient.AnswerCallbackQueryAsync(
                callbackQueryId: message.Id
            );

            Routes.ParseForConcreteConsultation(message.Data, out string consultationType, out int pageIndex);

            const int pageSize = 10;
            var allConsultations = this.consultationRepository.GetAllByType(consultationType);
            int consultationsCount = allConsultations.Count();
            int pagesCount = (int)Math.Ceiling((double)consultationsCount / pageSize);
            var pageContent = allConsultations.Skip(pageIndex * pageSize).Take(pageSize);

            var keyboardButtons = GetPagesSwitcher(consultationType, pageIndex, pagesCount);
            string messageText = GetTextMessage(pageContent);

            await this.botClient.SendTextMessageAsync(
                chatId: message.Message.Chat,
                text: messageText,
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                disableWebPagePreview: true,
                replyMarkup: new InlineKeyboardMarkup(keyboardButtons)
            );

            await this.botClient.DeleteMessageAsync(
                message.Message.Chat.Id, message.Message.MessageId
            );
        }

        private static List<InlineKeyboardButton> GetPagesSwitcher(string consultationType, int pageIndex, int pagesCount)
        {
            var keyboardButtons = new List<InlineKeyboardButton>();
            if (pageIndex != 0)
            {
                keyboardButtons.Add(GetPreviousPageButton(consultationType, pageIndex));
            }

            keyboardButtons.Add(GetAllPagesButton(consultationType, pagesCount));

            if (pageIndex < pagesCount - 1)
            {
                keyboardButtons.Add(GetNextPageButton(consultationType, pageIndex));
            }

            return keyboardButtons;
        }

        private static InlineKeyboardButton GetPreviousPageButton(string consultationType, int pageIndex)
        {
            return new InlineKeyboardButton
            {
                Text = $"{Emoji.ARROW_BACKWARD} {pageIndex}",
                CallbackData = Routes.ForConcreteConsultation(consultationType, pageIndex - 1)
            };
        }

        private static InlineKeyboardButton GetNextPageButton(string consultationType, int pageIndex)
        {
            return new InlineKeyboardButton
            {
                Text = $"{pageIndex + 2} {Emoji.ARROW_FORWARD}",
                CallbackData = Routes.ForConcreteConsultation(consultationType, pageIndex + 1)
            };
        }

        private static InlineKeyboardButton GetAllPagesButton(string consultationType, int pagesCount)
        {
            return new InlineKeyboardButton
            {
                Text = $"{Emoji.HASH} Усі сторінки",
                CallbackData = Routes.RouteForPages(consultationType, pagesCount)
            };
        }

        private static string GetTextMessage(IEnumerable<Consultation> pageContent)
        {
            var sb = new StringBuilder($"Тримай {Emoji.SMIRK}");
            foreach (var cons in pageContent)
            {
                sb.AppendLine();
                sb.Append($"<b>{cons.Subject}</b> <i>{cons.Teacher}</i> <u>{cons.Group}</u> <b>{cons.Time}</b>");
            }
            string messageText = sb.ToString();
            return messageText;
        }
    }
}
