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
				foreach (KeyValuePair<string, APIMod> remoteMod in APIList.supportedMods)
				{
					if (installedMod.Key.ToLower() == remoteMod.Key.ToLower())
					{
						foundApiEntry = true;
						stringApiMods.TryAdd(remoteMod.Key.ToLower(), remoteMod.Value);
						stringInstalledMods.TryAdd(installedMod.Key.ToLower(), installedMod.Value);
						installedApiMods.TryAdd(installedMod.Value, remoteMod.Value);
						break;
					}
				}
				if (!foundApiEntry)
				{
					Logger.Warning($"There is no associated API entry for '{installedMod.Key.ToLower()}'");
				}
			}
			Logger.Minor($"Found {stringApiMods.Count} supported mods installed.");
		}
	}
}
