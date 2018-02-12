using System;
using System.IO;
using Frodo.Common;
using Newtonsoft.Json;

namespace Frodo.Infrastructure.Configuration
{
    public sealed class SettingsProvider : ISettingsProvider
    {
        public const string DEFAULT_CONFIGURATION_FILE = "~\\settings.conf";

        private Lazy<ISettings> _settings;

        public SettingsProvider()
        {
            _settings = new Lazy<ISettings>(SettingsFactory, true);
        }

        public ISettings Settings
        {
            get
            {
                return _settings.Value;
            }
        }

        private ISettings SettingsFactory()
        {
            return Load();
        }

        private static ISettings Load(string configurationFile = DEFAULT_CONFIGURATION_FILE)
        {
            Settings result = null;
            var settingsFile = PathUtils.ToAbsolutePath(configurationFile);

            if (File.Exists(settingsFile))
            {
                result = TryDeserialize(File.ReadAllText(settingsFile));
            }

            if (result == null)
            {
                result = new Settings();
            }

            return result;
        }

        public void Save(string configurationFile = DEFAULT_CONFIGURATION_FILE)
        {
            File.WriteAllText(PathUtils.ToAbsolutePath(configurationFile), TrySerialize(Settings));
            _settings = new Lazy<ISettings>(SettingsFactory);
        }

        private static Settings TryDeserialize(string json)
        {
            try
            {
                return JsonConvert.DeserializeObject<Settings>(json);
            }
            catch
            {
                return new Settings();
            }
        }

        private static string TrySerialize(ISettings settings)
        {
            try
            {
                return JsonConvert.SerializeObject(settings, Formatting.Indented);
            }
            catch
            {
                return "{}";
            }
        }
    }
}
