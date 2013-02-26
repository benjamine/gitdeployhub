using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GitDeployHub.Web.Engine
{
    public static class DateTimeFormatter
    {
        // partialy taken from: http://www.robfe.com/2009/09/timeago-for-csharp/

        public static string TimeAgo(DateTime utc)
        {
            return Describe(DateTime.UtcNow.Subtract(utc));
        }

        static readonly string[] NAMES = {
                                         "day",
                                         "hour",
                                         "minute",
                                         "second"
                                     };

        public static string Describe(TimeSpan t)
        {
            int[] ints = {
                         t.Days,
                         t.Hours,
                         t.Minutes,
                         t.Seconds
                     };

            double[] doubles = {
                               t.TotalDays,
                               t.TotalHours,
                               t.TotalMinutes,
                               t.TotalSeconds
                           };

            var firstNonZero = ints
                .Select((value, index) => new { value, index })
                .FirstOrDefault(x => x.value != 0);
            if (firstNonZero == null)
            {
                return "now";
            }
            int i = firstNonZero.index;
            string prefix = (i >= 3) ? "" : "about ";
            int quantity = (int)Math.Round(doubles[i]);
            return prefix + Tense(quantity, NAMES[i]) + " ago";
        }

        public static string Tense(int quantity, string noun)
        {
            return quantity == 1
                ? "1 " + noun
                : string.Format("{0} {1}s", quantity, noun);
        }
    }
}