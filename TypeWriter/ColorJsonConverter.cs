using System.Text.Json;
using System.Text.Json.Serialization;

namespace TypeWriter
{
    public class ColorJsonConverter : JsonConverter<System.Windows.Media.Color>
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert == typeof(System.Windows.Media.Color);
        }

        public override System.Windows.Media.Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return (System.Windows.Media.Color)System.Windows.Media.ColorConverter
                .ConvertFromString(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, System.Windows.Media.Color value, JsonSerializerOptions options)
        {
            writer.WriteRawValue($"\"#{value.A:X2}{value.R:X2}{value.G:X2}{value.B:X2}\"");
        }
    }
}