using System.Text.Encodings.Web;
using System.Text.Json.Serialization;

namespace Alohomora.Core.Common.Helpers;

public static class JsonHelper
{
    private static JsonSerializerOptions? _options;

    public static JsonSerializerOptions GetJsonSerializerOptions()
    {
        _options ??= new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            // Performance & Memory
            DefaultBufferSize = 1024 * 4, // 4 KB buffer pool tuning

            // Payload Optimization
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false, // Keep compact in production

            // Flexibility & Compatibility
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            NumberHandling = JsonNumberHandling.AllowReadingFromString,

            // Character Handling (Prevents unnecessary Unicode escaping for non-Latin characters)
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,

            // Reference Handling (Prevent cyclic dependency exceptions in complex graphs)
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        };

        // Enums as strings globally
        _options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));

        // Make immutable for thread safety and internal caching optimization
        _options.MakeReadOnly();

        return _options;
    }

    public static T? Deserialize<T>(string json)
    {
        return JsonSerializer.Deserialize<T>(json, GetJsonSerializerOptions());
    }

    public static string Serialize<T>(T obj)
    {
        return JsonSerializer.Serialize(obj, GetJsonSerializerOptions());
    }
}
