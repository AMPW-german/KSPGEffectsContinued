#nullable enable
using UnityEngine;

namespace KSPGEffectsContinued.Logging
{
    public class GEffectsLogicLogging : GEffectsLogic.Logging.Logger
    {
        private const string LogPrefix = "GEffectsContinued [GEffectsLogicInstance] ";

        public override bool LogStr(string message, int id, LogLevel level = LogLevel.Debug)
        {
            string name = KSPGEffectsLogicInstance.GetInstanceName(id);
            
            switch (level)
            {
                case LogLevel.Debug:
                    if (GEffectsLogic.LogicSettings.DebugMode)
                        Debug.Log($"{LogPrefix}Debug ({name}): {message}");
                    break;
                case LogLevel.Info:
                    if (!GEffectsLogic.LogicSettings.SuppresInfoLogs)
                        Debug.Log($"{LogPrefix}Info ({name}): {message}");
                    break;
                case LogLevel.Warning:
                    Debug.LogWarning($"{LogPrefix}Warning ({name}): {message}");
                    break;
                case LogLevel.Error:
                    Debug.LogError($"{LogPrefix}Error ({name}): {message}");
                    break;
                default:
                    Debug.Log($"{LogPrefix}Unknown LogLevel ({name}): {message}");
                    break;
            }
            return true;
        }

        public GEffectsLogicLogging()
        {
            GEffectsLogicLogging.Instance = this;
        }
    }
}