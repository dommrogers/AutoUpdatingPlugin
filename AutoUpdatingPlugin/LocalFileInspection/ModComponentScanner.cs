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
						BuildInfoDetail buildInfo = InternalZipInspector.InspectZipFile(filename);


						string modFileName = Path.GetFileNameWithoutExtension(filename);
						string modName = (string.IsNullOrWhiteSpace(buildInfo.Name)) ? modFileName : buildInfo.Name ;
						string version = buildInfo.Version;

						bool isAliasName = APIList.IsAliasName(modName);
						if (isAliasName)
						{
							string newName = APIList.GetNewModName(modName);
							Logger.Msg($"MC '{modName}' is obsolete. It will be replaced with '{newName}'.");
							modName = newName;
						}

						modName = FileUtils.GetCleanName(modName);
						modFileName = FileUtils.GetCleanName(modFileName);


						if (InstalledModList.installedMods.TryGetValue(modName, out InstalledModDetail installedModDetail))
						{
							Logger.Msg($"MC Tracking Installed Mod File {modName}|{modFileName}");
							installedModDetail.files.Add(new InstalledFileDetail(modName, version, filename, InstalledFileType.ModComponent));
							if (isAliasName)
							{
								installedModDetail.TriggerOutdated();
							}
						}
						else
						{
							Logger.Msg($"Tracking MC File {modFileName}=>{modName}");
							InstalledModDetail newModDetail = new InstalledModDetail(modName);
							newModDetail.files.Add(new InstalledFileDetail(modName, version, filename, InstalledFileType.ModComponent));
							if (isAliasName)
							{
								newModDetail.TriggerOutdated();
							}

							InstalledModList.installedMods.Add(modName, newModDetail);
#if DEBUG
							Logger.Msg($"Adding InstalledMods {modName}");
#endif

							// also track the modcomponent filename
							if (modName != modFileName) {
#if DEBUG
Logger.Msg($"Adding InstalledMods {modFileName}");
#endif
								InstalledModDetail newModDetailMC = new InstalledModDetail(modFileName);
								newModDetailMC.files.Add(new InstalledFileDetail(modFileName, version, filename, InstalledFileType.ModComponent));
								InstalledModList.installedMods.TryAdd(modFileName, newModDetailMC);
								
							}


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
