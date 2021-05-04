using NureSEConsultations.Bot.Constants;
using NureSEConsultations.Bot.Model;
using System.Collections.Generic;

namespace NureSEConsultations.Bot.Services.MessageBuilders
{
    public class ConsultationPageMessageBuilderFactory
    {
        public IPaginatedMessageBuilder Create(string consultationType, IEnumerable<Consultation> consultations, int pageIndex, int pagesCount)
        {
            return new ConsultationPageMessageBuilder(
                consultations: consultations,
                pageIndex: pageIndex,
                pagesCount: pagesCount,
                routeToViewPage: pageNumber => Routes.ForConcreteConsultation(consultationType, pageNumber),
                routeToPageSelect: pagesCount => Routes.ForPages(consultationType, pagesCount));
        }
    }
}
