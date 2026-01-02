using System.IO;
using System.IO.Compression;

namespace AutoUpdatingPlugin
{
	internal static class ZipFileHandler
	{
		internal static void ExtractZipFilesInDirectory(string directory)
		{
			string[] files = Directory.GetFiles(directory, "*.zip");
			foreach (string eachFile in files)
			{
				
				ExtractZipFile(eachFile);
			}
		}
		private static void ExtractZipFile(string zipFilePath)
		{
			string targetDir = Path.GetDirectoryName(zipFilePath);
			Logger.Debug($"Extracting.. {zipFilePath} > {targetDir}");
			ZipFile.ExtractToDirectory(zipFilePath, targetDir);
			File.Delete(zipFilePath);
		}
	}
}
