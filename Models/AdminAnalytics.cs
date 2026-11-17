namespace Dcd.Web.Models;

public sealed record AdminDatePicker(
    string Id, string Caption, DateOnly Earliest, DateOnly Latest, DateOnly Start, DateOnly End);

public sealed record AdminAnalyticsDay(DateOnly Date, int Signups, int Total, int Inactive);

public static class AdminAnalytics
{
    public static DateOnly FirstDay { get; } = new(2026, 5, 1);
    public static DateOnly LastDay { get; } = new(2026, 11, 17);
    public static IReadOnlyList<AdminAnalyticsDay> Days { get; } = CreateDays();

    private static List<AdminAnalyticsDay> CreateDays()
    {
        int[] inactiveAtMonthEnd = [2, 4, 8, 10, 17, 23, AdminPreview.InactiveAccounts];
        int[] novemberSignups = [1, 0, 3, 1, 2, 0, 2, 2, 0, 1, 1, 2, 0, 3, 1, 2, 3];
        var days = new List<AdminAnalyticsDay>();
        var total = 0;
        var inactive = 0;

        for (var monthIndex = 0; monthIndex < AdminPreview.Signups.Count; monthIndex++)
        {
            var month = FirstDay.AddMonths(monthIndex);
            var dayCount = Math.Min(DateTime.DaysInMonth(month.Year, month.Month), LastDay.DayNumber - month.DayNumber + 1);
            var newUsers = month.Month == LastDay.Month ? novemberSignups : new int[dayCount];
            if (month.Month != LastDay.Month)
            {
                for (var signup = 0; signup < AdminPreview.Signups[monthIndex].Count; signup++)
                    newUsers[(monthIndex * 3 + signup * 7 + signup / 4) % dayCount]++;
            }

            for (var day = 0; day < dayCount; day++)
            {
                total += newUsers[day];
                var dayInactive = inactive + (inactiveAtMonthEnd[monthIndex] - inactive) * (day + 1) / dayCount;
                days.Add(new(month.AddDays(day), newUsers[day], total, Math.Min(total, dayInactive)));
            }

            inactive = inactiveAtMonthEnd[monthIndex];
        }

        return days;
    }
}
