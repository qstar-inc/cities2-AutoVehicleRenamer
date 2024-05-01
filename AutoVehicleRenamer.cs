// AutoVehicleRenamer.cs
// https://github.com/qstar-inc/cities2-AutoVehicleRenamer
// StarQ 2024

using Game;
using Game.Common;
using Game.Tools;
using Game.UI;
using Unity.Collections;
using Unity.Entities;

namespace AutoVehicleRenamer
{
    public partial class AutoVehicleRenamer : GameSystemBase
    {
        private NameSystem nameSystem;
        private EntityQuery _query;

        protected override void OnCreate()
        {
            base.OnCreate();

            _query = GetEntityQuery(new EntityQueryDesc()
            {
                All = [
                    ComponentType.ReadOnly<Game.Vehicles.Vehicle>(),
                    ComponentType.ReadOnly<Owner>(),
                    ComponentType.ReadOnly<Created>()
                ],
                None =
                [
                    ComponentType.ReadOnly<Game.Vehicles.WorkVehicle>(),
                    ComponentType.ReadOnly<Game.Vehicles.DeliveryTruck>(),
                    ComponentType.ReadOnly<Game.Vehicles.PersonalCar>(),
                    ComponentType.ReadOnly<Deleted>(),
                    ComponentType.ReadOnly<Temp>()
                ]
            });
            RequireForUpdate(_query);

            nameSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<NameSystem>();
        }

        protected override void OnUpdate()
        {
            UpdateVehicleName();
        }


        private void UpdateVehicleName()
        {
            var m_Setting = Mod.m_Setting;

            bool enableDefaults = m_Setting.enableDefault;
            string separator = m_Setting.separator;
            bool enableVerbose = m_Setting.enableVerbose;

            var vehicles = _query.ToEntityArray(Allocator.Temp);

            foreach (var entity in vehicles)
            {
                var vehicleName = nameSystem.GetRenderedLabelName(entity).ToString();
                switch (vehicleName)
                {
                    case "Park Maintenance Vehicle":
                        if (enableVerbose) { Mod.log.Info("Found \"Park Maintenance Vehicle\", using \"Park MV\""); }
                        vehicleName = "Park MV";
                        break;
                    case "Road Maintenance Vehicle":
                        if (enableVerbose) { Mod.log.Info("Found \"Road Maintenance Vehicle\", using \"Road MV\""); }
                        vehicleName = "Road MV";
                        break;
                    default:
                        break;
                }

                var ownerEntity = EntityManager.GetComponentData<Owner>(entity);
                string buildingName = "";

                if (nameSystem.TryGetCustomName(ownerEntity.m_Owner, out var ownerNameCustomName))
                {
                    buildingName = ownerNameCustomName;
                }
                else
                {
                    if (enableDefaults == true)
                    {
                        var defaultName = nameSystem.GetRenderedLabelName(ownerEntity.m_Owner).ToString();
                        buildingName = defaultName;
                    }
                }
                if (buildingName != "")
                {
                    string format = m_Setting.textFormat.ToString();
                    switch (format)
                    {
                        case "Value1":
                            if (enableVerbose) { Mod.log.Info($"Renaming \"{entity}\" to \"{vehicleName} {separator} {buildingName}\""); }
                            nameSystem.SetCustomName(entity, $"{vehicleName} {separator} {buildingName}");
                            break;
                        case "Value2":
                            if (enableVerbose) { Mod.log.Info($"Renaming \"{entity}\" to \"{buildingName} {separator} {vehicleName}\""); }
                            nameSystem.SetCustomName(entity, $"{buildingName} {separator} {vehicleName}");
                            break;
                        default:
                            break;
                    }

                }
            }

        }

        protected override void OnDestroy()
        {
        }
    }
}
