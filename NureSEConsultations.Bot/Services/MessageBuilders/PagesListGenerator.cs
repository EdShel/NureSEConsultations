using System;
using System.Collections.Generic;
using System.Linq;
using Telegram.Bot.Types.ReplyMarkups;

namespace NureSEConsultations.Bot.Services.MessageBuilders
{
    public class PagesListGenerator
    {
        private const int BUTTONS_IN_ROW = 5;

        public IEnumerable<IEnumerable<InlineKeyboardButton>> GetPageButtons(int pagesCount, Func<int, string> routeForPage)
        {
            return Enumerable.Range(1, pagesCount)
                .GroupBy(pageNumber => (pageNumber - 1) / BUTTONS_IN_ROW)
                .Select(rowOfPageNumbers => rowOfPageNumbers.Select(pageNumber => new InlineKeyboardButton
                {
                    Text = $"{pageNumber}",
                    CallbackData = routeForPage(pageNumber - 1)
                })).ToList();
        }
    }
}
