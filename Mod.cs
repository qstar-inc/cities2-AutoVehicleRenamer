// AutoVehicleRenamer.cs
// https://github.com/qstar-inc/cities2-AutoVehicleRenamer
// StarQ 2024

using Colossal.IO.AssetDatabase;
using Colossal.Logging;
using Game;
using Game.Modding;
using Game.SceneFlow;
using Colossal.PSI.Environment;

namespace AutoVehicleRenamer
{
    public class Mod : IMod
    {
        public const string ModName = "Auto Vehicle Renamer";
        public static ILog log = LogManager.GetLogger($"{nameof(AutoVehicleRenamer)}.{nameof(Mod)}").SetShowsErrorsInUI(false);
        public static AutoVehicleRenamerSetting m_Setting;

        public void OnLoad(UpdateSystem updateSystem)
        {
            log.Info("Auto Vehicle Renamer loaded successfully");

            if (GameManager.instance.modManager.TryGetExecutableAsset(this, out var asset))
                log.Info($"Current mod asset at {asset.path}");

            m_Setting = new AutoVehicleRenamerSetting(this);
            m_Setting.RegisterInOptionsUI();
            GameManager.instance.localizationManager.AddSource("en-US", new LocaleEN(m_Setting));

            AssetDatabase.global.LoadSettings(nameof(AutoVehicleRenamer), m_Setting, new AutoVehicleRenamerSetting(this));
            m_Setting.DummySetting = false;

            updateSystem.UpdateAt<AutoVehicleRenamer>(SystemUpdatePhase.UIUpdate);
            if (m_Setting.enableVerbose) { log.Info("Verbose logging is enabled. Disable it in Settings if you're not debugging."); }
        }

        public void OnDispose()
        {
            if (m_Setting != null)
            {
                m_Setting.UnregisterInOptionsUI();
                m_Setting = null;
            }
            log.Info("Shutting down Auto Vehicle Renamer");
        }
        public static void OpenLogFile()
        {
            System.Diagnostics.Process.Start($"{EnvPath.kUserDataPath}/Logs/{nameof(AutoVehicleRenamer)}.{nameof(Mod)}.log");
        }
    }
}
