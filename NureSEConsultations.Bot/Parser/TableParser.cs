using NureSEConsultations.Bot.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace NureSEConsultations.Bot.Parser
{
    public static class StringExtensions
    {
        public static IEnumerable<string> SplitByNewLine(this string value)
        {
            return Regex
                .Split(value, @"((\r)+)?(\n)+((\r)+)?")
                .Select(gr => gr.Trim())
                .Where(gr => !string.IsNullOrEmpty(gr));
        }
    }

    public interface IRowsExtender
    {
        IEnumerable<IList<string>> SplitRowOnManyRows(IList<string> initialRow);
    }

    public interface IRowMapper<T>
    {
        T Map(IList<string> row);
    }

    public interface IRowsUnmerger
    {
        IList<IList<string>> FillSkippedByMergeValues(IList<IList<string>> table);
    }

    public class KeyBoundUnmerger : IRowsUnmerger
    {
        private readonly int keyColumnIndex;

        public KeyBoundUnmerger(int keyColumnIndex)
        {
            this.keyColumnIndex = keyColumnIndex;
        }

        public IList<IList<string>> FillSkippedByMergeValues(IList<IList<string>> table)
        {
            var rowsList = new List<IList<string>>(table.Count);
            IList<string> previousKeyedRow = null;
            foreach (var row in table)
            {
                if (!string.IsNullOrEmpty(row[this.keyColumnIndex]))
                {
                    previousKeyedRow = row;
                    rowsList.Add(row);
                }
                else if (previousKeyedRow != null)
                {
                    var rowToAdd = new string[row.Count];
                    for (int colIndex = 0; colIndex < row.Count; colIndex++)
                    {
                        string columnValue = row[colIndex];
                        string valueToApply;
                        if (!string.IsNullOrEmpty(columnValue))
                        {
                            valueToApply = columnValue;
                        }
                        else
                        {
                            valueToApply = previousKeyedRow[colIndex];
                        }
                        rowToAdd[colIndex] = valueToApply;
                    }
                    rowsList.Add(rowToAdd);
                }
            }
            return rowsList;
        }
    }

    public class CuratorMeetingConsultationMapper : IRowMapper<Consultation>
    {
        private readonly string subjectName;

        public CuratorMeetingConsultationMapper(string subjectName)
        {
            this.subjectName = subjectName;
        }

        public Consultation Map(IList<string> row)
        {
            return new Consultation
            {
                Subject = subjectName,
                Teacher = row[0],
                Group = row[2],
                Time = row[3],
                Link = row[4]
            };
        }
    }
    public class SubjectConsultationMapper : IRowMapper<Consultation>
    {
        public Consultation Map(IList<string> row)
        {
            return new Consultation
            {
                Subject = row[0],
                Teacher = row[1],
                Group = row[2],
                Time = row[3],
                Link = row[4]
            };
        }
    }

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

    public class TableParser<T>
    {
        private IRowMapper<T> rowMapper;

        public TableParser(IRowMapper<T> rowMapper)
        {
            this.rowMapper = rowMapper;
        }

        private IEnumerable<IRowsExtender> rowExtenders = Enumerable.Empty<IRowsExtender>();

        private IRowsUnmerger unmerger = new KeyBoundUnmerger(0);

        public IRowMapper<T> RowMapper
        {
            get => this.rowMapper;
            set => this.rowMapper = value ?? throw new ArgumentNullException(nameof(value));
        }

        public IEnumerable<IRowsExtender> RowExtenders
        {
            get => this.rowExtenders;
            set => this.rowExtenders = value ?? throw new ArgumentNullException(nameof(value));
        }
        public IRowsUnmerger Unmerger
        {
            get => this.unmerger;
            set => this.unmerger = value ?? throw new ArgumentNullException(nameof(value));
        }

        public IEnumerable<T> ParseTable(IList<IList<object>> tableData)
        {
            var stringified = StringifyData(tableData);
            var normalized = NormalizeCountOfColumns(stringified, string.Empty);
            var putSkipped = this.unmerger.FillSkippedByMergeValues(normalized);
            var extended = putSkipped.SelectMany(r => GetMaximumExtended(r));
            var mapped = extended.Select(r => this.rowMapper.Map(r));
            return mapped.ToList();
        }

        private IList<IList<T>> NormalizeCountOfColumns<T>(IList<IList<T>> table, T defaultValue)
        {
            int maxColumnsCount = table.Max(row => row.Count);
            return table.Select(row =>
                (IList<T>)row.Concat(Enumerable.Repeat(defaultValue, maxColumnsCount - row.Count))
                    .ToList()
            ).ToList();
        }

        protected virtual IList<IList<string>> StringifyData(IList<IList<object>> tableData)
        {
            var cleanupRegex = new Regex(@"\s{2,}");
            return tableData.Select(row =>
                (IList<string>)row.Select(col =>
                {
                    return cleanupRegex.Replace(col.ToString(), " ").Trim();
                }).ToList()
            ).ToList();
        }

        private IList<IList<string>> GetMaximumExtended(IList<string> row)
        {
            IEnumerable<IList<string>> rows = new[] { row };
            foreach (var extender in this.rowExtenders)
            {
                rows = rows.SelectMany(r => extender.SplitRowOnManyRows(r));
            }
            return rows.ToList();
        }
    }
}
