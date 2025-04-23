// AutoVehicleRenamer.cs
// https://github.com/qstar-inc/cities2-AutoVehicleRenamer
// StarQ 2024

using Colossal.Entities;
using Colossal.Serialization.Entities;
using Game;
using Game.Common;
using Game.Companies;
using Game.Prefabs;
using Game.SceneFlow;
using Game.Tools;
using Game.UI;
using Game.UI.InGame;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace AutoVehicleRenamer
{
    public partial class AutoVehicleRenamer : GameSystemBase
    {
        private PrefabUISystem prefabUISystem;
        private PrefabSystem prefabSystem;
        private NameSystem nameSystem;
        private EntityQuery vehicleQuery;
        private EntityQuery vehicleQueryAll;

        protected override void OnCreate()
        {
            base.OnCreate();

            prefabUISystem = World.GetExistingSystemManaged<PrefabUISystem>();
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

        protected override void OnUpdate()
        {
            UpdateVehicleName();
        }

        protected override void OnGameLoadingComplete(Purpose purpose, GameMode mode)
        {
            base.OnGameLoadingComplete(purpose, mode);
            if (mode.IsGameOrEditor())
            {
                Mod.m_Setting.IsInGameOrEditor = true;
            }
            else
            {
                Mod.m_Setting.IsInGameOrEditor = false;
            }
        }

        private string GetId(Entity entity)
        {
            if (entity == Entity.Null)
            {
                return null;
            }
            Entity entity2 = Entity.Null;
            if (base.EntityManager.TryGetComponent(entity, out PrefabRef prefabRef))
            {
                entity2 = prefabRef.m_Prefab;
            }

            if (!(entity2 != Entity.Null))
            {
                return string.Empty;
            }

            if (!prefabSystem.TryGetPrefab(entity2, out PrefabBase prefabBase))
            {
                return prefabSystem.GetObsoleteID(entity2).GetName();
            }

            if (!prefabBase.TryGet(out Localization localization))
            {
                prefabUISystem.GetTitleAndDescription(entity2, out string text, out _);
                return text;
            }
            return localization.m_LocalizationID;
        }

        public string GetNameNonRecursive(Entity entity)
        {
            string id = GetId(entity);
            if (
                !GameManager.instance.localizationManager.activeDictionary.TryGetValue(
                    id,
                    out string text2
                )
            )
            {
                return id;
            }
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
                string truncated = input.Substring(0, length);
                if (utf8.GetByteCount(truncated) <= maxBytes)
                    return truncated;

                length--;
            }

            return string.Empty;
        }

        public void UpdateVehicleName(bool all = false)
        {
            var setting = Mod.m_Setting;
            bool enableDefaults = setting.EnableDefault;
            string separatorStr = setting.Separator.Trim();
            bool enableVerbose = setting.EnableVerbose;
            string format = setting.TextFormat.ToString();

            FixedString32Bytes separator;
            separator = new FixedString32Bytes("");
            separator.Append(new FixedString32Bytes(SanitizeStringToBytes(separatorStr, 6)));
            separator.Append(new FixedString32Bytes(""));

            EntityQuery query = all ? vehicleQueryAll : vehicleQuery;
            NativeArray<Entity> entities = query.ToEntityArray(Allocator.Temp);
            NativeArray<FixedString64Bytes> vehicleNames = new(entities.Length, Allocator.TempJob);
            NativeArray<FixedString64Bytes> buildingNames = new(entities.Length, Allocator.TempJob);
            NativeArray<FixedString128Bytes> resultNames = new(entities.Length, Allocator.TempJob);

            for (int i = 0; i < entities.Length; i++)
            {
                var entity = entities[i];
                var vehicleName = GetNameNonRecursive(entity);

                switch (vehicleName)
                {
                    case "Park Maintenance Vehicle":
                        vehicleName = "Park MV";
                        break;

                    case "Road Maintenance Vehicle":
                        vehicleName = "Road MV";
                        break;
                }

                vehicleNames[i] = new FixedString64Bytes(SanitizeStringToBytes(vehicleName, 60));

                var ownerEntity = EntityManager.GetComponentData<Owner>(entity).m_Owner;

                string buildingName = "";

                if (nameSystem.TryGetCustomName(ownerEntity, out var custom))
                {
                    buildingName = custom;
                }
                else if (enableDefaults)
                {
                    buildingName = nameSystem.GetRenderedLabelName(ownerEntity).ToString();
                }

                if (buildingName.StartsWith("Assets.NAME"))
                {
                    if (EntityManager.TryGetComponent(ownerEntity, out CompanyData companyData))
                    {
                        buildingName = nameSystem
                            .GetRenderedLabelName(companyData.m_Brand)
                            .ToString();
                    }
                }

                buildingNames[i] = new FixedString64Bytes(SanitizeStringToBytes(buildingName, 60));
                ;
            }

            var job = new RenameVehicleJob
            {
                separator = separator,
                formatValue = format == "Value1" ? 1 : 2,
                vehicleNames = vehicleNames,
                buildingNames = buildingNames,
                resultNames = resultNames,
            };

            JobHandle handle = job.ScheduleParallel(entities.Length, 8, default);
            handle.Complete();

            for (int i = 0; i < entities.Length; i++)
            {
                if (resultNames[i].Length > 0)
                {
                    if (enableVerbose)
                    {
                        Mod.log.Info($"Renaming {entities[i]} to \"{resultNames[i]}\"");
                    }

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
                {
                    return;
                }

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

        //public void UpdateVehicleName(bool all = false)
        //{
        //    vehicleQuery = SystemAPI
        //        .QueryBuilder()
        //        .WithAll<Game.Vehicles.Vehicle, Owner, Created>()
        //        .WithNone<Game.Vehicles.WorkVehicle, Game.Vehicles.PersonalCar, Deleted, Temp>()
        //        .Build();

        //    vehicleQueryAll = SystemAPI
        //        .QueryBuilder()
        //        .WithAll<Game.Vehicles.Vehicle, Owner>()
        //        .WithNone<Game.Vehicles.WorkVehicle, Game.Vehicles.PersonalCar, Deleted, Temp>()
        //        .Build();

        //    try
        //    {
        //        var setting = Mod.m_Setting;

        //        bool enableDefaults = setting.EnableDefault;
        //        string separator = setting.Separator;
        //        bool enableVerbose = setting.EnableVerbose;
        //        NativeArray<Entity> vehicles;
        //        if (all)
        //        {
        //            vehicles = vehicleQueryAll.ToEntityArray(Allocator.Temp);
        //        }
        //        else
        //        {
        //            vehicles = vehicleQuery.ToEntityArray(Allocator.Temp);
        //        }

        //        foreach (var entity in vehicles)
        //        {
        //            var vehicleName = GetNameNonRecursive(entity);
        //            switch (vehicleName)
        //            {
        //                case "Park Maintenance Vehicle":
        //                    if (enableVerbose)
        //                        Mod.log.Info(
        //                            "Found \"Park Maintenance Vehicle\", using \"Park MV\""
        //                        );
        //                    vehicleName = "Park MV";
        //                    break;
        //                case "Road Maintenance Vehicle":
        //                    if (enableVerbose)
        //                        Mod.log.Info(
        //                            "Found \"Road Maintenance Vehicle\", using \"Road MV\""
        //                        );
        //                    vehicleName = "Road MV";
        //                    break;
        //                default:
        //                    break;
        //            }

        //            var ownerEntity = EntityManager.GetComponentData<Owner>(entity);
        //            string buildingName = "";

        //            if (
        //                nameSystem.TryGetCustomName(
        //                    ownerEntity.m_Owner,
        //                    out var ownerNameCustomName
        //                )
        //            )
        //            {
        //                buildingName = ownerNameCustomName;
        //            }
        //            else
        //            {
        //                if (enableDefaults == true)
        //                {
        //                    var defaultName = nameSystem
        //                        .GetRenderedLabelName(ownerEntity.m_Owner)
        //                        .ToString();
        //                    buildingName = defaultName;
        //                }
        //            }
        //            if (buildingName != "")
        //            {
        //                if (buildingName.StartsWith("Assets.NAME"))
        //                {
        //                    EntityManager.TryGetComponent(
        //                        ownerEntity.m_Owner,
        //                        out CompanyData companyData
        //                    );

        //                    buildingName = nameSystem
        //                        .GetRenderedLabelName(companyData.m_Brand)
        //                        .ToString();
        //                }

        //                string format = setting.TextFormat.ToString();
        //                switch (format)
        //                {
        //                    case "Value1":
        //                        if (enableVerbose)
        //                            Mod.log.Info(
        //                                $"Renaming \"{entity}\" to \"{vehicleName} {separator} {buildingName}\""
        //                            );
        //                        nameSystem.SetCustomName(
        //                            entity,
        //                            $"{vehicleName} {separator} {buildingName}"
        //                        );
        //                        break;
        //                    case "Value2":
        //                        if (enableVerbose)
        //                            Mod.log.Info(
        //                                $"Renaming \"{entity}\" to \"{buildingName} {separator} {vehicleName}\""
        //                            );
        //                        nameSystem.SetCustomName(
        //                            entity,
        //                            $"{buildingName} {separator} {vehicleName}"
        //                        );
        //                        break;
        //                    default:
        //                        break;
        //                }
        //            }
        //        }
        //        vehicles.Dispose();
        //    }
        //    catch (Exception ex)
        //    {
        //        Mod.log.Info(ex);
        //    }
        //}
    }
}
