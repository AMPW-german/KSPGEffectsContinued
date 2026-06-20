#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KSPGEffectsContinued.Logging;
using UnityEngine;
using VehiclePhysics;
using Logger = GEffectsLogic.Logging.Logger;

namespace KSPGEffectsContinued
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class KSPGEffectsContinued : MonoBehaviour
    {
        const string APP_NAME = "GEffectsContinued";
        const string CONTROL_LOCK_ID = "GEffectsContinuedLock";
        public const double G_CONST = 9.81;

        //Specialization priority to be a commander
        Dictionary<string, int> priorities = new Dictionary<string, int>() {
            {"pilot", 3}, {"engineer", 2}, {"scientist", 2}, {"tourist", 0}
        };

        bool paused = false;

        protected void Start()
        {
            GameEvents.onGamePause.Add(onPause);
            GameEvents.onGameUnpause.Add(onUnPause);
            GameEvents.onCrash.Add(onCrewKilled);
            GameEvents.onCrashSplashdown.Add(onCrewKilled);
            //GameEvents.onCrewKilled.Add(onCrewKilled);

            ProtoCrewMember.doStockGCalcs = false;
        }

        protected void Awake()
        {
            KSPGEffectsLogicInstance.ClearInstances();
            new GEffectsLogicLogging();
        }

        protected void OnDestroy()
        {
            GameEvents.onGamePause.Remove(onPause);
            GameEvents.onGameUnpause.Remove(onUnPause);
            GameEvents.onCrash.Remove(onCrewKilled);
            GameEvents.onCrashSplashdown.Remove(onCrewKilled);
            //GameEvents.onCrewKilled.Remove(onCrewKilled);
        }

        private void applyControlLock()
        {
            if (InputLockManager.GetControlLock(CONTROL_LOCK_ID) != ControlTypes.None)
            {
                FlightGlobals.ActiveVessel.ctrlState.NeutralizeStick();
            }
        }

        void onPause()
        {
            paused = true;
        }

        void onUnPause()
        {
            paused = false;
        }

        void onCrewKilled(EventReport eventReport)
        {
        }


        public void FixedUpdate()
        {
            if (paused)
            {
                return;
            }
            
            double deltaTime = TimeWarp.fixedDeltaTime;

            Vessel activeVessel = FlightGlobals.ActiveVessel;
            List<Vessel> allVessels = FlightGlobals.Vessels.Where(v => v.vesselType != VesselType.SpaceObject).ToList();

            allVessels.ForEach(v =>
            {
                //Calculate G forces
                Vector3d gAcceleration = FlightGlobals.getGeeForceAtPosition(v.GetWorldPos3D()) - v.acceleration;
                Vector3d cabinAcceleration = v.transform.InverseTransformDirection(gAcceleration); //vessel.transform is an active part's transform
                cabinAcceleration /= -G_CONST;
                
                GEffectsModLogging.LogStr($"Vessel: {v.id}; Gx: {cabinAcceleration.x}; Gy: {cabinAcceleration.y}; Gz: {cabinAcceleration.z}", Logger.LogLevel.Debug);

                double bestVisibility = -1.0; // Smallest (best) visibility from all vessel crew to provide vision as long as possible
                
                v.GetVesselCrew().ForEach(pcm =>
                {
                    KSPGEffectsLogicInstance instance = KSPGEffectsLogicInstance.GetLogicInstance(pcm.name);
                    instance.Update(deltaTime, cabinAcceleration.x, cabinAcceleration.y, cabinAcceleration.z);
                });
            });
        }
    }
}
