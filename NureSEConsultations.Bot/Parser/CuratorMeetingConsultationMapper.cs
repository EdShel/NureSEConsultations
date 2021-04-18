using NureSEConsultations.Bot.Model;
using System.Collections.Generic;

namespace NureSEConsultations.Bot.Parser
{
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
}
