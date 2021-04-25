using NureSEConsultations.Bot.Model;

namespace NureSEConsultations.Bot.Parser
{
    public interface IParserResolver
    {
        TableParser<Consultation> GetTableParserByType(string parserType);
    }
}