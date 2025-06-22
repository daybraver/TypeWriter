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
        }

        public void SaveConfig(AppConfig config)
        {
            var configJson = JsonSerializer.Serialize(config, _options);
            _appConfig = JsonSerializer.Deserialize<AppConfig>(configJson, _options)!;
            File.WriteAllText(_path, configJson, Encoding.UTF8);
        }
    }
}
