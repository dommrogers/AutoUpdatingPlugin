using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;

namespace AutoUpdatingPlugin
{
	internal static class ModUpdater
	{
		public static int progressTotal = 0, progressDownload = 0;
		private static int toUpdateCount = 0;
		private static readonly List<FailedUpdateInfo> failedUpdates = new List<FailedUpdateInfo>();

		private static void LogProgress()
		{
			if (toUpdateCount == 0)
			{
				Logger.Success("All installed mods are already up to date !");
			}
			else if (failedUpdates.Count > 0)
			{
				Logger.Warning($"{failedUpdates.Count} mods failed to update ({toUpdateCount - failedUpdates.Count}/{toUpdateCount} succeeded)");
			}
			else
			{
				Logger.Success("Successfully updated " + toUpdateCount + " mods !");
			}
		}

		internal static void DownloadAndUpdateMods()
		{
//			Logger.Minor("Checking for outdated mods...");
			List<Tuple<InstalledModDetail, APIMod>> toUpdate = new List<Tuple<InstalledModDetail, APIMod>>();

			// List all installed mods that can be updated
			foreach (KeyValuePair<InstalledModDetail, APIMod> pair in IntersectedList.installedApiMods)
			{
				InstalledModDetail? installedMod = pair.Key;
				APIMod? remoteMod = pair.Value;
				if (installedMod.Outdated)
				{
					toUpdate.Add(new Tuple<InstalledModDetail, APIMod>(installedMod, remoteMod));
				}
				else if (remoteMod.CanUseToUpdate() && installedMod.CanBeUpdated())
				{
					VersionData installedModVersion = installedMod.GetMinValidVersion();
					VersionData remoteModVersion = remoteMod.version;
#if DEBUG
					int compareResult = remoteModVersion.CompareTo(installedModVersion);
					Logger.Msg($"Version comparison between [remote] {remoteMod.version.ToString(3)} and [local] {installedModVersion.ToString(3)} for ({remoteMod.name}): " + compareResult);
#endif
					if (installedModVersion < remoteModVersion)
					{
						toUpdate.Add(new Tuple<InstalledModDetail, APIMod>(installedMod, remoteMod));
					}
				}
			}

			toUpdateCount = toUpdate.Count;

			if (toUpdateCount > 0)
			{
				Logger.Warning($"Found {toUpdateCount} outdated mods.");
			}

			for (int i = 0; i < toUpdateCount; ++i)
			{
				InstalledModDetail installedMod = toUpdate[i].Item1;
				APIMod apiMod = toUpdate[i].Item2;

				Logger.Msg($"Updating '{installedMod.name}' ({i + 1} / {toUpdateCount})...");
				progressTotal = (int)(i / (double)toUpdateCount * 100);

				UpdateInstallation(installedMod, apiMod);

				progressTotal = (int)((i + 1) / (double)toUpdateCount * 100);
				Logger.Minor($"Progress: {i + 1}/{toUpdateCount} -> {progressTotal}%");

			}

			LogProgress();
		}

		public static void UpdateInstallation(InstalledModDetail installedMod, APIMod apiMod)
		{
			try
			{
				bool errored = false;
				List<(string, byte[]?)> downloadedData = new();
				using WebClient client = new();
				bool downloading;
				byte[]? buffer;
				client.DownloadDataCompleted += (sender, e) =>
				{
					if (e.Error != null)
					{
						Logger.Error("Failed to download " + installedMod.name + ":\n" + e.Error);
						errored = true;
						failedUpdates.Add(new FailedUpdateInfo(installedMod, FailedUpdateReason.DownloadError, e.ToString() ?? ""));
					}
					else
					{
						buffer = e.Result;
					}

					downloading = false;
				};
				foreach (string link in apiMod.downloadlinks)
				{
					downloading = true;
					buffer = null;
					client.DownloadDataAsync(new Uri(link));

					while (downloading)
					{
						Thread.Sleep(50);
					}

					downloadedData.Add((FileUtils.GetDestination(link), buffer));
				}


				if (!errored)
				{
					try
					{
						foreach (InstalledFileDetail oldFile in installedMod.files)
						{
							File.Delete(oldFile.filepath);
						}
						foreach ((string, byte[]?) modFile in downloadedData)
						{
							File.WriteAllBytes(modFile.Item1, modFile.Item2);
						}
					}
					catch (Exception e)
					{
						Logger.Error("Failed to save replacement files for " + installedMod.name + ":\n" + e);
						failedUpdates.Add(new FailedUpdateInfo(installedMod, FailedUpdateReason.SaveError, e.ToString()));
						return;
					}
				}

			}
			catch (Exception e)
			{
				Logger.Error("Failed to update " + installedMod.name + ":\n" + e);
				failedUpdates.Add(new FailedUpdateInfo(installedMod, FailedUpdateReason.Unknown, e.ToString()));
			}

		}
	}
}
