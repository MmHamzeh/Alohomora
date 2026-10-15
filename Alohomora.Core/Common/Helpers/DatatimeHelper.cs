namespace Alohomora.Core.Common.Helpers;

internal static class DataTimeHelper
{
    internal static PersianDateTime ToPersianDateTime(this DateTime dt) => new(dt);

    public static string ToPersianRemainingTime(this TimeSpan remaining)
    {
        if (remaining <= TimeSpan.Zero)
            return string.Empty;

        var totalDays = (int)Math.Floor(remaining.TotalDays);
        var hours = remaining.Hours;
        var minutes = remaining.Minutes;

        // If under 1 hour, round up remaining fractional minute
        if (totalDays == 0 && hours == 0)
        {
            var roundedMinutes = (int)Math.Ceiling(remaining.TotalMinutes);
            return $"{roundedMinutes} دقیقه";
        }

        if (totalDays > 0)
        {
            return hours > 0
                ? $"{totalDays} روز و {hours} ساعت"
                : $"{totalDays} روز";
        }

        return minutes > 0
            ? $"{hours} ساعت و {minutes} دقیقه"
            : $"{hours} ساعت";
    }
}
