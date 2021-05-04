using System.Text;
using System.Text.RegularExpressions;

namespace NureSEConsultations.Bot.Services
{
    public class SearchQueryNormalizer
    {
        public string NormalizeStrict(string searchQuery)
        {
            var wordRegex = new Regex(@"[0-9A-Za-zА-Яа-яІїіїҐґЪъЁё-]+");
            var sb = new StringBuilder();
            foreach(Match match in wordRegex.Matches(searchQuery))
            {
                sb.Append(match.Value);
                sb.Append(' ');
            }
            sb.Remove(sb.Length - 1, 1);

            return sb.ToString();
        }

        public string NormalizeFuzzy(string searchQuery)
        {
            var wordRegex = new Regex(@"[0-9A-Za-zА-Яа-яІїіїҐґЪъЁё-]+");
            var sb = new StringBuilder();
            foreach (Match match in wordRegex.Matches(searchQuery))
            {
                sb.Append(match.Value);
                sb.Append("~ ");
            }
            sb.Remove(sb.Length - 1, 1);

            return sb.ToString();
        }
    }
}
