using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;

namespace AutoUpdatingPlugin
{
	internal static class APIList
	{
		private static readonly Dictionary<string, string> oldToNewModNames = new Dictionary<string, string>()
		{
			// Used in case something is missing on the API
		};

		internal static readonly Dictionary<string, APIMod> allMods = new Dictionary<string, APIMod>();
		internal static readonly Dictionary<string, APIMod> supportedMods = new Dictionary<string, APIMod>();
		internal static void FetchRemoteMods()
		{
			Stopwatch sw = new Stopwatch();
			sw.Start();
//			Logger.Minor("Fetching remote mods...");
			string apiResponse = "";
			using (WebClient? client = new WebClient())
			{
				client.Headers["User-Agent"] = "AutoUpdatingPlugin";
	//			Logger.Minor("Attempting to download from API site...");
				apiResponse = client.DownloadString("http://tld.xpazeapps.com/api.json");
			}
			Logger.Msg($"Downloaded mod API data ({(float)sw.ElapsedMilliseconds/1000:N2}s)");

			APIMod[] apiMods = APIReader.Deserialize(apiResponse);

			supportedMods.Clear();

			List<string> disabledByAuthor = new List<string>();
			List<string> disabledByLargeFile = new List<string>();
			List<string> disabledNoValidLink = new List<string>();


			foreach (APIMod mod in apiMods)
			{
				allMods.Add(mod.name, mod);

				if (!mod.enableUpdate)
				{
					disabledByAuthor.Add(mod.name);
//					Logger.Msg($"Automatic updating for {mod.name} has been disabled by the mod author.");
					continue;
				}

				if (mod.ContainsModSceneFile())
				{
					disabledByLargeFile.Add(mod.name);
//					Logger.Msg($"Automatic updating for {mod.name} has been disabled due to potentially large file sizes.");
					continue;
				}

				if (mod.downloadlinks.Length == 0)
				{
					disabledNoValidLink.Add(mod.name);
//					Logger.Msg($"Automatic updating for {mod.name} has been disabled due to having no valid download links.");
					continue;
				}

				// Aliases
				foreach (string alias in mod.aliases)
				{
					if (alias != mod.name && !oldToNewModNames.ContainsKey(alias))
					{
						oldToNewModNames[alias] = mod.name;
					}
				}

				// Add to known mods
				supportedMods.Add(mod.name, mod);
			}

			if (disabledByAuthor.Count > 0)
			{
				Logger.Msg($"# Update Disabled - By Author:");
				Logger.Minor(string.Join(", ", disabledByAuthor));
			}
			if (disabledByLargeFile.Count > 0)
			{
				Logger.Msg($"# Update Disabled - Has Large Files:");
				Logger.Minor(string.Join(", ", disabledByLargeFile));
			}
			if (disabledNoValidLink.Count > 0)
			{
				Logger.Msg($"# Update Disabled - No Valid Link:");
				Logger.Minor(string.Join(", ", disabledNoValidLink));
			}
			sw.Stop();
			Logger.Msg("API Mods " + apiMods.Length + ", Supported " + supportedMods.Count + $" ({(float)sw.ElapsedMilliseconds/1000:N2}s)");
		}
		internal static string GetNewModName(string currentName)
		{
			return oldToNewModNames.TryGetValue(currentName, out string? newName) ? newName : currentName;
		}
		internal static bool IsAliasName(string currentName) => oldToNewModNames.ContainsKey(currentName);

		internal static string[] GetModNames() => supportedMods.Keys.ToArray();
		internal static string[] GetSortedModNames()
		{
			List<string>? result = new List<string>(supportedMods.Keys.ToArray());
			result.Sort();
			return result.ToArray();
		}

		internal static Dictionary<string, APIMod> SortedDictionary()
		{
			IOrderedEnumerable<KeyValuePair<string, APIMod>>? sortedDict = from entry in supportedMods orderby entry.Key ascending select entry;
			return sortedDict.ToDictionary(pair => pair.Key, pair => pair.Value);
		}
	}
}