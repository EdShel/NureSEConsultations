using NureSEConsultations.Bot.Constants;
using NureSEConsultations.Bot.Model;
using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot.Types.ReplyMarkups;

namespace NureSEConsultations.Bot.Services
{
    public class ConsultationPageMessageBuilder : IPaginatedMessageBuilder
    {
        private readonly IEnumerable<Consultation> pageContent;

        private readonly int pageIndex;

        private readonly int pagesCount;

        private readonly Func<int, string> routeToViewPage;

        private readonly Func<int, string> routeToPageSelect;

        public ConsultationPageMessageBuilder(
            IEnumerable<Consultation> consultations,
            int pageIndex,
            int pagesCount,
            Func<int, string> routeToViewPage,
            Func<int, string> routeToPageSelect)
        {
            this.pageContent = consultations;
            this.pageIndex = pageIndex;
            this.pagesCount = pagesCount;
            this.routeToViewPage = routeToViewPage;
            this.routeToPageSelect = routeToPageSelect;
        }

        public StringBuilder AppendMessage(StringBuilder sb)
        {
            foreach (var cons in this.pageContent)
            {
                sb.AppendLine();
                sb.Append($"<b>{cons.Subject}</b> <i>{cons.Teacher}</i> <u>{cons.Group}</u> <b>{cons.Time}</b>");
            }
            return sb;
        }

        public IEnumerable<InlineKeyboardButton> GetPagesSwitcher()
        {
            var keyboardButtons = new List<InlineKeyboardButton>();
            if (this.pageIndex != 0)
            {
                keyboardButtons.Add(GetPreviousPageButton());
            }

            keyboardButtons.Add(GetAllPagesButton());

            if (this.pageIndex < this.pagesCount - 1)
            {
                keyboardButtons.Add(GetNextPageButton());
            }

            return keyboardButtons;
        }

        private InlineKeyboardButton GetPreviousPageButton()
        {
            return new InlineKeyboardButton
            {
                Text = $"{Emoji.ARROW_BACKWARD} {this.pageIndex}",
                CallbackData = this.routeToViewPage(this.pageIndex - 1)
            };
        }

        private InlineKeyboardButton GetNextPageButton()
        {
            return new InlineKeyboardButton
            {
                Text = $"{this.pageIndex + 2} {Emoji.ARROW_FORWARD}",
                CallbackData = this.routeToViewPage(this.pageIndex + 1)
            };
        }

        private InlineKeyboardButton GetAllPagesButton()
        {
            return new InlineKeyboardButton
            {
                Text = $"{Emoji.HASH} Усі сторінки",
                CallbackData = this.routeToPageSelect(this.pagesCount)
            };
        }

    }
}
