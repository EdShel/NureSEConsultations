using System.Collections.Generic;
using System.Text;
using Telegram.Bot.Types.ReplyMarkups;

namespace NureSEConsultations.Bot.Services.MessageBuilders
{
    public interface IPaginatedMessageBuilder
    {
        StringBuilder AppendMessage(StringBuilder sb);

        IEnumerable<InlineKeyboardButton> GetPagesSwitcher();
    }
}
