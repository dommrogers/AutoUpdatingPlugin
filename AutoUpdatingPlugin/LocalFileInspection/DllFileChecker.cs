using Mono.Cecil;
using System;
using System.IO;
using System.Linq;

namespace AutoUpdatingPlugin
{
	internal static class DllFileChecker
	{
		internal static void ScanForDllFiles(string basedirectory)
		{
			if (!Directory.Exists(basedirectory))
			{
				Logger.Msg("No Mods folder. Creating...");
				Directory.CreateDirectory(basedirectory);
				return;
			}

			string[] dlltbl = Directory.GetFiles(basedirectory, "*.dll");
			if (dlltbl.Length > 0)
			{
				for (int i = 0; i < dlltbl.Length; i++)
				{
					string filename = dlltbl[i];
					if (string.IsNullOrEmpty(filename))
					{
						continue;
					}

					if (filename.EndsWith(".dev.dll"))
					{
						Logger.Msg($"Skipping development mod '{filename}'");
						continue;
					}

					try
					{
						string? modName;
						string? modVersion;
						using (AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly(filename))
						{

							CustomAttribute? melonInfoAttribute = assembly.CustomAttributes.FirstOrDefault(a =>
								a.AttributeType.Name == "MelonModInfoAttribute" || a.AttributeType.Name == "MelonInfoAttribute");

							if (melonInfoAttribute == null)
							{
								continue;
							}

							modName = melonInfoAttribute.ConstructorArguments[1].Value as string;
							modVersion = melonInfoAttribute.ConstructorArguments[2].Value as string;

							//Logger.Msg($"found '{modName}' > '{modVersion}' > {assembly.Name.Version.ToString()}");

							if ((VersionData)assembly.Name.Version.ToString() != (VersionData)modVersion)
							{
								Logger.Warning($"Version MisMatch: {modName} MelonInfo = {modVersion} Assembly = {assembly.Name.Version.ToString()}");
							}
							if ((VersionData)assembly.Name.Version.ToString() > (VersionData)modVersion)
							{
								modVersion = assembly.Name.Version.ToString();
								Logger.Warning($"Using Assembly Version: {modName} {assembly.Name.Version.ToString()}");
							}
						}

						bool isAliasName = APIList.IsAliasName(modName);
						string newName = APIList.GetNewModName(modName);

						if (isAliasName)
						{
							if (!InstalledModList.installedMods.ContainsKey(newName)) {
								Logger.Msg($"'{modName}' is obsolete. It will be replaced with '{newName}'.");
								modName = newName;
							}
						}

						modName = FileUtils.GetCleanName(modName);

						if (InstalledModList.installedMods.TryGetValue(modName, out InstalledModDetail? installedModDetail))
						{
							if (installedModDetail.files[0].version > (VersionData)modVersion)
							{
								File.Delete(filename); // Delete duplicated mods
								Logger.Msg("1Deleted duplicated mod " + modName);
							}
							else
							{
								if (!InstalledModList.installedMods.ContainsKey(newName))
								{
									File.Delete(installedModDetail.files[0].filepath); // Delete duplicated mods
									installedModDetail.files.RemoveAt(0);
									Logger.Msg("2Deleted duplicated mod " + modName);
									installedModDetail.files.Add(new InstalledFileDetail(modName, modVersion, filename, InstalledFileType.DLL));
									if (isAliasName)
									{
										installedModDetail.TriggerOutdated();
									}
								} else
								{
									Logger.Warning($"Naming MisMatch: {modName} > {newName}");
								}
							}
						}
						else
						{
							InstalledModDetail newModDetail = new InstalledModDetail(modName);
							newModDetail.files.Add(new InstalledFileDetail(modName, modVersion, filename, InstalledFileType.DLL));
							InstalledModList.installedMods.Add(modName, newModDetail);
							if (isAliasName)
							{
								newModDetail.TriggerOutdated();
							}
						}
					}
					catch (Exception)
					{
						Logger.Msg("Failed to read assembly " + filename);
					}
				}
			}
		}
	}
}
