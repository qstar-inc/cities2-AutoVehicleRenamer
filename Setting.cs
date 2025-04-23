// AutoVehicleRenamer.cs
// https://github.com/qstar-inc/cities2-AutoVehicleRenamer
// StarQ 2024

using System;
using System.Collections.Generic;
using Colossal.IO.AssetDatabase;
using Game.Modding;
using Game.Settings;
using Game.UI.Widgets;
using Unity.Entities;
using UnityEngine.Device;

namespace AutoVehicleRenamer
{
    [FileLocation("ModsSettings\\StarQ\\" + nameof(AutoVehicleRenamer))]
    [SettingsUIGroupOrder(GeneralOptions, Actions)]
    [SettingsUIShowGroupName(GeneralOptions, Actions)]
    public class Setting : ModSetting
    {
        public const string MainSection = "Main";

        public const string GeneralOptions = "General";
        public const string Actions = "Actions";

        public const string AboutTab = "About";
        public const string InfoGroup = "Info";

        public AutoVehicleRenamer avr =
            World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<AutoVehicleRenamer>();

        public Setting(IMod mod)
            : base(mod)
        {
            SetDefaults();
        }

        [SettingsUIHidden]
        public bool IsInGameOrEditor { get; set; } = false;

        [SettingsUIButton]
        [SettingsUISection(MainSection, Actions)]
        [SettingsUIDisableByCondition(typeof(Setting), nameof(IsInGameOrEditor), true)]
        public bool ApplyToAll
        {
            set { avr.UpdateVehicleName(true); }
        }

        [SettingsUISection(MainSection, GeneralOptions)]
        public bool EnableDefault { get; set; }

        [SettingsUITextInput]
        [SettingsUISection(MainSection, GeneralOptions)]
        public string Separator { get; set; } = string.Empty;

        [SettingsUISection(MainSection, GeneralOptions)]
        public TextFormatEnum TextFormat { get; set; } = TextFormatEnum.Value1;

        public DropdownItem<int>[] GetIntDropdownItems()
        {
            var items = new List<DropdownItem<int>>();

            for (var i = 0; i < 3; i += 1)
            {
                items.Add(new DropdownItem<int>() { value = i, displayName = i.ToString() });
            }

            return items.ToArray();
        }

        public enum TextFormatEnum
        {
            Value1,
            Value2,
        }

        [SettingsUIButton]
        [SettingsUIConfirmation]
        [SettingsUISection(MainSection, Actions)]
        public bool RestoreDefaults
        {
            set { SetDefaults(); }
        }

        [SettingsUISection(MainSection, Actions)]
        public bool EnableVerbose { get; set; }

        public override void SetDefaults()
        {
            EnableDefault = true;
            Separator = "•";
            TextFormat = TextFormatEnum.Value1;
            EnableVerbose = false;
        }

        [SettingsUISection(AboutTab, InfoGroup)]
        public string NameText => Mod.Name;

        [SettingsUISection(AboutTab, InfoGroup)]
        public string VersionText => Mod.Version;

        [SettingsUISection(AboutTab, InfoGroup)]
        public string AuthorText => "StarQ";

        [SettingsUIButtonGroup("Social")]
        [SettingsUIButton]
        [SettingsUISection(AboutTab, InfoGroup)]
        public bool BMaCLink
        {
            set
            {
                try
                {
                    Application.OpenURL($"https://buymeacoffee.com/starq");
                }
                catch (Exception e)
                {
                    Mod.log.Info(e);
                }
            }
        }
    }
}
