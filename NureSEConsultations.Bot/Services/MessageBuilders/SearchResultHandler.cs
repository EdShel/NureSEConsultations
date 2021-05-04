using NureSEConsultations.Bot.Constants;
using NureSEConsultations.Bot.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace NureSEConsultations.Bot.Services.MessageBuilders
{
    public class SearchResultHandler
    {
        private const int PAGE_SIZE = 10;

        private readonly ITelegramBotClient botClient;

        private readonly IConsultationSearcher searcher;

        private readonly SearchResultMessageBuilderFactory messageBuilderFactory;

        private readonly SearchQueryNormalizer searchQueryNormalizer;

        private int stickerCounter;

        public SearchResultHandler(
            ITelegramBotClient botClient,
            IConsultationSearcher searcher,
            SearchResultMessageBuilderFactory messageBuilderFactory,
            SearchQueryNormalizer searchQueryNormalizer)
        {
            this.botClient = botClient;
            this.searcher = searcher;
            this.messageBuilderFactory = messageBuilderFactory;
            this.searchQueryNormalizer = searchQueryNormalizer;
        }

        public async Task HandleSearchAsync(long chatId, string searchQuery, int pageIndex)
        {
            var normalizedQuery = this.searchQueryNormalizer.NormalizeStrict(searchQuery);
            if (string.IsNullOrWhiteSpace(normalizedQuery))
            {
                await HandleUglySearch(chatId);
                return;
            }

            var allFoundItems = this.searcher.Search(normalizedQuery);
            if (!allFoundItems.Any())
            {
                normalizedQuery = this.searchQueryNormalizer.NormalizeFuzzy(normalizedQuery);
                allFoundItems = this.searcher.Search(normalizedQuery);
            }

            if (!allFoundItems.Any())
            {
                await HandleEmptySearchResult(chatId, searchQuery);
                return;
            }

            await HandleFoundSearchResults(chatId, searchQuery, pageIndex, allFoundItems);
        }

        private async Task HandleUglySearch(long chatId)
        {
            // Actually, race conditions does not matter 
            string stickerName = this.stickerCounter++ % 2 == 0
                ? Stickers.DO_NOT_UNDERSTAND0
                : Stickers.DO_NOT_UNDERSTAND1;
            var stickerFileStream = new FileStream(stickerName, FileMode.Open, FileAccess.Read);
            await this.botClient.SendStickerAsync(
                chatId: chatId,
                sticker: new InputOnlineFile(stickerFileStream)
            );
        }

        private async Task HandleEmptySearchResult(long chatId, string searchQuery)
        {
            await this.botClient.SendTextMessageAsync(
                chatId: chatId,
                text: Emoji.SCREAM_CAT
            );

            await this.botClient.SendTextMessageAsync(
                chatId: chatId,
                text: $"На жаль, немає нічого схожого на <i>{searchQuery}</>.",
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Html
            );
        }

        private async Task HandleFoundSearchResults(long chatId, string searchQuery, int pageIndex, IEnumerable<Consultation> allFoundItems)
        {
            int pagesCount = (int)Math.Ceiling((double)allFoundItems.Count() / PAGE_SIZE);
            var currentPage = allFoundItems
                .Skip(pageIndex * PAGE_SIZE)
                .Take(PAGE_SIZE);

            var messageBuilder = this.messageBuilderFactory.Create(searchQuery, currentPage, pageIndex, pagesCount);

            var text = GetTextMessage(messageBuilder);
            var keyboard = messageBuilder.GetPagesSwitcher();

            await this.botClient.SendTextMessageAsync(
                chatId: chatId,
                text: text,
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                disableWebPagePreview: true,
                replyMarkup: new InlineKeyboardMarkup(keyboard)
            );
        }

        private string GetTextMessage(IPaginatedMessageBuilder builder)
        {
            var sb = new StringBuilder($"Ось, що знайшов {Emoji.MAG_RIGHT}");
            builder.AppendMessage(sb);
            return sb.ToString();
        }
    }
}
