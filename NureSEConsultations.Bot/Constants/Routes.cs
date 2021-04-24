using System.Text.RegularExpressions;

namespace NureSEConsultations.Bot.Constants
{
    public static class Routes
    {
        public const string START = "/start";

        public const string DEFAULT = "/what";

        public const string CONSULTATIONS_LIST = "Список консультацій";

        public const string STATISTICS = "Статистика";

        public const string CONCRETE_CONSULTATION = "list";

        public const string PAGES = "pages";

        public static string ForConcreteConsultation(string type, int page)
        {
            return $"{CONCRETE_CONSULTATION} {type}/{page}";
        }

        public static void ParseForConcreteConsultation(string route, out string consultationType, out int pageIndex)
        {
            var regex = new Regex($@"^{CONCRETE_CONSULTATION} (.+)/(\d+)$");
            var match = regex.Match(route);

            consultationType = match.Groups[1].Value;
            pageIndex = int.Parse(match.Groups[2].Value);
        }

        public static string RouteForPages(string type, int pagesCount)
        {
            return $"{PAGES} {type}/{pagesCount}";
        }

        public static void ParseRouteForPages(string route, out string consultationType, out int pagesCount)
        {
            var regex = new Regex($@"^{PAGES} (.+)/(\d+)$");
            var match = regex.Match(route);

            consultationType = match.Groups[1].Value;
            pagesCount = int.Parse(match.Groups[2].Value);
        }
    }
}
