using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace VRCX_API.Helpers
{
    public class IgnoreEmptyStringNullableEnumConverter : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            if (!typeToConvert.IsValueType ||
                !typeToConvert.IsGenericType ||
                typeToConvert.GetGenericTypeDefinition() != typeof(Nullable<>))
            {
                return false;
            }

            Type underlyingType = typeToConvert.GetGenericArguments()[0];
            return underlyingType.IsEnum; // Replace with whatever check is appropriate
        }

        public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            Debug.Assert(CanConvert(typeToConvert));
            Type underlyingType = typeToConvert.GetGenericArguments()[0];
            Type converterType = typeof(IgnoreEmptyStringNullableEnumConverter<>).MakeGenericType(underlyingType);
            JsonConverter underlyingConverter = options.GetConverter(underlyingType);
            return (JsonConverter)Activator.CreateInstance(converterType, underlyingConverter)!;
        }
    }

    public class IgnoreEmptyStringNullableEnumConverter<T>(JsonConverter<T> underlyingConverter) : JsonConverter<T?>
        where T : struct
    {
        public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader is { TokenType: JsonTokenType.Null }
                       or { TokenType: JsonTokenType.String, ValueSpan.Length: 0 })
            {
                return null;
            }

            return underlyingConverter.Read(ref reader, typeof(T), options);
        }

        public override void Write(Utf8JsonWriter writer, T? value, JsonSerializerOptions options)
        {
            if (value is null)
            {
                writer.WriteNullValue();
                return;
            }

            underlyingConverter.Write(writer, value.Value, options);
        }
    }
}
