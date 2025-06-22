using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using XamlPearls.XamlFont;

namespace TypeWriter
{
    public class FontInfoJsonConverter : System.Text.Json.Serialization.JsonConverter<FontInfo>
    {
        private Dictionary<string, System.Windows.FontStyle> _styles = new Dictionary<string, System.Windows.FontStyle>();

        public FontInfoJsonConverter()
        {
            _styles.Add(nameof(FontStyles.Normal), FontStyles.Normal);
            _styles.Add(nameof(FontStyles.Italic), FontStyles.Italic);
            _styles.Add(nameof(FontStyles.Oblique), FontStyles.Oblique);
        }

        public override FontInfo? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            FontInfo? fontInfo = new FontInfo();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    if (reader.GetString() == nameof(fontInfo.Size))
                    {
                        reader.Read();
                        fontInfo.Size = reader.GetInt32();
                    }
                    else if (reader.GetString() == nameof(fontInfo.Weight))
                    {
                        reader.Read();
                        fontInfo.Weight = FontWeight.FromOpenTypeWeight(reader.GetInt32());
                    }
                    else if (reader.GetString() == nameof(fontInfo.Stretch))
                    {
                        reader.Read();
                        fontInfo.Stretch = FontStretch.FromOpenTypeStretch(reader.GetInt32());
                    }
                    else if (reader.GetString() == nameof(fontInfo.Family))
                    {
                        reader.Read();
                        fontInfo.Family = new System.Windows.Media.FontFamily(reader.GetString());
                    }
                    else if (reader.GetString() == nameof(fontInfo.Style))
                    {
                        reader.Read();
                        fontInfo.Style = _styles[reader.GetString()!];
                    }
                    else if (reader.GetString() == nameof(fontInfo.BrushColor))
                    {
                        reader.Read();
                        fontInfo.BrushColor = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter
                        .ConvertFromString(reader.GetString()));
                    }
                }
                else if (reader.TokenType == JsonTokenType.EndObject)
                {
                    break;
                }
            }
            return fontInfo;
        }

        public override void Write(Utf8JsonWriter writer, FontInfo value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName(nameof(value.Size));
            writer.WriteNumberValue(value.Size);
            writer.WritePropertyName(nameof(value.Weight));
            writer.WriteNumberValue(value.Weight.ToOpenTypeWeight());
            writer.WritePropertyName(nameof(value.Stretch));
            writer.WriteNumberValue(value.Stretch.ToOpenTypeStretch());
            writer.WritePropertyName(nameof(value.Family));
            writer.WriteStringValue(value.Family.Source);
            writer.WritePropertyName(nameof(value.Style));
            writer.WriteStringValue(value.Style.ToString());
            writer.WritePropertyName(nameof(value.BrushColor));
            writer.WriteRawValue(System.Text.Json.JsonSerializer.Serialize(value.BrushColor.Color, options));
            writer.WriteEndObject();
        }
    }
}