using System.Collections.Generic;

namespace NureSEConsultations.Bot.Model
{
    public interface IConsultationRepository
    {
        IEnumerable<Consultation> GetAllByType(string type);

        IEnumerable<string> GetConsultationsNames();
    }
}