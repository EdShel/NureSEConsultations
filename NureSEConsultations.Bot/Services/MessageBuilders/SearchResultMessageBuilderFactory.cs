using NureSEConsultations.Bot.Constants;
using NureSEConsultations.Bot.Model;
using System.Collections.Generic;

namespace NureSEConsultations.Bot.Services.MessageBuilders
{
    public class SearchResultMessageBuilderFactory
    {
        public IPaginatedMessageBuilder Create(string searchQuery, IEnumerable<Consultation> consultations, int pageIndex, int pagesCount)
        {
            return new ConsultationPageMessageBuilder(
                consultations: consultations,
                pageIndex: pageIndex,
                pagesCount: pagesCount,
                routeToViewPage: pageNumber => Routes.ForSearchResult(searchQuery, pageNumber),
                routeToPageSelect: pagesCount => Routes.ForSearchPages(searchQuery, pagesCount));
        }
    }
}
