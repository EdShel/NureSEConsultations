using System.Text.RegularExpressions;
using Telegram.Bot.Types.Enums;

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

        public const string VOICE_SEARCH = nameof(MessageType.Voice);

        public const string SEARCH_RESULT = "search";

        public const string SEARCH_PAGES = "searchPages";

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

        public static string ForPages(string type, int pagesCount)
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

        public static string ForSearchResult(string searchQuery, int page)
        {
            return $"{SEARCH_RESULT} {searchQuery}/{page}";
        }

        public static void ParseForSearchResult(string route, out string searchQuery, out int pageIndex)
        {
            var regex = new Regex($@"^{SEARCH_RESULT} (.+)/(\d+)$");
            var match = regex.Match(route);

            searchQuery = match.Groups[1].Value;
            pageIndex = int.Parse(match.Groups[2].Value);
        }

        public static string ForSearchPages(string searchQuery, int pagesCount)
        {
            return $"{SEARCH_PAGES} {searchQuery}/{pagesCount}";
        }

        public static void ParseForSearchPages(string route, out string searchQuery, out int pagesCount)
        {
            var regex = new Regex($@"^{SEARCH_PAGES} (.+)/(\d+)$");
            var match = regex.Match(route);

            searchQuery = match.Groups[1].Value;
            pagesCount = int.Parse(match.Groups[2].Value);
        }
    }
}
