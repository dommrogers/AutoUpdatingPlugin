using System;
using System.IO;
using System.Net;
using System.Threading;

namespace AutoUpdatingPlugin
{
	internal static class InternetAccess
	{
		internal const string DownloadURL = @"https://github.com/dommrogers/AutoUpdatingPlugin/releases/latest/download/AutoUpdatingPlugin.dll";
		private const string VersionURL = @"https://raw.githubusercontent.com/dommrogers/AutoUpdatingPlugin/master/Version.json";
		private const string DepsUrl = @"https://raw.githubusercontent.com/TLD-Mods/ModLists/master/dependency_files/aup.json";

		internal static string GetVersionJsonText()
		{
			string apiResponse = "";
			using (WebClient? client = new WebClient())
			{
				client.Headers["User-Agent"] = "AutoUpdatingPlugin";
				apiResponse = client.DownloadString(VersionURL);
			}
			return apiResponse;
		}

		internal static string GetGlobalDependencyData()
		{

			string apiResponse = "";
			using (WebClient? client = new WebClient())
			{
				client.Headers["User-Agent"] = "AutoUpdatingPlugin";
				apiResponse = client.DownloadString(DepsUrl);
			}
			return apiResponse;
		}

		internal static bool TryDownloadFile(string downloadLink, out byte[]? data)
		{
			bool errored = false;
			using WebClient? client = new WebClient();
			bool downloading;
			byte[]? buffer;
			client.DownloadDataCompleted += (sender, e) =>
			{
				if (e.Error != null)
				{
					Logger.Error("Failed to download a newer version of the Auto Updating Plugin:\n" + e.Error);
					errored = true;
				}
				else
				{
					buffer = e.Result;
				}

				downloading = false;
			};
			downloading = true;
			buffer = null;
			client.DownloadDataAsync(new Uri(downloadLink));

			while (downloading)
			{
				Thread.Sleep(50);
			}

			data = buffer;
			return !errored;
		}
	}
}
