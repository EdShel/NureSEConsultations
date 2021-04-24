using NureSEConsultations.Bot.Constants;
using NureSEConsultations.Bot.Model;
using System;
using System.Collections.Generic;
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

            string messageText = GetTextMessage(pageContent);

            var keyboardButtons = GetPagesSwitcher(consultationType, pageIndex, pagesCount);

            await this.botClient.SendTextMessageAsync(
                chatId: message.Message.Chat,
                text: messageText,
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                replyMarkup: new InlineKeyboardMarkup(keyboardButtons)
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
