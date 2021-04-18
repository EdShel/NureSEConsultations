using NureSEConsultations.Bot.Model;

namespace NureSEConsultations.Bot.Parser
{
    public interface IParserResolver
    {
        TableParser<Consultation> GetTableParserBySheetName(string name, string parserType);
    }
}