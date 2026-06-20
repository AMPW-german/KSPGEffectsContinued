#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GEffectsLogic;

namespace KSPGEffectsContinued
{
    public class KSPGEffectsLogicInstance : GEffectsLogicInstance, IDisposable
    {
        public static Dictionary<string, KSPGEffectsLogicInstance> KSPInstances { get; private set; } = new Dictionary<string, KSPGEffectsLogicInstance> ();

        public static string GetInstanceName(int index) => KSPInstances.FirstOrDefault(kvp => kvp.Value.UniqueID == index).Value?.VehicleId ?? "Unknown";
        
        public static KSPGEffectsLogicInstance GetLogicInstance(string vehicleId) => KSPInstances.FirstOrDefault(kvp => kvp.Key == vehicleId).Value ?? new KSPGEffectsLogicInstance(vehicleId);
        
        public string VehicleId { get; private set; }
        public bool Enabled { get; set; }

        public override void Update(double deltaTime, double currentGx, double currentGy, double currentGz)
        {
            if (Enabled) base.Update(deltaTime, currentGx, currentGy, currentGz);
        }
        
        public void Dispose()
        {
            KSPInstances.Remove(VehicleId);
        }

        public static void ClearInstances() => KSPInstances.ToList().ForEach(kvp => kvp.Value.Dispose());

        public KSPGEffectsLogicInstance(string vehicleId) : base()
        {
            VehicleId = vehicleId;
            KSPInstances.Add(VehicleId, this);
        }
    }
}
