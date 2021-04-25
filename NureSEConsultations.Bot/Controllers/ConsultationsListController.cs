using NureSEConsultations.Bot.Constants;
using NureSEConsultations.Bot.Model;
using System;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace NureSEConsultations.Bot.Controllers
{
    public class ConsultationsListController
    {
        private readonly ITelegramBotClient botClient;

        private readonly IConsultationRepository consultationRepository;

        public ConsultationsListController(ITelegramBotClient botClient, IConsultationRepository consultationRepository)
        {
            this.botClient = botClient;
            this.consultationRepository = consultationRepository;
        }

        [Command(Routes.CONSULTATIONS_LIST)]
        public async Task HandleConsultationsAsync(Message message)
        {
            var consultationsNames = this.consultationRepository.GetConsultationsNames();
            const int buttonsInRow = 2;
            var rows = Enumerable.Range(0, (int)Math.Ceiling(consultationsNames.Count() / 2d))
                .Select(rowIndex => consultationsNames
                    .Skip(rowIndex * buttonsInRow)
                    .Take(buttonsInRow)
                    .Select(type => new InlineKeyboardButton
                    {
                        Text = type,
                        CallbackData = Routes.ForConcreteConsultation(type, 0)
                    }));

            await this.botClient.SendTextMessageAsync(
                chatId: message.Chat,
                text: $"Тримай {Emoji.SMIRK}",
                replyMarkup: new InlineKeyboardMarkup(rows)
            );
        }
    }
}
