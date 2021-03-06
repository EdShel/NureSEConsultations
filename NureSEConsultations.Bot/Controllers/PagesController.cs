using NureSEConsultations.Bot.Constants;
using NureSEConsultations.Bot.Services.MessageBuilders;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace NureSEConsultations.Bot.Controllers
{
    public class PagesController
    {
        private readonly ITelegramBotClient botClient;

        private readonly PagesListGenerator pagesListGenerator;

        public PagesController(ITelegramBotClient botClient, PagesListGenerator pagesListGenerator)
        {
            this.botClient = botClient;
            this.pagesListGenerator = pagesListGenerator;
        }

        [Command(Routes.PAGES)]
        public async Task ShowPagesConsultation(CallbackQuery message)
        {
            await this.botClient.AnswerCallbackQueryAsync(
                callbackQueryId: message.Id
            );

            Routes.ParseRouteForPages(message.Data, out string consultationType, out int pagesCount);

            var textMessage = $"Сторінки для <i>{consultationType}</i> {Emoji.NERD_FACE}";
            var buttons = this.pagesListGenerator.GetPageButtons(
                pagesCount: pagesCount,
                routeForPage: pageIndex => Routes.ForConcreteConsultation(consultationType, pageIndex));

            await this.botClient.SendTextMessageAsync(
                chatId: message.Message.Chat,
                text: textMessage,
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                replyMarkup: new InlineKeyboardMarkup(buttons)
            );

            await this.botClient.DeleteMessageAsync(
                message.Message.Chat.Id, message.Message.MessageId
            );
        }
    }
}
