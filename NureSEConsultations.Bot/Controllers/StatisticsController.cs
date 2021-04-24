using Microsoft.EntityFrameworkCore;
using NureSEConsultations.Bot.Constants;
using NureSEConsultations.Bot.Model;
using System;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Emoji = NureSEConsultations.Bot.Constants.Emoji;

namespace NureSEConsultations.Bot.Controllers
{
    public class StatisticsController
    {
        private readonly ITelegramBotClient botClient;

        private readonly DbContextFactory dbContextFactory;

        public StatisticsController(ITelegramBotClient botClient, DbContextFactory dbContextFactory)
        {
            this.botClient = botClient;
            this.dbContextFactory = dbContextFactory;
        }

        [Command(Routes.STATISTICS)]
        public async Task ShowStatistics(Message message)
        {
            using var db = await this.dbContextFactory.CreateAsync();

            int usersCount = await db.Users.CountAsync();

            DateTime weekAgo = DateTime.Now.AddDays(-7d);
            int newUsersInWeek = await db.Users.Where(user => user.StartTime >= weekAgo).CountAsync();

            string trendEmoji = newUsersInWeek != 0 
                ? Emoji.CHART_WITH_UPWARDS_TREND 
                : Emoji.CHART_WITH_DOWNWARDS_TREND;

            await this.botClient.SendTextMessageAsync(
                chatId: message.Chat,
                text: $"Усього {usersCount} користувачів {Emoji.COUPLE}\n" +
                      $"За останній тиждень {newUsersInWeek} нових користувачів {trendEmoji}"
            );
        }
    }
}
