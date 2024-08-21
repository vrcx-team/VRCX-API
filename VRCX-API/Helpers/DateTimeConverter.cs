using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace VRCX_API.Helpers
{
    /// <summary>
    /// Converter to match GitHubs DateTimeOffset formatting
    /// </summary>
    public class DateTimeConverter : JsonConverter<DateTimeOffset?>
    {
        public override DateTimeOffset? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            Debug.Assert(typeToConvert == typeof(DateTimeOffset));
            string? str = reader.GetString();
            return string.IsNullOrWhiteSpace(str) ? null : DateTimeOffset.Parse(str);
        }

        public override void Write(Utf8JsonWriter writer, DateTimeOffset? value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value?.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ssZ"));
        }
    }
}
