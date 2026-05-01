using System.Collections.Generic;
using System.Reflection;
using AutoVehicleRenamer.Systems;
using Colossal.IO.AssetDatabase;
using Colossal.Localization;
using Colossal.Logging;
using Game;
using Game.Modding;
using Game.SceneFlow;
using StarQ.Shared.Extensions;

namespace AutoVehicleRenamer
{
    public class Mod : IMod
    {
        public static string Id = nameof(AutoVehicleRenamer);
        public static string Name = Assembly
            .GetExecutingAssembly()
            .GetCustomAttribute<AssemblyTitleAttribute>()
            .Title;
        public static string Version = Assembly
            .GetExecutingAssembly()
            .GetName()
            .Version.ToString(3);

        public static ILog log = LogManager
            .GetLogger(nameof(AutoVehicleRenamer))
            .SetShowsErrorsInUI(false);
        public static Setting m_Setting;

        public void OnLoad(UpdateSystem updateSystem)
        {
            LocalizationManager locMan = GameManager.instance.localizationManager;
            LogHelper.Init(Id, log);
            LocaleHelper.Init(Id, Name, GetReplacements, AddLocale);

            foreach (var item in new LocaleHelper($"{Id}.Locale.json").GetAvailableLanguages())
                locMan.AddSource(item.LocaleId, item);

            locMan.onActiveDictionaryChanged += LocaleHelper.OnActiveDictionaryChanged;

            m_Setting = new Setting(this);
            m_Setting.RegisterInOptionsUI();

            AssetDatabase.global.LoadSettings(
                nameof(AutoVehicleRenamer),
                m_Setting,
                new Setting(this)
            );

            m_Setting.IsDetailedDescriptionsRunning = ModHelper.IsModActive("DetailedDescriptions");

            updateSystem.UpdateAt<AutoVehicleRenamerSystem>(SystemUpdatePhase.UIUpdate);
        }

        public void OnDispose()
        {
            if (m_Setting != null)
            {
                m_Setting.UnregisterInOptionsUI();
                m_Setting = null;
            }
        }

        public static void AddLocale()
        {
            var Building = LocaleHelper.Translate($"{Id}.Mod.Building");
            var Separator = LocaleHelper.Translate($"{Id}.Mod.Separator");
            var Vehicle = LocaleHelper.Translate($"{Id}.Mod.Vehicle");
            LocaleHelper.AddLocalization(
                "Options.AutoVehicleRenamer.AutoVehicleRenamer.Mod.TEXTFORMATENUM[Building_Separator_Vehicle]",
                $"{{{Building}}} {{{Separator}}} {{{Vehicle}}}"
            );
            LocaleHelper.AddLocalization(
                "Options.AutoVehicleRenamer.AutoVehicleRenamer.Mod.TEXTFORMATENUM[Vehicle_Separator_Building]",
                $"{{{Vehicle}}} {{{Separator}}} {{{Building}}}"
            );
            LocaleHelper.AddLocalization(
                "Options.AutoVehicleRenamer.AutoVehicleRenamer.Mod.TEXTFORMATENUM[Building_Separator_Vehicle]",
                $"{{{Building}}} {{{Separator}}} {{{Vehicle}}}"
            );
        }

        public static Dictionary<string, string> GetReplacements()
        {
            return new()
            {
                {
                    "Vehicle_Separator_Building",
                    LocaleHelper.Translate(
                        "Options.AutoVehicleRenamer.AutoVehicleRenamer.Mod.TEXTFORMATENUM[Vehicle_Separator_Building]"
                    )
                },
            };
        }
    }
}
