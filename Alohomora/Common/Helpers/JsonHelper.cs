namespace Alohomora.Core.Common.Helpers;

internal static class JsonHelper
{
    internal static T? Deserialize<T>(string json)
    {
        return System.Text.Json.JsonSerializer.Deserialize<T>(json);
    }

    internal static string Serialize<T>(T obj)
    {
        return System.Text.Json.JsonSerializer.Serialize(obj);
    }
}
