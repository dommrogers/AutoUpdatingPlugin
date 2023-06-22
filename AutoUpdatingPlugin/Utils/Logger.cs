namespace AutoUpdatingPlugin
{
	internal static class Logger
	{

        internal static void Msg(string message) => MelonLoader.MelonLogger.Msg(message);
        internal static void Warning(string message) => MelonLoader.MelonLogger.Warning(message);
        internal static void Error(string message) => MelonLoader.MelonLogger.Error(message);

	}
}
