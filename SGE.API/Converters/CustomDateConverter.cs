using System.Text.Json;
using System.Text.Json.Serialization;
using SGE.Core.Exceptions;
using SGE.Core.Helpers;

namespace SGE.API.Converters;

public class CustomDateConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var dateStr = reader.GetString();
        var date = DateHelper.ParseDate(dateStr);

        return date ?? throw new DateTimeFormatException(dateStr);
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString("yyyy-MM-ddTHH:mm:ss"));
    }
}