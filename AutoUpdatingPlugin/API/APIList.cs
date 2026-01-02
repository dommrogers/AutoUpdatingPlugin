using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;

namespace AutoUpdatingPlugin
{
	internal static class APIList
	{
		private static readonly Dictionary<string, string> aliasNames = new Dictionary<string, string>()
		{
			// Used in case something is missing on the API
		};

		private static readonly Dictionary<string, string> replacements = new Dictionary<string, string>();

		internal static Dictionary<string, APIMod> allMods = new Dictionary<string, APIMod>();
		internal static readonly Dictionary<string, APIMod> validMods = new Dictionary<string, APIMod>();
		internal static readonly Dictionary<string, APIMod> supportedMods = new Dictionary<string, APIMod>();

		internal static List<string> disabledByAuthor { get; set; }  = new List<string>();
		internal static List<string> disabledNoValidLink { get; set; } = new List<string>();

		internal static void FetchRemoteMods()
		{
			Logger.Minor("Attempting to download from API site...");

			Stopwatch sw = new Stopwatch();
			sw.Start();
			//			Logger.Minor("Fetching remote mods...");
			string apiResponse = "";
			using (WebClient? client = new WebClient())
			{
				client.Headers["User-Agent"] = "AutoUpdatingPlugin";
				
				apiResponse = client.DownloadString("https://tldmods.com/api.php");
			}
			//			apiResponse = File.ReadAllText(@"C:\REPOS\ModLists\api.json");
#if DEBUG
			Logger.Msg($"Downloaded API data (L:{apiResponse.Length}) ({(float)sw.ElapsedMilliseconds / 1000:N2}s)");
#endif
			APIMod[] apiMods = APIReader.Deserialize(apiResponse);

			allMods.Clear();
			validMods.Clear();
			supportedMods.Clear();

			disabledByAuthor.Clear();
			disabledNoValidLink.Clear();


			foreach (APIMod mod in apiMods)
			{

				string modName = mod.CleanName;

				if (allMods.TryGetValue(modName, out APIMod existing))
				{
					Logger.Warning($"Duplicate Mod in API: {modName} => {mod.Author}:{mod.Version} <> {existing.Author}:{existing.Version}");
					if (mod.VersionData >= existing.VersionData)
					{
						Logger.Warning($"Using {modName} => {mod.Author}:{mod.Version}");
						allMods.Remove(modName);
						validMods.Remove(modName);
						supportedMods.Remove(modName);
					}
					if (mod.VersionData <= existing.VersionData)
					{
						Logger.Warning($"Using {modName} => {existing.Author}:{existing.Version}");
						continue;
					}
				}

				allMods.Add(modName, mod);
				//Logger.Debug($"allMods : {modName}");

				// Aliases
				foreach (string alias in mod.Aliases)
				{
					string _alias = alias.ToLowerInvariant();
					if (_alias != modName.ToLowerInvariant() && !aliasNames.ContainsKey(_alias))
					{
						aliasNames[_alias] = modName;
					}
				}

				// Replacements
				foreach (string replace in mod.Replaces)
				{
					string _replace = replace.ToLowerInvariant();
					if (_replace != modName.ToLowerInvariant() && !replacements.ContainsKey(_replace))
					{
						replacements[_replace] = modName;
					}
				}

				if (!mod.AutoUpdate)
				{
					disabledByAuthor.Add(modName);
					//Logger.Debug($"disabledByAuthor : {modName}");
					continue;
				}

				if (mod.Downloads.Length == 0)
				{
					disabledNoValidLink.Add(modName);
					//Logger.Debug($"disabledNoValidLink : {modName}");
					continue;
				}

				validMods.Add(modName, mod);
				//Logger.Debug($"validMods : {modName}");

				// Add to supported mods
				supportedMods.Add(modName, mod);
			}

#if DEBUG
			File.WriteAllText(Path.Combine(FileUtils.PluginsFolder, "allMods.json"), JsonSerializer.Serialize(allMods.Keys.ToArray(), new JsonSerializerOptions() { WriteIndented = true }));
			File.WriteAllText(Path.Combine(FileUtils.PluginsFolder, "validMods.json"), JsonSerializer.Serialize(validMods.Keys.ToArray(), new JsonSerializerOptions() { WriteIndented = true }));
			File.WriteAllText(Path.Combine(FileUtils.PluginsFolder, "replacements.json"), JsonSerializer.Serialize(replacements, new JsonSerializerOptions() { WriteIndented = true }));
			File.WriteAllText(Path.Combine(FileUtils.PluginsFolder, "disabledByAuthor.json"), JsonSerializer.Serialize(disabledByAuthor, new JsonSerializerOptions() { WriteIndented = true }));
			File.WriteAllText(Path.Combine(FileUtils.PluginsFolder, "disabledNoValidLink.json"), JsonSerializer.Serialize(disabledNoValidLink, new JsonSerializerOptions() { WriteIndented = true }));
#endif

			sw.Stop();
			Logger.Msg("API Mods " + apiMods.Length + ", Valid " + validMods.Count + ", Supported " + supportedMods.Count + $" ({(float)sw.ElapsedMilliseconds / 1000:N2}s)");
		}
		internal static bool IsReplaced(string currentName)
		{
			bool result = replacements.ContainsKey(currentName.ToLowerInvariant());
			//			Logger.Msg($"IsReplaced {currentName}|{currentName.ToLowerInvariant()} >> {result}");
			return result;
		}
		internal static string GetReplacementName(string currentName)
		{
			return replacements.TryGetValue(currentName.ToLowerInvariant(), out string? newName) ? newName : currentName;
		}

		internal static bool IsAlias(string currentName) => aliasNames.ContainsKey(currentName.ToLowerInvariant());
		internal static string GetNameFromAlias(string currentName)
		{
			return aliasNames.TryGetValue(currentName.ToLowerInvariant(), out string? newName) ? newName : currentName;
		}

		internal static bool IsDisabled(string currentName) => disabledByAuthor.Contains(currentName.ToLowerInvariant()) || disabledNoValidLink.Contains(currentName.ToLowerInvariant());
		internal static string GetDisabledReason(string currentName)
		{
			if (disabledByAuthor.Contains(currentName.ToLowerInvariant()))
			{
				return "by Author";
			}
			if (disabledNoValidLink.Contains(currentName.ToLowerInvariant()))
			{
				return "by NoValidLink";
			}
			return null;
		}

		internal static string[] GetModNames() => validMods.Keys.ToArray();
		internal static string[] GetSortedModNames()
		{
			List<string>? result = new List<string>(validMods.Keys.ToArray());
			result.Sort();
			return result.ToArray();
		}

		internal static Dictionary<string, APIMod> SortedDictionary()
		{
			IOrderedEnumerable<KeyValuePair<string, APIMod>>? sortedDict = from entry in validMods orderby entry.Key ascending select entry;
			return sortedDict.ToDictionary(pair => pair.Key, pair => pair.Value);
		}
	}
}