namespace Alohomora.Common.Helpers;

internal static class DatatimeHelper
{
    internal static PersianDateTime ToPersianDateTime(this DateTime dt) => new(dt);
}
