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
				MelonLoader.MelonLogger.Msg(message);
			} else
			{
				Console.WriteLine(message);
			}
		}
		internal static void Warning(string message)
		{
			if (Implementation.UseMelonLoader)
			{
				MelonLoader.MelonLogger.Warning(message);
			}
			else
			{
				Console.WriteLine(message);
			}
		}

		internal static void Error(string message)
		{
			if (Implementation.UseMelonLoader)
			{
				MelonLoader.MelonLogger.Error(message);
			}
			else
			{
				Console.WriteLine(message);
			}
		}

		internal static void Success(string message)
		{
			if (Implementation.UseMelonLoader)
			{
				MelonLoader.MelonLogger.Msg(ConsoleColor.Green, message);
			}
			else
			{
				Console.WriteLine(message);
			}
		}

		internal static void Minor(string message)
		{
			if (Implementation.UseMelonLoader)
			{
				MelonLoader.MelonLogger.Msg(ConsoleColor.DarkGray, message);
			}
			else
			{
				Console.WriteLine(message);
			}
		}

	}
}
