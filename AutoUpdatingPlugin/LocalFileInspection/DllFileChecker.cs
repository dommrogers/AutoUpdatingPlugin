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
						}

						bool isAliasName = APIList.IsAliasName(modName);

						if (isAliasName)
						{
							string newName = APIList.GetNewModName(modName);
							Logger.Msg($"'{modName}' is obsolete. It will be replaced with '{newName}'.");
							modName = newName;
						}

						modName = FileUtils.GetCleanName(modName);

						if (InstalledModList.installedMods.TryGetValue(modName, out InstalledModDetail? installedModDetail))
						{
							if (installedModDetail.files[0].version > (VersionData)modVersion)
							{
								File.Delete(filename); // Delete duplicated mods
								Logger.Msg("Deleted duplicated mod " + modName);
							}
							else
							{
								File.Delete(installedModDetail.files[0].filepath); // Delete duplicated mods
								installedModDetail.files.RemoveAt(0);
								Logger.Msg("Deleted duplicated mod " + modName);
								installedModDetail.files.Add(new InstalledFileDetail(modName, modVersion, filename, InstalledFileType.DLL));
								if (isAliasName)
								{
									installedModDetail.TriggerOutdated();
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
