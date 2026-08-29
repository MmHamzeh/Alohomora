namespace Alohomora.Core.Common.Helpers;

internal static class GuidHelper
{
    internal static bool IsNullOrEmpty(this Guid? guid) => guid.GetValueOrDefault() == Guid.Empty;

}
