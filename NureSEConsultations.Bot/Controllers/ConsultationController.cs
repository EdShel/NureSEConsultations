using NAudio.Wave;
using NureSEConsultations.Bot.Constants;
using NureSEConsultations.Bot.Model;
using NureSEConsultations.Bot.Services;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace NureSEConsultations.Bot.Controllers
{
    public class ConsultationController
    {
        private readonly ITelegramBotClient botClient;

        private readonly IConsultationRepository consultationRepository;

        private readonly ConsultationPageMessageBuilderFactory messageBuilderFactory;

        public ConsultationController(
            ITelegramBotClient botClient,
            IConsultationRepository consultationRepository,
            ConsultationPageMessageBuilderFactory messageBuilderFactory)
        {
            this.botClient = botClient;
            this.consultationRepository = consultationRepository;
            this.messageBuilderFactory = messageBuilderFactory;
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

            var messageBuilder = this.messageBuilderFactory.Create(
                consultationType: consultationType,
                consultations: pageContent,
                pageIndex: pageIndex,
                pagesCount: pagesCount);

            var text = GetTextMessage(messageBuilder);
            var keyboard = messageBuilder.GetPagesSwitcher();

            await this.botClient.SendTextMessageAsync(
                chatId: message.Message.Chat,
                text: text,
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                disableWebPagePreview: true,
                replyMarkup: new InlineKeyboardMarkup(keyboard)
            );

            await this.botClient.DeleteMessageAsync(
                message.Message.Chat.Id, message.Message.MessageId
            );
        }

        private static string GetTextMessage(IPaginatedMessageBuilder messageBuilder)
        {
            var sb = new StringBuilder($"Тримай {Emoji.SMIRK}");
            messageBuilder.AppendMessage(sb);
            return sb.ToString();
        }
    }
}
