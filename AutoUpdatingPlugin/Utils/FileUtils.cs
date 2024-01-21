using System;
using System.IO;

namespace AutoUpdatingPlugin
{
	internal static class FileUtils
	{

		internal static string PluginsFolder { get; }
		internal static string ModsFolder { get; }

		static FileUtils()
		{
			PluginsFolder = Path.Combine(Implementation.GameDirectory, "Plugins");
			ModsFolder = Path.Combine(Implementation.GameDirectory, "Mods");
		}

		public static string GetDestination(string link)
		{
			return string.IsNullOrWhiteSpace(link)
				? throw new ArgumentException("Invalid link argument")
				: Path.Combine(ModsFolder, Path.GetFileName(link));
		}

		public static string GetDestinationPlugin(string link)
		{
			return string.IsNullOrWhiteSpace(link)
				? throw new ArgumentException("Invalid link argument")
				: Path.Combine(PluginsFolder, Path.GetFileName(link));
		}

		internal static string GetPathSelf()
		{
			return Path.Combine(FileUtils.PluginsFolder, BuildInfo.Name + ".dll");
		}
	}
}
