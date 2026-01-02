using System;
using System.IO;

namespace AutoUpdatingPlugin
{
	internal static class ModComponentScanner
	{
		internal static void ScanForModComponentFiles()
		{
			string basedirectory = FileUtils.ModsFolder;

			if (!Directory.Exists(basedirectory))
			{
				Logger.Msg("No Mods folder. Creating...");
				Directory.CreateDirectory(basedirectory);
				return;
			}

			string[] mctbl = Directory.GetFiles(basedirectory, "*.modcomponent");
			if (mctbl.Length > 0)
			{
				for (int i = 0; i < mctbl.Length; i++)
				{
					string filename = mctbl[i];
					if (string.IsNullOrEmpty(filename))
					{
						continue;
					}

					if (filename.EndsWith(".dev.modcomponent"))
					{
						Logger.Msg($"Skipping development mod '{filename}'");
						continue;
					}

					try
					{
						MCBuildInfo buildInfo = InternalZipInspector.InspectZipFile(filename);


						string modFileName = Path.GetFileNameWithoutExtension(filename);
						string modName = (string.IsNullOrWhiteSpace(buildInfo.Name)) ? modFileName : buildInfo.Name ;
						string originalModName = FileUtils.GetCleanName(modName);
						string version = buildInfo.Version;

						modName = FileUtils.GetCleanName(modName);
						modFileName = FileUtils.GetCleanName(modFileName);

//						Logger.Msg($"MC CHeck '{originalModName}' `{version}`.");

						bool isReplaced = APIList.IsReplaced(modName);
						if (isReplaced)
						{
							string replacementName = APIList.GetReplacementName(modName);
							Logger.Msg($"MC '{originalModName}' is obsolete. It will be replaced with '{replacementName}'.");
							modName = replacementName;
						}

						if (APIList.IsAlias(originalModName))
						{
							modName = APIList.GetNameFromAlias(originalModName);
//							Logger.Msg($"MC '{originalModName}' is an alias for '{modName}'.");
						}




						if (InstalledModList.installedMods.TryGetValue(modName, out InstalledModDetail installedModDetail))
						{
//							Logger.Msg($"MC Tracking Installed Mod File {modName}|{modFileName}");
							installedModDetail.Files.Add(new InstalledFileDetail(modName, version, filename, InstalledFileType.ModComponent));
							if (isReplaced)
							{
								installedModDetail.TriggerOutdated();
							}
						}
						else
						{
//							Logger.Msg($"Tracking MC File {modFileName}=>{modName}");
							InstalledModDetail newModDetail = new InstalledModDetail(modName);
							newModDetail.Files.Add(new InstalledFileDetail(modName, version, filename, InstalledFileType.ModComponent));
							if (isReplaced)
							{
								newModDetail.TriggerOutdated();
							}

							InstalledModList.installedMods.Add(modName, newModDetail);
							Logger.Debug($"Adding InstalledMods {modName}");

						}
					}
					catch (Exception e)
					{
						Logger.Msg($"Failed to read modcomponent file {filename}: {e}");
					}
				}
			}
		}
	}
}
