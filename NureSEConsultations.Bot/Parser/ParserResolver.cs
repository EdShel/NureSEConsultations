using NureSEConsultations.Bot.Model;
using System;

namespace NureSEConsultations.Bot.Parser
{
    public class ParserResolver : IParserResolver
    {
        public TableParser<Consultation> GetTableParserByType(string parserType)
        {
            return parserType switch
            {
                "curators" => new TableParser<Consultation>(new CuratorMeetingConsultationMapper("Зустріч з куратором"))
                {
                    RowExtenders = new IRowsExtender[] { new GroupNameRowsExtender(2), new ConsultationTimeRowsExtender(3) },
                },
                "subjects" => new TableParser<Consultation>(new SubjectConsultationMapper())
                {
                    RowExtenders = new IRowsExtender[] { new GroupNameRowsExtender(2), new ConsultationTimeRowsExtender(3) },
                },
                _ => throw new InvalidOperationException($"Unknown parser type {parserType}."),
            };
        }
    }
}
