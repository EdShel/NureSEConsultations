using System.Collections.Generic;

namespace NureSEConsultations.Bot.Parser
{
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
}
