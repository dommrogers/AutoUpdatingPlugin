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
				string InstalledName = FileUtils.GetCleanName(installedMod.Key);
//				Logger.Debug($"Checking: {InstalledName}");

				bool foundApiEntry = false;
				foreach (KeyValuePair<string, APIMod> remoteMod in APIList.allMods)
				{
					string RemoteName = FileUtils.GetCleanName(remoteMod.Key);
//					Logger.Debug($"Checking: {InstalledName} | {RemoteName}");
					if (InstalledName == RemoteName)
					{
						foundApiEntry = true;
						stringApiMods.TryAdd(RemoteName, remoteMod.Value);
						stringInstalledMods.TryAdd(InstalledName, installedMod.Value);
						installedApiMods.TryAdd(installedMod.Value, remoteMod.Value);
						Logger.Debug($"Installed: {InstalledName} {installedMod.Value.Version} => API: {RemoteName} {remoteMod.Value.Version}");
						break;
					}
					foreach (string alias in remoteMod.Value.Aliases)
					{
						string AliasName = FileUtils.GetCleanName(alias);
						if (InstalledName == AliasName)
						{
							foundApiEntry = true;
							stringApiMods.TryAdd(RemoteName, remoteMod.Value);
							stringInstalledMods.TryAdd(InstalledName, installedMod.Value);
							installedApiMods.TryAdd(installedMod.Value, remoteMod.Value);
							Logger.Debug($"Installed: {InstalledName} {installedMod.Value.Version} => API(Alias): {RemoteName}|{AliasName} {remoteMod.Value.Version}");
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
