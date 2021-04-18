using System.Collections.Generic;

namespace NureSEConsultations.Bot.Parser
{
    public interface IRowMapper<T>
    {
        T Map(IList<string> row);
    }
}
