#nullable enable
using UnityEngine;
using static GEffectsLogic.Logging.Logger;

namespace KSPGEffectsContinued.Logging
{
    public static class GEffectsModLogging
    {
        private const string LogPrefix = KSPGEffectsContinued.APP_NAME + " [mod]";

        public static bool LogStr(string message, LogLevel level = LogLevel.Debug)
        {
            switch (level)
            {
                case LogLevel.Debug:
                    if (GEffectsLogic.LogicSettings.DebugMode)
                    {
                        Debug.Log(LogPrefix + " Debug: " + message);
                    }
                    break;
                case LogLevel.Info:
                    if (!GEffectsLogic.LogicSettings.SuppresInfoLogs)
                        Debug.Log(LogPrefix + " Info: " + message);
                    break;
                case LogLevel.Warning:
                    Debug.LogWarning(LogPrefix + " Warning: " + message);
                    break;
                case LogLevel.Error:
                    Debug.LogError(LogPrefix + " Error: " + message);
                    break;
                default:
                    Debug.Log(LogPrefix + " Unknown LogLevel: " + message);
                    break;
            }
            return true;
        }
    }
}