using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace NureSEConsultations.Bot.Parser
{
    public static class StringExtensions
    {
        public static IEnumerable<string> SplitByNewLine(this string value)
        {
            return Regex
                .Split(value, @"((\r)+)?(\n)+((\r)+)?")
                .Select(gr => gr.Trim())
                .Where(gr => !string.IsNullOrEmpty(gr));
        }
    }
}
