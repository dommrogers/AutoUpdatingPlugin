using MelonLoader.TinyJSON;
using System;
using System.IO;

namespace AutoUpdatingPlugin
{
	internal static class SelfUpdater
	{
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		internal static void CheckVersion()
		{
//			Logger.Msg("Fetching updater version data...");
//			Logger.Msg("Attempting to get version information from the repository...");
			string apiResponse = InternetAccess.GetVersionJsonText();

			if (string.IsNullOrWhiteSpace(apiResponse))
			{
				Logger.Error("Failed to download AUP version data.");
				return;
			}

//			Logger.Msg("Downloaded AUP version data");

			ProxyObject? data = (ProxyObject)JSON.Load(apiResponse);

			string version = data["Version"];
			if ((VersionData)BuildInfo.Version >= (VersionData)version)
			{
				Logger.Success($"The Auto Updating Plugin ({BuildInfo.Version}) is up-to-date.");
				return;
			}

			Logger.Warning($"The Auto Updating Plugin ({BuildInfo.Version}) is out-dated. Updating now to ({version})...");
			string downloadLink = data["Download"];
			if (string.IsNullOrWhiteSpace(downloadLink))
			{
				downloadLink = InternetAccess.DownloadURL;
			}

			string path = FileUtils.GetPathSelf();
			Logger.Msg(path);
			try
			{
				if (InternetAccess.TryDownloadFile(downloadLink, out byte[] bytes))
				{
					if (!TrySaveDataToFile(path, bytes))
					{
						return;//Failed to save the updated version, so don't end the program.
					}
					Logger.Success("The Auto Updating Plugin has been successfully updated. The game must be relaunched for these changes to take effect.");
					EndProgram();
				}
			}
			catch (Exception e)
			{
				Logger.Error("Failed to update the Auto Updating Plugin:\n" + e);
			}
		}

		private static bool TrySaveDataToFile(string path, byte[] data)
		{
			try
			{
				File.WriteAllBytes(path, data);
				return true;
			}
			catch (Exception e)
			{
				Logger.Error("Failed to save replacement files for a newer version of the Auto Updating Plugin:\n" + e);
				return false;
			}
		}

		private static void EndProgram()
		{
			Logger.Warning("Ending Program in 10 seconds");
			System.Diagnostics.Stopwatch? x = new System.Diagnostics.Stopwatch();
			x.Start();
			while (x.ElapsedMilliseconds < 10000) { }
			System.Environment.Exit(1);
		}
	}
}
