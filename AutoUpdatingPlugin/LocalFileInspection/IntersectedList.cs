using System.Collections.Generic;

namespace AutoUpdatingPlugin
{
	internal static class IntersectedList
	{
		internal static readonly Dictionary<string, APIMod> stringApiMods = new Dictionary<string, APIMod>();
		internal static readonly Dictionary<string, InstalledModDetail> stringInstalledMods = new Dictionary<string, InstalledModDetail>();
		internal static readonly Dictionary<InstalledModDetail, APIMod> installedApiMods = new Dictionary<InstalledModDetail, APIMod>();

		internal static void GenerateLists()
		{

			stringApiMods.Clear();
			stringInstalledMods.Clear();
			installedApiMods.Clear();

//			Logger.Msg("Generating an intersection of installed mods and the supported api...");
			foreach (KeyValuePair<string, InstalledModDetail> installedMod in InstalledModList.installedMods)
			{
				bool foundApiEntry = false;
				foreach (KeyValuePair<string, APIMod> remoteMod in APIList.validMods)
				{
					if (installedMod.Key.ToLowerInvariant() == remoteMod.Key.ToLowerInvariant())
					{
						foundApiEntry = true;
						stringApiMods.TryAdd(remoteMod.Key.ToLowerInvariant(), remoteMod.Value);
						stringInstalledMods.TryAdd(installedMod.Key.ToLowerInvariant(), installedMod.Value);
						installedApiMods.TryAdd(installedMod.Value, remoteMod.Value);
//						Logger.Msg($"Match: {installedMod.Key.ToLowerInvariant()} => {remoteMod.Key.ToLowerInvariant()}");
						break;
					}
					foreach (string alias in remoteMod.Value.aliases)
					{
						if (installedMod.Key.ToLowerInvariant() == alias.ToLowerInvariant())
						{
							foundApiEntry = true;
							stringApiMods.TryAdd(remoteMod.Key.ToLowerInvariant(), remoteMod.Value);
							stringInstalledMods.TryAdd(installedMod.Key.ToLowerInvariant(), installedMod.Value);
							installedApiMods.TryAdd(installedMod.Value, remoteMod.Value);
//							Logger.Msg($"Match Alias: {installedMod.Key.ToLowerInvariant()} => {alias.ToLowerInvariant()}");
							break;
						}
					}
				}
				if (!foundApiEntry)
				{
					Logger.Warning($"There is no associated API entry for '{installedMod.Key.ToLowerInvariant()}'");
				}
			}
			Logger.Minor($"Found {stringApiMods.Count} valid mods installed.");
		}
	}
}
