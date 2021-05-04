using NureSEConsultations.Bot.Model;
using System.Collections.Generic;

namespace NureSEConsultations.Bot.Services
{
    public interface IConsultationSearcher
    {
        IEnumerable<Consultation> Search(string text);
    }
}
