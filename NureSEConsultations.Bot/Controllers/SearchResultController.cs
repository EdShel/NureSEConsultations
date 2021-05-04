using NureSEConsultations.Bot.Constants;
using NureSEConsultations.Bot.Services;
using NureSEConsultations.Bot.Services.MessageBuilders;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace NureSEConsultations.Bot.Controllers
{
    public class SearchResultController
    {
        private readonly ITelegramBotClient botClient;

        private readonly IConsultationSearcher searcher;

        private readonly SearchResultMessageBuilderFactory messageBuilderFactory;

        public SearchResultController(
            ITelegramBotClient botClient,
            IConsultationSearcher searcher,
            SearchResultMessageBuilderFactory messageBuilderFactory)
        {
            this.botClient = botClient;
            this.searcher = searcher;
            this.messageBuilderFactory = messageBuilderFactory;
        }

        [Command(Routes.SEARCH_RESULT)]
        public async Task ShowSearchResult(CallbackQuery message)
        {
            Routes.ParseForSearchResult(message.Data, out string searchQuery, out int pageIndex);

            const int pageSize = 10;
            var allFoundItems = this.searcher.Search(searchQuery);
            int pagesCount = (int)Math.Ceiling((double)allFoundItems.Count() / pageSize);
            var currentPage = allFoundItems
                .Skip(pageIndex * pageSize)
                .Take(pageSize);

            var messageBuilder = this.messageBuilderFactory.Create(searchQuery, currentPage, pageIndex, pagesCount);

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

        private string GetTextMessage(IPaginatedMessageBuilder builder)
        {
            var sb = new StringBuilder($"Ось, що я знайшов {Emoji.MAG_RIGHT}");
            builder.AppendMessage(sb);
            return sb.ToString();
        }
    }
}
