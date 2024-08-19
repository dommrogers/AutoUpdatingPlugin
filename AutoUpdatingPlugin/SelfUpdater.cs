using Il2CppSystem.Reflection;
using MelonLoader;
using MelonLoader.TinyJSON;
using System;
using System.IO;
using System.Linq;
using UnityEngine;

namespace AutoUpdatingPlugin
{
	internal static class SelfUpdater
	{


		internal static string updatePath;
		internal static byte[] updateBytes;
		internal static bool updatePending = false;

		private static Il2CppSystem.Action PerformPendingUpdate()
		{
			//Logger.Msg($"PerformPendingUpdate: {updatePath + "UPDATE"} => {updatePath}");
			//if (SelfUpdater.updatePending)
			//{
			//	if (File.Exists(updatePath + "UPDATE"))
			//	{
			//		try
			//		{
			//			File.Move(updatePath + "UPDATE", updatePath, true);
			//			Logger.Success("\n\n\n\nThe Auto Updating Plugin has been successfully updated. The game must be relaunched for these changes to take effect.\n\n\n\n");
			//		} catch (Exception e)
			//		{
			//			Logger.Error("\n\n\n\nUpdate Failed:\n\n"+e);
			//		}
			//	}
			//}
			return null;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		internal static void CheckForUpdate()
		{
			Logger.Msg("Fetching updater version data...");
			Logger.Msg("Attempting to get version information from the repository...");
			string apiResponse = InternetAccess.GetVersionJsonText();

			if (string.IsNullOrWhiteSpace(apiResponse))
			{
				Logger.Error("Failed to download AUP version data.");
				return;
			}

			Logger.Msg("Downloaded AUP version data");

			ProxyObject? data = (ProxyObject)JSON.Load(apiResponse);

			string version = data["Version"];
			if ((VersionData)BuildInfo.Version >= (VersionData)version)
			{
				Logger.Success($"The Auto Updating Plugin ({BuildInfo.Version}) is up-to-date.");
				//				return;
			}

			Logger.Warning($"The Auto Updating Plugin ({BuildInfo.Version}) is out-dated. Updating now to ({version})...");
			string downloadLink = data["Download"];
			if (string.IsNullOrWhiteSpace(downloadLink))
			{
				downloadLink = InternetAccess.DownloadURL;
			}

			Logger.Msg($"Plugins Path: {FileUtils.PluginsFolder}");
			string path = FileUtils.GetPathSelf();
			Logger.Msg($"Self Path = {path}");
			try
			{
				if (InternetAccess.TryDownloadFile(downloadLink, out byte[] bytes))
				{
					if (TrySaveDataToFile(path, bytes)) {
						updatePath = path;
						updatePending = true;
						MelonAssembly asm = MelonAssembly.LoadedAssemblies.Where(asm=>asm.Assembly == System.Reflection.Assembly.GetExecutingAssembly()).FirstOrDefault();
						if (asm != null)
						{
							asm.UnregisterMelons("Unloading for Update", false);
							asm = null;
							//						Application.quitting += PerformPendingUpdateDummy();
							//Application.quitting += PerformPendingUpdate();
							EndProgram();
						}
					}
				}
			}
			catch (Exception e)
			{
				Logger.Error("Failed to update the Auto Updating Plugin:\n" + e);
			}
		}

		internal static bool TrySaveDataToFile(string path, byte[] data)
		{

			try
			{
				FileStream fileStream = new(path + "UPDATE", FileMode.Create, FileAccess.Write);
				fileStream.Write(data, 0, data.Length);
				fileStream.Close();
				//				File.WriteAllBytes(path, data);
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
			Logger.Warning("Auto Updating Plugin update in progress...closing game in 5 seconds");

			System.Diagnostics.Stopwatch? x = new System.Diagnostics.Stopwatch();
			x.Start();
			while (x.ElapsedMilliseconds < 5000) { }
			System.Environment.Exit(0);
		}
	}
}
