using System.Collections.Generic;

namespace NureSEConsultations.Bot.Parser
{
    public interface IRowsExtender
    {
        IEnumerable<IList<string>> SplitRowOnManyRows(IList<string> initialRow);
    }
}
