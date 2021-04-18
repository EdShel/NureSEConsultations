using NureSEConsultations.Bot.Model;
using System.Collections.Generic;

namespace NureSEConsultations.Bot.Parser
{
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
}
