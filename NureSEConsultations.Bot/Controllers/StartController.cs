using NureSEConsultations.Bot.Constants;
using NureSEConsultations.Bot.Model;
using System.IO;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;
using Emoji = NureSEConsultations.Bot.Constants.Emoji;

namespace NureSEConsultations.Bot.Controllers
{
    public class StartController
    {
        private readonly ITelegramBotClient botClient;

        private readonly RepositoryConfiguration repoConfig;

        public StartController(ITelegramBotClient botClient, RepositoryConfiguration repositoryConfiguration)
        {
            this.botClient = botClient;
            this.repoConfig = repositoryConfiguration;
        }

        [Command(Routes.START)]
        public async Task HandleNewUser(Message message)
        {
            var stickerFileStream = new FileStream(Stickers.GREETINGS, FileMode.Open, FileAccess.Read);
            await this.botClient.SendStickerAsync(
                message.Chat.Id,
                new InputOnlineFile(stickerFileStream)
            );

            await this.botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: $"Привіт! Я бот {Emoji.ROBOT_FACE} для інформування <b>щодо консультацій кафедри ПІ.</b>\n" +
                "Я вмію відображати консультації потоків та зустрічей з кураторами.\n" +
                "Звідки я беру інформацію? " +
                "\n<a href=\"https://docs.google.com/spreadsheets/d/" + this.repoConfig.GoogleSheetId +
                "\">З онлайн таблиці консультацій кафедри.</a>\n" +
                $"А також я вмію в статистику {Emoji.NERD_FACE}",
                parseMode: ParseMode.Html,
                disableWebPagePreview: true,
                replyMarkup: new ReplyKeyboardMarkup(
                    keyboardRow: new KeyboardButton[]
                    {
                        new KeyboardButton(Routes.CONSULTATIONS_LIST),
                        new KeyboardButton(Routes.STATISTICS),
                    }, 
                    resizeKeyboard: true
                )
            );
        }
    }
}
