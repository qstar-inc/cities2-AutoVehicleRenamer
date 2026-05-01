using System;
using System.Text.RegularExpressions;
using Colossal.Entities;
using Colossal.Serialization.Entities;
using Game;
using Game.Common;
using Game.Companies;
using Game.Prefabs;
using Game.SceneFlow;
using Game.Tools;
using Game.UI;
using StarQ.Shared.Extensions;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace AutoVehicleRenamer.Systems
{
    public partial class AutoVehicleRenamerSystem : GameSystemBase
    {
        private PrefabSystem prefabSystem;
        private NameSystem nameSystem;
        private EntityQuery vehicleQuery;
        private EntityQuery vehicleQueryAll;

        private static readonly Regex regex = new(
            @"^(.*?)(?: ?\([\w]+ [\d\- ]+\)| ?\[[\d\- ]+ [\w]+\])$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant
        );

        protected override void OnCreate()
        {
            base.OnCreate();

            prefabSystem = World.GetExistingSystemManaged<PrefabSystem>();
            nameSystem = World.GetOrCreateSystemManaged<NameSystem>();

            vehicleQuery = SystemAPI
                .QueryBuilder()
                .WithAll<Game.Vehicles.Vehicle, Owner, Created>()
                .WithNone<Game.Vehicles.WorkVehicle, Game.Vehicles.PersonalCar, Deleted, Temp>()
                .Build();

            vehicleQueryAll = SystemAPI
                .QueryBuilder()
                .WithAll<Game.Vehicles.Vehicle, Owner>()
                .WithNone<Game.Vehicles.WorkVehicle, Game.Vehicles.PersonalCar, Deleted, Temp>()
                .Build();
        }

        protected override void OnUpdate() => UpdateVehicleName();

        protected override void OnGameLoadingComplete(Purpose purpose, GameMode mode)
        {
            base.OnGameLoadingComplete(purpose, mode);
            Mod.m_Setting.IsInGameOrEditor = mode.IsGameOrEditor();
        }

        private string GetId(Entity entity)
        {
            if (entity == Entity.Null)
                return null;

            Entity prefabEntity = Entity.Null;
            if (EntityManager.TryGetComponent(entity, out PrefabRef prefabRef))
                prefabEntity = prefabRef.m_Prefab;

            if (!(prefabEntity != Entity.Null))
                return string.Empty;

            if (!prefabSystem.TryGetPrefab(prefabEntity, out PrefabBase prefabBase))
                return prefabSystem.GetObsoleteID(prefabEntity).GetName();

            if (!prefabBase.TryGet(out Localization localization))
                return $"Assets.NAME[{prefabBase.name}]";

            return localization.m_LocalizationID;
        }

        public string GetNameNonRecursive(Entity entity)
        {
            string id = GetId(entity);
            if (id == null)
                return string.Empty;

            if (
                !GameManager.instance.localizationManager.activeDictionary.TryGetValue(
                    id,
                    out string text2
                )
            )
                return id;

            return text2;
        }

        public static string SanitizeStringToBytes(string input, int maxBytes)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            var utf8 = System.Text.Encoding.UTF8;
            byte[] inputBytes = utf8.GetBytes(input);

            if (inputBytes.Length <= maxBytes)
                return input;

            int length = input.Length;
            while (length > 0)
            {
                string truncated = input[..length];
                if (utf8.GetByteCount(truncated) <= maxBytes)
                    return truncated;

                length--;
            }

            return string.Empty;
        }

        public void UpdateVehicleName(bool all = false)
        {
            Setting setting = Mod.m_Setting;
            bool enableDefaults = setting.EnableDefault;
            string separatorStr = setting.Separator.Trim();
            string format = setting.TextFormat.ToString();
            bool detailed = setting.IsDetailedDescriptionsRunning;

            FixedString32Bytes separator = default;
            separator.Append(' ');
            separator.Append(SanitizeStringToBytes(separatorStr, 6));
            separator.Append(' ');

            EntityQuery query = all ? vehicleQueryAll : vehicleQuery;
            NativeArray<Entity> entities = query.ToEntityArray(Allocator.Temp);
            int count = entities.Length;
            if (count == 0)
                return;

            NativeArray<FixedString64Bytes> vehicleNames = new(entities.Length, Allocator.TempJob);
            NativeArray<FixedString64Bytes> buildingNames = new(entities.Length, Allocator.TempJob);
            NativeArray<FixedString128Bytes> resultNames = new(entities.Length, Allocator.TempJob);

            for (int i = 0; i < count; i++)
            {
                Entity entity = entities[i];
                string vehicleName = GetNameNonRecursive(entity);

                if (detailed)
                {
                    var match = regex.Match(vehicleName);
                    if (match.Success)
                        vehicleName = match.Groups[1].Value;
                }

                if (vehicleName == "Park Maintenance Vehicle")
                    vehicleName = "Park MV";
                else if (vehicleName == "Road Maintenance Vehicle")
                    vehicleName = "Road MV";

                vehicleNames[i] = new FixedString64Bytes(SanitizeStringToBytes(vehicleName, 60));

                Entity ownerEntity = EntityManager.GetComponentData<Owner>(entity).m_Owner;

                string buildingName = "";

                if (nameSystem.TryGetCustomName(ownerEntity, out var custom))
                    buildingName = custom;
                else if (enableDefaults)
                    buildingName = nameSystem.GetRenderedLabelName(ownerEntity).ToString();

                if (buildingName.StartsWith("Assets.NAME", StringComparison.Ordinal))
                {
                    if (EntityManager.TryGetComponent(ownerEntity, out CompanyData companyData))
                        buildingName = nameSystem
                            .GetRenderedLabelName(companyData.m_Brand)
                            .ToString();
                }

                buildingNames[i] = new FixedString64Bytes(SanitizeStringToBytes(buildingName, 60));
            }

            var job = new RenameVehicleJob
            {
                separator = separator,
                formatValue = format == "Value1" ? 1 : 2,
                vehicleNames = vehicleNames,
                buildingNames = buildingNames,
                resultNames = resultNames,
            };
            job.Run(count);
            //JobHandle handle = job.ScheduleParallel(entities.Length, 8, default);
            //handle.Complete();

            for (int i = 0; i < entities.Length; i++)
            {
                if (resultNames[i].Length > 0)
                {
                    LogHelper.SendLog(
                        $"Renaming {entities[i]} to \"{resultNames[i]}\"",
                        LogLevel.DEV
                    );

                    nameSystem.SetCustomName(entities[i], resultNames[i].ToString());
                }
            }

            entities.Dispose();
            vehicleNames.Dispose();
            buildingNames.Dispose();
            resultNames.Dispose();
        }

        [BurstCompile]
        public struct RenameVehicleJob : IJobFor
        {
            [ReadOnly]
            public NativeArray<Entity> entities;

            [ReadOnly]
            public NativeArray<FixedString64Bytes> vehicleNames;

            [ReadOnly]
            public NativeArray<FixedString64Bytes> buildingNames;

            [ReadOnly]
            public FixedString32Bytes separator;

            [ReadOnly]
            public int formatValue;

            [WriteOnly]
            public NativeArray<FixedString128Bytes> resultNames;

            public void Execute(int index)
            {
                var vehicleName = vehicleNames[index];
                var buildingName = buildingNames[index];

                if (vehicleName.Length == 0 || buildingName.Length == 0)
                    return;

                FixedString128Bytes result;

                if (formatValue == 1)
                {
                    result = new FixedString128Bytes(vehicleName);
                    result.Append(separator);
                    result.Append(buildingName);
                }
                else
                {
                    result = new FixedString128Bytes(buildingName);
                    result.Append(separator);
                    result.Append(vehicleName);
                }

                resultNames[index] = result;
            }
        }
    }
}
