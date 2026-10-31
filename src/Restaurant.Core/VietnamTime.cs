namespace Restaurant.Core;

public readonly record struct UtcRange(DateTime Start, DateTime EndExclusive);

public static class VietnamTime
{
    private static readonly TimeZoneInfo Zone = TimeZoneInfo.FindSystemTimeZoneById(
        OperatingSystem.IsWindows() ? "SE Asia Standard Time" : "Asia/Ho_Chi_Minh");

    public static DateTime ToLocal(DateTime utc)
    {
        ArgumentOutOfRangeException.ThrowIfNotEqual(utc.Kind, DateTimeKind.Utc);
        return TimeZoneInfo.ConvertTimeFromUtc(utc, Zone);
    }

    public static DateTime ToUtc(DateTime local)
    {
        ArgumentOutOfRangeException.ThrowIfNotEqual(local.Kind, DateTimeKind.Unspecified);
        return TimeZoneInfo.ConvertTimeToUtc(local, Zone);
    }

    public static UtcRange GetUtcRange(DateOnly localDate)
    {
        var start = localDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified);
        var end = localDate.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified);
        return new UtcRange(ToUtc(start), ToUtc(end));
    }
}
