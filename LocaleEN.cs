// AutoVehicleRenamer.cs
// https://github.com/qstar-inc/cities2-AutoVehicleRenamer
// StarQ 2024

using System.Collections.Generic;
using Colossal;

namespace AutoVehicleRenamer
{
    public class LocaleEN : IDictionarySource
    {
        private readonly Setting m_Setting;

        public LocaleEN(Setting setting)
        {
            m_Setting = setting;
        }

        public IEnumerable<KeyValuePair<string, string>> ReadEntries(
            IList<IDictionaryEntryError> errors,
            Dictionary<string, int> indexCounts
        )
        {
            return new Dictionary<string, string>
            {
                { m_Setting.GetSettingsLocaleID(), Mod.Name },
                { m_Setting.GetOptionTabLocaleID(Setting.MainSection), "Main" },
                { m_Setting.GetOptionGroupLocaleID(Setting.GeneralOptions), "General" },
                { m_Setting.GetOptionGroupLocaleID(Setting.Actions), "Actions" },
                {
                    m_Setting.GetOptionLabelLocaleID(nameof(Setting.ApplyToAll)),
                    "Apply to all Vehicles"
                },
                {
                    m_Setting.GetOptionDescLocaleID(nameof(Setting.ApplyToAll)),
                    $"Apply the selected settings to all vehicles on the current save. Helpful after installing the mod for the first time or after renaming an existing depot/origin building."
                },
                {
                    m_Setting.GetOptionLabelLocaleID(nameof(Setting.EnableDefault)),
                    "Rename vehicles from buildings with default names?"
                },
                {
                    m_Setting.GetOptionDescLocaleID(nameof(Setting.EnableDefault)),
                    $"By default, buildings with default names such as \"Bus Depot\" or \"Rail Yard\" won't be renamed. Enable this to allow renames of vehicles from buildings with default names."
                },
                {
                    m_Setting.GetOptionLabelLocaleID(nameof(Setting.Separator)),
                    "Choose a separator"
                },
                {
                    m_Setting.GetOptionDescLocaleID(nameof(Setting.Separator)),
                    $"Type what you want as the character(s) you want as the separator among the vehicle name and origin name. Using emoji or other unsupported characters might crash the game. To be safe, save first, then rename a vehicle manually, let the simulation run for a few seconds, save the game again on a separate file, exit the game, start the game, load the save, unpause the simulation, if this doesn't crash the game, you're good to go."
                },
                {
                    m_Setting.GetOptionLabelLocaleID(nameof(Setting.TextFormat)),
                    "Choose a text format"
                },
                {
                    m_Setting.GetOptionDescLocaleID(nameof(Setting.TextFormat)),
                    $"Select one of the text formatting to use on the renaming script."
                },
                {
                    m_Setting.GetEnumValueLocaleID(Setting.TextFormatEnum.Value1),
                    "{vehicle} {separator} {building}"
                },
                {
                    m_Setting.GetEnumValueLocaleID(Setting.TextFormatEnum.Value2),
                    "{building} {separator} {vehicle}"
                },
                {
                    m_Setting.GetOptionLabelLocaleID(nameof(Setting.RestoreDefaults)),
                    "Restore Defaults"
                },
                {
                    m_Setting.GetOptionDescLocaleID(nameof(Setting.RestoreDefaults)),
                    $"Disables renaming vehicles from buildings with default names, sets the separator to \"•\", and sets the format to \"{{vehicle}} {{separator}} {{building}}\""
                },
                {
                    m_Setting.GetOptionWarningLocaleID(nameof(Setting.RestoreDefaults)),
                    "Confirm restore defaults settings\nfor 'Auto Vehicle Renamer'?"
                },
                {
                    m_Setting.GetOptionLabelLocaleID(nameof(Setting.EnableVerbose)),
                    "Enable Verbose Logging"
                },
                {
                    m_Setting.GetOptionDescLocaleID(nameof(Setting.EnableVerbose)),
                    $"Only enable for debugging. Not recommended for gameplay."
                },
                { m_Setting.GetOptionTabLocaleID(Setting.AboutTab), "About" },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.NameText)), "Mod Name" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.NameText)), "" },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.VersionText)), "Mod Version" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.VersionText)), "" },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.AuthorText)), "Author" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.AuthorText)), "" },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.BMaCLink)), "Buy Me a Coffee" },
                {
                    m_Setting.GetOptionDescLocaleID(nameof(Setting.BMaCLink)),
                    "Support the author."
                },
            };
        }

        public void Unload() { }
    }
}
