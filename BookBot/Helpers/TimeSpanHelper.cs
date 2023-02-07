using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookBot.Helpers
{
    public static class TimeSpanHelper
    {
        public static string ToReadableAgeString(this TimeSpan span)
        {
            return string.Format("{0:0}", span.Days / 365.25);
        }

        public static string ToReadableString(this TimeSpan span)
        {
            string formatted = string.Format("{0}{1}{2}{3}",
                span.Duration().Days > 0 ? string.Format("{0:0} день{1}, ", span.Days, span.Days == 1 ? string.Empty : "") : string.Empty,
                span.Duration().Hours > 0 ? string.Format("{0:0} час.{1}, ", span.Hours, span.Hours == 1 ? string.Empty : "") : string.Empty,
                span.Duration().Minutes > 0 ? string.Format("{0:0} мин.{1}, ", span.Minutes, span.Minutes == 1 ? string.Empty : "") : string.Empty,
                span.Duration().Seconds > 0 ? string.Format("{0:0} сек.{1}", span.Seconds, span.Seconds == 1 ? string.Empty : "") : string.Empty);

            if (formatted.EndsWith(", ")) formatted = formatted.Substring(0, formatted.Length - 2);

            if (string.IsNullOrEmpty(formatted)) formatted = "0 секунд";

            return formatted;
        }
    }
}
