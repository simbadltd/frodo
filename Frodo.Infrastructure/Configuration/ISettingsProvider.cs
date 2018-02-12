namespace Frodo.Infrastructure.Configuration
{
    public interface ISettingsProvider
    {
        ISettings Settings { get; }

        void Save(string configurationFile = SettingsProvider.DEFAULT_CONFIGURATION_FILE);
    }
}