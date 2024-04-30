// AutoVehicleRenamer.cs
// https://github.com/qstar-inc/cities2-AutoVehicleRenamer
// StarQ 2024

using Colossal;
using Colossal.IO.AssetDatabase;
using Game.Modding;
using Game.Settings;
using Game.UI.Widgets;
using System.Collections.Generic;

namespace AutoVehicleRenamer
{
    [FileLocation($"ModsSettings\\" + nameof(AutoVehicleRenamer))]
    [SettingsUIGroupOrder(generalOptions, actions)]
    [SettingsUIShowGroupName(generalOptions, actions)]
    public class AutoVehicleRenamerSetting : ModSetting
    {
        public const string sectionMain = "Main";

        public const string generalOptions = "General";
        public const string actions = "Actions";

        public AutoVehicleRenamerSetting(IMod mod) : base(mod)
        {
            SetDefaults();
        }

        [SettingsUISection(sectionMain, generalOptions)]
        public bool enableDefault { get; set; }

        [SettingsUITextInput]
        [SettingsUISection(sectionMain, generalOptions)]
        public string separator { get; set; } = string.Empty;

        [SettingsUISection(sectionMain, generalOptions)]
        public textFormatEnum textFormat { get; set; } = textFormatEnum.Value1;

        public DropdownItem<int>[] GetIntDropdownItems()
        {
            var items = new List<DropdownItem<int>>();

            for (var i = 0; i < 3; i += 1)
            {
                items.Add(new DropdownItem<int>()
                {
                    value = i,
                    displayName = i.ToString(),
                });
            }

            return items.ToArray();
        }

        public enum textFormatEnum
        {
            Value1,
            Value2,
        }

        [SettingsUIButton]
        [SettingsUIConfirmation]
        [SettingsUISection(sectionMain, actions)]
        public bool restoreDefaults
        {
            set
            {
                SetDefaults();
            }
        }

        [SettingsUISection(sectionMain, actions)]
        public bool enableVerbose { get; set; }

        [SettingsUIButton]
        [SettingsUISection(sectionMain, actions)]
        public bool openLogFile
        {
            set { Mod.OpenLogFile(); }
        }



        [SettingsUIHidden]
        public bool DummySetting { get; set; }


        public override void SetDefaults()
        {
            enableDefault = false;
            separator = "•";
            textFormat = textFormatEnum.Value1;
            enableVerbose = false;
            DummySetting = true;
            //throw new System.NotImplementedException();
        }
    }

    public class LocaleEN : IDictionarySource
    {
        private readonly AutoVehicleRenamerSetting m_Setting;
        public LocaleEN(AutoVehicleRenamerSetting setting)
        {
            m_Setting = setting;
        }
        public IEnumerable<KeyValuePair<string, string>> ReadEntries(IList<IDictionaryEntryError> errors, Dictionary<string, int> indexCounts)
        {
            return new Dictionary<string, string>
            {
                { m_Setting.GetSettingsLocaleID(), "Auto Vehicle Renamer" },
                { m_Setting.GetOptionTabLocaleID(AutoVehicleRenamerSetting.sectionMain), "Main" },
                { m_Setting.GetOptionGroupLocaleID(AutoVehicleRenamerSetting.generalOptions), "General" },
                { m_Setting.GetOptionGroupLocaleID(AutoVehicleRenamerSetting.actions), "Actions" },

                { m_Setting.GetOptionLabelLocaleID(nameof(AutoVehicleRenamerSetting.enableDefault)), "Rename vehicles from buildings with default names?" },
                { m_Setting.GetOptionDescLocaleID(nameof(AutoVehicleRenamerSetting.enableDefault)), $"By default, buildings with default names such as \"Bus Depot\" or \"Rail Yard\" won't be renamed. Enable this to allow renames of vehicles from buildings with default names." },

                { m_Setting.GetOptionLabelLocaleID(nameof(AutoVehicleRenamerSetting.separator)), "Choose a separator" },
                { m_Setting.GetOptionDescLocaleID(nameof(AutoVehicleRenamerSetting.separator)), $"Type what you want as the character(s) you want as the separator among the vehicle name and origin name. Using emoji or other unsupported characters might crash the game. To be safe, save first, then rename a vehicle manually, let the simulation run for a few seconds, save the game again on a separate file, exit the game, start the game, load the save, unpause the simulation, if this doesn't crash the game, you're good to go." },

                { m_Setting.GetOptionLabelLocaleID(nameof(AutoVehicleRenamerSetting.textFormat)), "Choose a text format" },
                { m_Setting.GetOptionDescLocaleID(nameof(AutoVehicleRenamerSetting.textFormat)), $"Select one of the text formatting to use on the renaming script." },
                { m_Setting.GetEnumValueLocaleID(AutoVehicleRenamerSetting.textFormatEnum.Value1), "{vehicle} {separator} {building}" },
                { m_Setting.GetEnumValueLocaleID(AutoVehicleRenamerSetting.textFormatEnum.Value2), "{building} {separator} {vehicle}" },

                { m_Setting.GetOptionLabelLocaleID(nameof(AutoVehicleRenamerSetting.restoreDefaults)), "Restore Defaults" },
                { m_Setting.GetOptionDescLocaleID(nameof(AutoVehicleRenamerSetting.restoreDefaults)), $"Disables renaming vehicles from buildings with default names, sets the separator to \"•\", and sets the format to \"{{vehicle}} {{separator}} {{building}}\"" },
                { m_Setting.GetOptionWarningLocaleID(nameof(AutoVehicleRenamerSetting.restoreDefaults)), "Confirm restore defaults settings\nfor 'Auto Vehicle Renamer'?" },

                { m_Setting.GetOptionLabelLocaleID(nameof(AutoVehicleRenamerSetting.enableVerbose)), "Enable Verbose Logging" },
                { m_Setting.GetOptionDescLocaleID(nameof(AutoVehicleRenamerSetting.enableVerbose)), $"Only enable for debugging. Not recommended for gameplay." },

                { m_Setting.GetOptionLabelLocaleID(nameof(AutoVehicleRenamerSetting.openLogFile)), "Open Log File" },
                { m_Setting.GetOptionDescLocaleID(nameof(AutoVehicleRenamerSetting.openLogFile)), $"Open the log file on your device's default text/log viewer. Not recommended while cloud streaming." },

            };
        }

        public void Unload()
        {

        }
    }
}
