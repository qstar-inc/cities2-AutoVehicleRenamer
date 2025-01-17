// AutoVehicleRenamer.cs
// https://github.com/qstar-inc/cities2-AutoVehicleRenamer
// StarQ 2024

using Colossal.IO.AssetDatabase;
using Colossal.Logging;
using Game;
using Game.Modding;
using Game.SceneFlow;

namespace AutoVehicleRenamer
{
    public class Mod : IMod
    {
        public const string Name = "Auto Vehicle Renamer";
        public static string Version = "1.0.3";
        public static ILog log = LogManager.GetLogger($"{nameof(AutoVehicleRenamer)}").SetShowsErrorsInUI(false);
        public static Setting m_Setting;

        public void OnLoad(UpdateSystem updateSystem)
        {
            if (m_Setting.EnableVerbose) log.Info("Auto Vehicle Renamer loaded successfully");

            m_Setting = new Setting(this);
            m_Setting.RegisterInOptionsUI();
            GameManager.instance.localizationManager.AddSource("en-US", new LocaleEN(m_Setting));

            AssetDatabase.global.LoadSettings(nameof(AutoVehicleRenamer), m_Setting, new Setting(this));

            updateSystem.UpdateAt<AutoVehicleRenamer>(SystemUpdatePhase.UIUpdate);
            if (m_Setting.EnableVerbose) log.Info("Verbose logging is enabled. Disable it in Settings if you're not debugging.");
        }

        public void OnDispose()
        {
            if (m_Setting != null)
            {
                m_Setting.UnregisterInOptionsUI();
                m_Setting = null;
            }
            if (m_Setting.EnableVerbose) log.Info("Shutting down Auto Vehicle Renamer");
        }
    }
}
