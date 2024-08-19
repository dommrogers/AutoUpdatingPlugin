using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;

namespace AutoUpdatingPlugin
{
	internal static class DependencyHandler
	{



		private static GlobalDependencyData GlobalDependencyData()
		{
			string? data = InternetAccess.GetGlobalDependencyData();
			return new GlobalDependencyData(data);
		}

		private static void ValidateDependencies()
		{
			string[] modNames = APIList.GetModNames();
			foreach (KeyValuePair<string, APIMod> remoteMod in IntersectedList.stringApiMods)
			{
				remoteMod.Value.ValidateDependencies(modNames);
			}
		}
		private static APIMod[] GetMissingDependencies()
		{
			string[] installedMods = InstalledModList.GetInstalledModNames();
			List<string> missingNames = new List<string>();
			List<string> missingNamesForce = new List<string>();
			foreach (KeyValuePair<InstalledModDetail, APIMod> remoteMod in IntersectedList.installedApiMods)
			{
				if (!remoteMod.Value.canCheckDependencies)
				{
					continue;
				}

				foreach (string dependency in remoteMod.Value.dependencies)
				{
					string _dependency = FileUtils.GetCleanName(dependency);
					if (!installedMods.Contains(_dependency) && !missingNames.Contains(_dependency))
					{
						missingNames.Add(_dependency);
					}
				}
			}

			// new global dependency checker
			//			Logger.Msg($"Checking GlobalDependencyData");
			GlobalDependencyData? gdd = GlobalDependencyData();
			if (gdd.Entries.Count > 0)
			{
				//				Logger.Msg($"GlobalDependencyData {gdd.Entries.Count}");
				foreach (GDEntry entry in gdd.Entries)
				{
					if (installedMods.Contains(FileUtils.GetCleanName(entry.Mod)))
					{
						//						Logger.Msg($"GlobalDependencyData {entry.Mod} {entry.Requires.Length}");
						foreach (string dependency in entry.Requires)
						{
							string _dependency = FileUtils.GetCleanName(dependency);
							if (!installedMods.Contains(_dependency) && !missingNamesForce.Contains(_dependency))
							{
								missingNames.Add(_dependency);
								//								Logger.Msg($"GlobalDependencyData {entry.Mod} Missing {dependency}");
							}
						}
					}
				}
			}


			List<APIMod> result = new List<APIMod>(missingNames.Count);
			foreach (string name in missingNames)
			{
				string _name = FileUtils.GetCleanName(name);

				if (APIList.validMods.ContainsKey(_name))
				{
					result.Add(APIList.validMods[_name]);
				}
			}

			return result.ToArray();
		}

		public static int InstallAllMissingDependencies()
		{
			//			Logger.Msg("Checking for missing dependencies...");

			ValidateDependencies();

			APIMod[]? toInstall = GetMissingDependencies();

			if (toInstall.Length > 0)
			{
				Logger.Warning($"Found {toInstall.Length} missing dependencies.");
			}

			int toUpdateCount = toInstall.Length;
			for (int i = 0; i < toUpdateCount; ++i)
			{
				APIMod apiMod = toInstall[i];

				Logger.Msg($"Installing {apiMod.name} ({i + 1} / {toUpdateCount})...");

				InstallMissingDependency(apiMod);

				int progressTotal = (int)((i + 1) / (double)toUpdateCount * 100);
				Logger.Minor($"Progress: {i + 1}/{toUpdateCount} -> {progressTotal}%");
			}

			return toUpdateCount;
		}

		private static void InstallMissingDependency(APIMod apiMod)
		{
			try
			{
				bool errored = false;
				List<(string, byte[])> downloadedData = new List<(string, byte[])>();
				using (WebClient? client = new WebClient())
				{
					bool downloading;
					byte[] buffer;
					client.DownloadDataCompleted += (sender, e) =>
					{
						if (e.Error != null)
						{
							Logger.Error("Failed to download " + apiMod.name + ":\n" + e.Error);
							errored = true;
						}
						else
						{
							buffer = e.Result;
						}

						downloading = false;
					};
					foreach (string? link in apiMod.downloadlinks)
					{
						downloading = true;
						buffer = null;
						client.DownloadDataAsync(new Uri(link));

						while (downloading)
						{
							Thread.Sleep(50);
						}
						if (apiMod.type.ToLower() == "plugin")
						{
							downloadedData.Add((FileUtils.GetDestinationPlugin(link), buffer));
						}
						else
						{
							downloadedData.Add((FileUtils.GetDestination(link), buffer));
						}
					}


					if (!errored)
					{
						try
						{
							foreach ((string, byte[]) modFile in downloadedData)
							{
								File.WriteAllBytes(modFile.Item1, modFile.Item2);
							}
						}
						catch (Exception e)
						{
							Logger.Error("Failed to save while installing files for " + apiMod.name + ":\n" + e);
							return;
						}
					}
				}

			}
			catch (Exception e)
			{
				Logger.Error("Failed to install " + apiMod.name + ":\n" + e);
			}

		}
	}
}
