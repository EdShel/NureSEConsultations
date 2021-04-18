using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;

namespace NureSEConsultations.Bot.Controllers
{
    public class ConsultationsController
    {
        private readonly ITelegramBotClient botClient;

        public ConsultationsController(ITelegramBotClient botClient)
        {
            this.botClient = botClient;
        }

        [Command("/list")]
        public async Task HandleConsultationsAsync()
        {

        }
    }
}
