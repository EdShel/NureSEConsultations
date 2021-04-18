using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace NureSEConsultations.Bot.Parser
{
    public class GroupNameRowsExtender : IRowsExtender
    {
        private readonly int groupNameColumnIndex;

        public GroupNameRowsExtender(int groupNameColumnIndex)
        {
            this.groupNameColumnIndex = groupNameColumnIndex;
        }

        public IEnumerable<IList<string>> SplitRowOnManyRows(IList<string> columns)
        {
            string groupValue = columns[this.groupNameColumnIndex];
            var allUniqueGroups = groupValue.SplitByNewLine()
                .SelectMany(gr => SplitCommaSeparatedGroupsRange(gr))
                .DefaultIfEmpty(string.Empty);
            return allUniqueGroups.Select(groupName =>
            {
                var rowCopy = columns.ToList();
                rowCopy[this.groupNameColumnIndex] = groupName;
                return rowCopy;
            });
        }

        private IEnumerable<string> SplitCommaSeparatedGroupsRange(string commaSeparatedGroups)
        {
            // Regex for formats
            // ПЗПІ-18-1
            // ПЗПІ-18-1,3,4
            // ПЗПІ-18-(4,5,6)
            var groupRegex = new Regex(@"^(.+-)\(?(.+)\)?$");
            var groupMatch = groupRegex.Match(commaSeparatedGroups);
            if (!groupMatch.Success)
            {
                yield return commaSeparatedGroups;
            }
            string groupPreffix = groupMatch.Groups[1].Value;
            var groupsNumbers = groupMatch.Groups[2].Value.Split(',');
            foreach (var groupNumber in groupsNumbers.Where(gr => !string.IsNullOrWhiteSpace(gr)))
            {
                yield return groupPreffix + groupNumber;
            }
        }
    }
}
