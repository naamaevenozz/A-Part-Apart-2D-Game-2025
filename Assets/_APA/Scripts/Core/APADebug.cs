
using System;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace _APA.Scripts
{
    public class APADebug
    {
        [Conditional("LOGS_ENABLE")]
        public static void Log(object message)
        {
            Debug.Log("APA Log: " + message);
        }

        [Conditional("LOGS_ENABLE")]
        public static void LogException(object message)
        {
            Debug.LogException(new Exception("APA LogException: " + message.ToString()));
        }

        [Conditional("LOGS_ENABLE")]
        public static void LogError(object message)
        {
            Debug.LogError("APA LogError: " + message);
        }
        [Conditional("LOGS_ENABLE")]
        public static void LogWarning(object message)
        {
            Debug.LogWarning("APA LogWarning: " + message);
        }
    }
}
