using AutoVehicleRenamer.Systems;
using Colossal.IO.AssetDatabase;
using Colossal.Json;
using Game.Modding;
using Game.Settings;
using StarQ.Shared.Extensions;
using Unity.Entities;

namespace AutoVehicleRenamer
{
    [FileLocation("ModsSettings\\StarQ\\" + nameof(AutoVehicleRenamer))]
    [SettingsUITabOrder(GeneralTab, AboutTab, LogTab)]
    public class Setting : ModSetting
    {
        public Setting(IMod mod)
            : base(mod) => SetDefaults();

        public const string GeneralTab = "GeneralTab";
        public const string GeneralGroup = "GeneralGroup";

        public const string AboutTab = "AboutTab";
        public const string InfoGroup = "InfoGroup";

        public const string LogTab = "LogTab";

        [Exclude]
        [SettingsUIHidden]
        public bool IsInGameOrEditor { get; set; } = false;

        [Exclude]
        [SettingsUIHidden]
        public bool IsDetailedDescriptionsRunning { get; set; } = false;

        [SettingsUIButton]
        [SettingsUISection(GeneralTab, GeneralGroup)]
        [SettingsUIDisableByCondition(typeof(Setting), nameof(IsInGameOrEditor), true)]
        public bool ApplyToAll
        {
            set =>
                World
                    .DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<AutoVehicleRenamerSystem>()
                    .UpdateVehicleName(true);
        }

        [SettingsUISection(GeneralTab, GeneralGroup)]
        public bool EnableDefault { get; set; }

        [SettingsUITextInput]
        [SettingsUISection(GeneralTab, GeneralGroup)]
        public string Separator { get; set; } = string.Empty;

        [SettingsUISection(GeneralTab, GeneralGroup)]
        public TextFormatEnum TextFormat { get; set; } = TextFormatEnum.Vehicle_Separator_Building;

        public enum TextFormatEnum
        {
            Vehicle_Separator_Building,
            Building_Separator_Vehicle,
        }

        [SettingsUIButton]
        [SettingsUIConfirmation]
        [SettingsUISection(GeneralTab, GeneralGroup)]
        public bool RestoreDefaults
        {
            set => SetDefaults();
        }

        public override void SetDefaults()
        {
            EnableDefault = true;
            Separator = "•";
            TextFormat = TextFormatEnum.Vehicle_Separator_Building;
        }

        [SettingsUISection(AboutTab, InfoGroup)]
        public string NameText => Mod.Name;

        [SettingsUISection(AboutTab, InfoGroup)]
        public string VersionText => VariableHelper.AddDevSuffix(Mod.Version);

        [SettingsUISection(AboutTab, InfoGroup)]
        public string AuthorText => VariableHelper.StarQ;

        [SettingsUIButton]
        [SettingsUIButtonGroup("Social")]
        [SettingsUISection(AboutTab, InfoGroup)]
        public bool BMaCLink
        {
            set => VariableHelper.OpenBMAC();
        }

        [SettingsUIButton]
        [SettingsUIButtonGroup("Social")]
        [SettingsUISection(AboutTab, InfoGroup)]
        public bool Discord
        {
            set => VariableHelper.OpenDiscord("1234887506336682065");
        }

        [SettingsUIMultilineText]
        [SettingsUIDisplayName(typeof(LogHelper), nameof(LogHelper.LogText))]
        [SettingsUISection(LogTab, "")]
        public string LogText => string.Empty;

        [Exclude]
        [SettingsUIHidden]
        public bool IsLogMissing
        {
            get => VariableHelper.CheckLog(Mod.Id);
        }

        [SettingsUIButton]
        [SettingsUIDisableByCondition(typeof(Setting), nameof(IsLogMissing))]
        [SettingsUISection(LogTab, "")]
        public bool OpenLog
        {
            set => VariableHelper.OpenLog(Mod.Id);
        }
    }
}
