using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.IO;

namespace AutoUpdatingPlugin
{
	internal static class InstalledModList
	{
		internal static readonly Dictionary<string, InstalledModDetail> installedMods = new Dictionary<string, InstalledModDetail>();

		internal static string[] GetInstalledModNames() => installedMods.Keys.ToArray();

		internal static string[] GetSortedModNames()
		{
			List<string>? result = new List<string>(installedMods.Keys.ToArray());
			result.Sort();
			return result.ToArray();
		}

		internal static Dictionary<string, InstalledModDetail> SortedDictionary()
		{
			IOrderedEnumerable<KeyValuePair<string, InstalledModDetail>>? sortedDict = from entry in installedMods orderby entry.Key ascending select entry;
			return sortedDict.ToDictionary(pair => pair.Key, pair => pair.Value);
		}

		internal static void ScanModFolder()
		{
			//			Logger.Msg("Scanning for installed mods...");
			installedMods.Clear();

			ModComponentScanner.ScanForModComponentFiles();

			//has to run first
			DllFileChecker.ScanForDllFiles(FileUtils.ModsFolder);
			DllFileChecker.ScanForDllFiles(FileUtils.PluginsFolder);

#if DEBUG
			File.WriteAllText("installedMods.json", JsonSerializer.Serialize(installedMods, new JsonSerializerOptions() { WriteIndented = true }));
			Logger.Minor("Write installedMods.json");
#endif


			Logger.Minor("Found " + installedMods.Count + " unique non-dev mods installed");
		}
	}
}
