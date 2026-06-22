#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KSPGEffectsContinued.Logging;
using KSPGEffectsContinued.Visuals;
using UnityEngine;
using VehiclePhysics;
using Logger = GEffectsLogic.Logging.Logger;

namespace KSPGEffectsContinued
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class KSPGEffectsContinued : MonoBehaviour
    {
        internal const string APP_NAME = "GEffectsContinued";
        internal const string CONTROL_LOCK_ID = "GEffectsContinuedLock";
        public const double G_CONST = 9.81;

        bool paused = false;

        protected void Start()
        {
            GameEvents.onGamePause.Add(OnPause);
            GameEvents.onGameUnpause.Add(OnUnPause);

            ProtoCrewMember.doStockGCalcs = false;
        }

        protected void Awake()
        {
            KSPGEffectsLogicInstance.ClearInstances();
            new GEffectsLogicLogging();

            KSPGEffectsVisual.initializeCameraFilter();
        }

        protected void OnDestroy()
        {
            GameEvents.onGamePause.Remove(OnPause);
            GameEvents.onGameUnpause.Remove(OnUnPause);
        }

        private void ApplyControlLock()
        {
            if (InputLockManager.GetControlLock(CONTROL_LOCK_ID) != ControlTypes.None)
            {
                FlightGlobals.ActiveVessel.ctrlState.NeutralizeStick();
            }
        }

        private void OnPause()
        {
            paused = true;
        }

        private void OnUnPause()
        {
            paused = false;
        }

        private void PassValuesToStock(ProtoCrewMember pcm, double consciousnessLevel)
        {
            GameParameters.AdvancedParams pars = HighLogic.CurrentGame.Parameters.CustomParams<GameParameters.AdvancedParams>();
            pcm.gExperienced =
                consciousnessLevel * PhysicsGlobals.KerbalGThresholdLOC * pars.KerbalGToleranceMult * ProtoCrewMember.GToleranceMult(pcm);
            
            if (consciousnessLevel < 0.05) pcm.SetInactive(60000, false);
            else pcm.SetInactive(0.0, false);
        }
        
        void OnGUI()
        {
            if (paused) KSPGEffectsVisual.drawGEffects(0.0f);
            else KSPGEffectsVisual.drawGEffects(tunnelVision);
        }

        float greyScale = 0.0f;
        float tunnelVision = 0.0f;

        public void FixedUpdate()
        {
            ApplyControlLock();
            
            if (paused)
            {
                return;
            }
            
            double deltaTime = TimeWarp.fixedDeltaTime;

            Vessel activeVessel = FlightGlobals.ActiveVessel;

            List<Vessel> allVessels = [.. FlightGlobals.Vessels.Where(v => v.vesselType != VesselType.SpaceObject && v != activeVessel)];
            allVessels.ForEach(v =>
            {
                //Calculate G forces
                Vector3d gAcceleration = FlightGlobals.getGeeForceAtPosition(v.GetWorldPos3D()) - v.acceleration;
                Vector3d cabinAcceleration = v.transform.InverseTransformDirection(gAcceleration); //vessel.transform is an active part's transform
                cabinAcceleration /= G_CONST; // Convert to Gs
                float accelerationLength = (float)cabinAcceleration.magnitude;
                //cabinAcceleration.z = -cabinAcceleration.z; // Invert z to match GLogic's coordinate system

                GEffectsModLogging.LogStr($"Vessel: {v.id}; Magnitude: {accelerationLength}; Gx: {cabinAcceleration.x}; Gy: {cabinAcceleration.y}; Gz: {cabinAcceleration.z}", Logger.LogLevel.Debug);

                v.GetVesselCrew().ForEach(pcm =>
                {
                    KSPGEffectsLogicInstance instance = KSPGEffectsLogicInstance.GetLogicInstance(pcm.name);
                    instance.Update(deltaTime, 0, 0, accelerationLength);
                });
            });

            //Calculate G forces
            Vector3d gAcceleration = FlightGlobals.getGeeForceAtPosition(activeVessel.GetWorldPos3D()) - activeVessel.acceleration;
            Vector3d cabinAcceleration = activeVessel.transform.InverseTransformDirection(gAcceleration); //vessel.transform is an active part's transform
            cabinAcceleration /= G_CONST; // Convert to Gs
            float accelerationLength = (float)cabinAcceleration.magnitude;

            GEffectsModLogging.LogStr($"Vessel: {activeVessel.id}; Magnitude: {accelerationLength}; Gx: {cabinAcceleration.x}; Gy: {cabinAcceleration.y}; Gz: {cabinAcceleration.z}", Logger.LogLevel.Debug);

            double bestConsciousness = -1.0; // Smallest (best) consciousness from all vessel crew to provide consciousness as long as possible
            tunnelVision = 0.0f;
            greyScale = 0.0f;

            activeVessel.GetVesselCrew().ForEach(pcm =>
            {
                KSPGEffectsLogicInstance instance = KSPGEffectsLogicInstance.GetLogicInstance(pcm.name);
                instance.Update(deltaTime, 0, 0, accelerationLength);

                if (instance.ConsciousnessLevel > bestConsciousness)
                {
                    bestConsciousness = instance.ConsciousnessLevel;
                    tunnelVision = (float)instance.TunnelVisionLevel;
                    greyScale = (float)instance.GreyScaleLevel;
                }

                PassValuesToStock(pcm, instance.ConsciousnessLevel);
            });

            if (bestConsciousness > 0 && bestConsciousness < 0.05) InputLockManager.SetControlLock(ControlTypes.ALL_SHIP_CONTROLS, CONTROL_LOCK_ID);
            else InputLockManager.RemoveControlLock(CONTROL_LOCK_ID);
            
        }
    }
}
