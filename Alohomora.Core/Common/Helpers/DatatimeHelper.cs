namespace Alohomora.Core.Common.Helpers;

internal static class DataTimeHelper
{
    internal static PersianDateTime ToPersianDateTime(this DateTime dt) => new(dt);
}
