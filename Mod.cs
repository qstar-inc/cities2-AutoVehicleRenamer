using System.Collections.Generic;
using System.Reflection;
using AutoVehicleRenamer.Systems;
using Colossal.IO.AssetDatabase;
using Colossal.Logging;
using Game;
using Game.Modding;
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
            LogHelper.Init(Id, log);
            LocaleHelper.Init(Id, Name, GetReplacements, AddLocale);

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
            LocaleHelper.Dispose();
            m_Setting?.UnregisterInOptionsUI();
            m_Setting = null;
        }

        public static void AddLocale()
        {
            string Building = LocaleHelper.Translate($"{Id}.Building");
            string Separator = LocaleHelper.Translate($"{Id}.Separator");
            string Vehicle = LocaleHelper.Translate($"{Id}.Vehicle");
            LocaleHelper.AddLocalization(
                $"Options.{Id}.{Id}.Mod.TEXTFORMATENUM[Building_Separator_Vehicle]",
                $"{{{Building}}} {{{Separator}}} {{{Vehicle}}}"
            );
            LocaleHelper.AddLocalization(
                $"Options.{Id}.{Id}.Mod.TEXTFORMATENUM[Vehicle_Separator_Building]",
                $"{{{Vehicle}}} {{{Separator}}} {{{Building}}}"
            );
            LocaleHelper.AddLocalization(
                $"Options.{Id}.{Id}.Mod.TEXTFORMATENUM[Building_Separator_Vehicle]",
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
                        $"Options.{Id}.{Id}.Mod.TEXTFORMATENUM[Vehicle_Separator_Building]"
                    )
                },
            };
        }
    }
}
