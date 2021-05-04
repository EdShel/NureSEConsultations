﻿using NureSEConsultations.Bot.Constants;
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
    public class DefaultController
    {
        private readonly ITelegramBotClient botClient;

        private readonly IConsultationSearcher searcher;

        private readonly SearchResultMessageBuilderFactory messageBuilderFactory;

        private int stickerCounter;

        public DefaultController(ITelegramBotClient botClient, IConsultationSearcher searcher, SearchResultMessageBuilderFactory messageBuilderFactory)
        {
            this.botClient = botClient;
            this.searcher = searcher;
            this.messageBuilderFactory = messageBuilderFactory;
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

            string searchQuery = message.Text;

            const int pageSize = 10;
            const int pageIndex = 0;
            var allFoundItems = this.searcher.Search(searchQuery);
            int pagesCount = (int)Math.Ceiling((double)allFoundItems.Count() / pageSize);
            var currentPage = allFoundItems
                .Skip(pageIndex * pageSize)
                .Take(pageSize);

            var messageBuilder = this.messageBuilderFactory.Create(searchQuery, currentPage, pageIndex, pagesCount);

            var text = GetTextMessage(messageBuilder);
            var keyboard = messageBuilder.GetPagesSwitcher();

            await this.botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: text,
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                disableWebPagePreview: true,
                replyMarkup: new InlineKeyboardMarkup(keyboard)
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
