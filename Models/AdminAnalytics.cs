namespace Dcd.Web.Models;

public sealed record DailyAccountChange(DateOnly Date, int Signups, int InactiveDelta);

public sealed record AdminDatePicker(
    string Id, string Caption, DateOnly Earliest, DateOnly Latest, DateOnly Start, DateOnly End);

public sealed record AdminAnalyticsDay(DateOnly Date, int Signups, int Total, int Inactive);
