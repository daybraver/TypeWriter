using System.IO;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using XamlPearls.XamlFont;

namespace TypeWriter.Services
{
    internal class AppConfigSource
    {
        private readonly JsonSerializerOptions _options;
        private readonly string _path;
        private readonly object _syncObj;
        private AppConfig _appConfig = null!;

        public AppConfigSource()
        {
            _path = Path.Combine(Path.GetDirectoryName(Environment.ProcessPath)!, "AppConfig.json");
            _syncObj = new object();
            _options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Converters =
                {
                    new ColorJsonConverter(),
                    new FontInfoJsonConverter()
                }
            };
        }

        public AppConfig GetConfig()
        {
            if (_appConfig == null)
            {
                lock (_syncObj)
                {
                    if (_appConfig == null)
                    {
                        try
                        {
                            if (File.Exists(_path))
                            {
                                var json = File.ReadAllText(_path, Encoding.UTF8);
                                _appConfig = JsonSerializer.Deserialize<AppConfig>(json, _options)!;
                            }
                            else
                            {
                                _appConfig = new AppConfig();
                            }
                        }
                        catch
                        {
                            _appConfig = new AppConfig();
                        }
                    }
                }
            }

            return _appConfig!;

            //void GiveDefaultValue()
            //{
            //    _appConfig = new AppConfig();
            //    _appConfig.BackColor = Colors.White;
            //    _appConfig.ToTypeFont = new FontInfo();
            //    _appConfig.ToTypeFont.BrushColor = new SolidColorBrush(Colors.Black);
            //    _appConfig.ToTypeFont.Stretch = FontStretches.Normal;
            //    _appConfig.ToTypeFont.Weight = FontWeights.Regular;
            //    _appConfig.ToTypeFont.Style = FontStyles.Normal;
            //    _appConfig.ToTypeFont.Family = new FontFamily("Consolas");
            //    _appConfig.ToTypeFont.Size = 20;
            //    _appConfig.TypedFont = new FontInfo();
            //    _appConfig.TypedFont.BrushColor = new SolidColorBrush(Colors.Black);
            //    _appConfig.TypedFont.Stretch = FontStretches.Normal;
            //    _appConfig.TypedFont.Weight = FontWeights.Regular;
            //    _appConfig.TypedFont.Style = FontStyles.Normal;
            //    _appConfig.TypedFont.Family = new FontFamily("Consolas");
            //    _appConfig.TypedFont.Size = 20;
            //    _appConfig.TypeBoxWidth = 1000;
            //    _appConfig.TypeBoxHeight = 40;
            //    _appConfig.LearnWordOption = new LearnWordOption()
            //    {
            //        BackColor = Colors.Black,
            //        BoxHeight = 40,
            //        BoxWidth = 1000,
            //        Accent = Accent.UK,
            //        FontInfo = new FontInfo(),
            //    };
            //    _appConfig.LearnWordOption.FontInfo.BrushColor = new SolidColorBrush(Colors.White);
            //    _appConfig.LearnWordOption.FontInfo.Stretch = FontStretches.Normal;
            //    _appConfig.LearnWordOption.FontInfo.Weight = FontWeights.Regular;
            //    _appConfig.LearnWordOption.FontInfo.Family = new FontFamily("Consolas");
            //    _appConfig.LearnWordOption.FontInfo.Style = FontStyles.Normal;
            //    _appConfig.LearnWordOption.FontInfo.Size = 20;
            //}
        }

        public void SaveConfig(AppConfig config)
        {
            var configJson = JsonSerializer.Serialize(config, _options);
            _appConfig = JsonSerializer.Deserialize<AppConfig>(configJson, _options)!;
            File.WriteAllText(_path, configJson, Encoding.UTF8);
        }
    }
}
