using System.Collections.Generic;
using System.Linq;

namespace NureSEConsultations.Bot.Parser
{
    public class ConsultationTimeRowsExtender : IRowsExtender
    {
        private readonly int timeColumnIndex;

        public ConsultationTimeRowsExtender(int timeColumnIndex)
        {
            this.timeColumnIndex = timeColumnIndex;
        }

        public IEnumerable<IList<string>> SplitRowOnManyRows(IList<string> initialRow)
        {
            string timeValue = initialRow[timeColumnIndex];
            var uniqueTimeRecords = timeValue.SplitByNewLine()
                .SelectMany(time => time.Split(';'))
                .Select(time => time.Trim())
                .Where(time => !string.IsNullOrWhiteSpace(time))
                .DefaultIfEmpty(string.Empty);
            return uniqueTimeRecords.Select(time =>
            {
                var rowCopy = initialRow.ToList();
                rowCopy[timeColumnIndex] = time;
                return rowCopy;
            });
        }
    }
}
