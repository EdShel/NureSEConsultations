using System.Collections.Generic;

namespace NureSEConsultations.Bot.Parser
{
    public interface IRowsUnmerger
    {
        IList<IList<string>> FillSkippedByMergeValues(IList<IList<string>> table);
    }
}
