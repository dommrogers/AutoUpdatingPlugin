using System.IO;
using System.IO.Compression;
using Newtonsoft.Json;

namespace AutoUpdatingPlugin
{
	public class MCBuildInfo
	{
		public string? Name { get; set; } = null;
		public string? Version { get; set; } = null;
		public string? Author { get; set; } = null;
	}

	internal static class InternalZipInspector
	{
		
		internal static MCBuildInfo InspectZipFile(string zipFilePath)
		{
			//Logger.Msg("Reading zip file at: '{0}'", zipFilePath);

			using (ZipArchive archive = ZipFile.Open(zipFilePath, ZipArchiveMode.Read))
			{
				if (archive.Entries.Count == 0)
				{
					Logger.Warning($"No entries fround in {zipFilePath}");
				}

				bool foundBuildInfo = false;

				foreach (ZipArchiveEntry entry in archive.Entries)
				{
					string internalPath = entry.Name;
					string internalPathCLean = internalPath.ToLowerInvariant();
					if (internalPathCLean == "buildinfo.json")
					{
						foundBuildInfo = true;
						Logger.Debug($"Found buildinfo.json {zipFilePath} {internalPathCLean}");

						using (var reader = new StreamReader(entry.Open()))
						{
							string buildInfo = reader.ReadToEnd();
							if (string.IsNullOrEmpty(buildInfo))
							{
								Logger.Warning($"Empty buildinfo.json in {zipFilePath}");
							}
							else
							{
								Logger.Debug($"Reading buildinfo.json {zipFilePath}");
								return JsonConvert.DeserializeObject<MCBuildInfo>(buildInfo);
							}

						}

					}

				}
				if (!foundBuildInfo)
				{
					Logger.Warning($"Failed to find buildinfo.json {zipFilePath}");
				}

			}

			return new MCBuildInfo();

		}

	}
}
