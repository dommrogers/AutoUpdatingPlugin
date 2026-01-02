using System;
using System.Linq;
using System.Reflection;

namespace AutoUpdatingPlugin
{
	internal static class Logger
	{

		internal static void Msg(string message)
		{
			if (Implementation.UseMelonLoader)
			{
				MelonLoader.MelonLogger.Msg("AUP: "+message);
			}
			else
			{
				Console.WriteLine("AUP: "+message);
			}
		}

		internal static void Debug(string message)
		{
#if DEBUG
			if (Implementation.UseMelonLoader)
			{
				MelonLoader.MelonLogger.Msg("## AUP: "+message);
			}
			else
			{
				Console.WriteLine("## AUP: " + message);
			}
#endif
		}
		internal static void Warning(string message)
		{
			if (Implementation.UseMelonLoader)
			{
				MelonLoader.MelonLogger.Warning("!! AUP: "+message);
			}
			else
			{
				Console.WriteLine("!! AUP: "+message);
			}
		}

		internal static void Error(string message)
		{
			if (Implementation.UseMelonLoader)
			{
				MelonLoader.MelonLogger.Error("?? AUP: "+message);
			}
			else
			{
				Console.WriteLine("?? AUP: "+message);
			}
		}

		internal static void Success(string message)
		{
			if (Implementation.UseMelonLoader)
			{
				MelonLoader.MelonLogger.Msg(ConsoleColor.Green, "AUP: "+message);
			}
			else
			{
				Console.WriteLine("AUP: "+message);
			}
		}

		internal static void Minor(string message)
		{
			if (Implementation.UseMelonLoader)
			{
				MelonLoader.MelonLogger.Msg(ConsoleColor.DarkGray, "AUP: "+message);
			}
			else
			{
				Console.WriteLine("AUP: "+message);
			}
		}

	}
}
