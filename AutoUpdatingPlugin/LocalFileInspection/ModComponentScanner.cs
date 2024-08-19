﻿using System;
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

						string modName;
						string modFileName = Path.GetFileNameWithoutExtension(filename);
						string version = buildInfo.Version;

						if (string.IsNullOrWhiteSpace(buildInfo.Name))
						{
							modName = Path.GetFileNameWithoutExtension(filename);
						}
						else
						{
							modName = buildInfo.Name;
						}

						bool isAliasName = APIList.IsAliasName(modName);
						if (isAliasName)
						{
							string newName = APIList.GetNewModName(modName);
							Logger.Msg($"'{modName}' is obsolete. It will be replaced with '{newName}'.");
							modName = newName;
						}

						modName = FileUtils.GetCleanName(modName);
						modFileName = FileUtils.GetCleanName(modFileName);


						if (InstalledModList.installedMods.TryGetValue(modName, out InstalledModDetail installedModDetail))
						{
							Logger.Msg($"Tracking Installed Mod File {modName}");
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
							Logger.Msg($"Adding InstalledMods {modName}");

							// also track the modcomponent filename
							if (modName != modFileName) {
								Logger.Msg($"Adding InstalledMods {modFileName}");
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
