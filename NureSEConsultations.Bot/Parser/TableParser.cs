using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace NureSEConsultations.Bot.Parser
{
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

        private IList<IList<TCell>> NormalizeCountOfColumns<TCell>(IList<IList<TCell>> table, TCell defaultValue)
        {
            int maxColumnsCount = table.Max(row => row.Count);
            return table.Select(row =>
                (IList<TCell>)row.Concat(Enumerable.Repeat(defaultValue, maxColumnsCount - row.Count))
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
