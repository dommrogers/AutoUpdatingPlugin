using MelonLoader.ICSharpCode.SharpZipLib.Zip;
using System.IO;
using System.Text;

namespace AutoUpdatingPlugin
{
	internal static class InternalZipInspector
	{
		internal static BuildInfoDetail InspectZipFile(string zipFilePath)
		{
			//Logger.Msg("Reading zip file at: '{0}'", zipFilePath);
			using (FileStream fileStream = File.OpenRead(zipFilePath))
			{
				using (ZipInputStream zipInputStream = new ZipInputStream(fileStream))
				{
					ZipEntry entry;
					while ((entry = zipInputStream.GetNextEntry()) != null)
					{
						string internalPath = entry.Name;
						if (internalPath.ToLowerInvariant() == "buildinfo.json")
						{
							using MemoryStream unzippedFileStream = new MemoryStream();
							int size = 0;
							byte[] buffer = new byte[4096];
							while (true)
							{
								size = zipInputStream.Read(buffer, 0, buffer.Length);
								if (size > 0)
								{
									unzippedFileStream.Write(buffer, 0, size);
								}
								else
								{
									break;
								}
							}
							//Logger.Msg(unzippedFileStream.ToArray().Length.ToString());
							string text = ReadToString2(unzippedFileStream);
							if (text is null)
							{
								Logger.Error("text in InternalZipInspector was null");
							}
							else
							{
								//Logger.Msg("Found BuildInfo.json\n" + text);
								return JsonAnalyzer.GetBuildInfoFromJson(text, Path.Combine(zipFilePath, internalPath));
							}
						}
					}

					Logger.Msg($"Cannot identify version because there is no BuildInfo.json in {zipFilePath}");
					return new BuildInfoDetail();
				}
			}
		}
		internal static Encoding GetEncoding(MemoryStream memoryStream)
		{
			using StreamReader reader = new StreamReader(memoryStream, true);
			reader.Peek();
			return reader.CurrentEncoding;
		}
		internal static string ReadToString(MemoryStream memoryStream)
		{
			Encoding encoding = GetEncoding(memoryStream);
			Logger.Msg(encoding.EncodingName);
			return encoding.GetString(memoryStream.ToArray());
		}
		internal static string ReadToString2(MemoryStream memoryStream)
		{
			using StreamReader reader = new StreamReader(memoryStream, true);
			//reader.Peek();
			memoryStream.Position = 0;
			//Logger.Msg(memoryStream.Position.ToString());
			return reader.ReadToEnd();
		}
	}
}
