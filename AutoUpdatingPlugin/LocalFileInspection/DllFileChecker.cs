using Mono.Cecil;
using System;
using System.IO;
using System.Linq;

namespace AutoUpdatingPlugin
{
	internal static class DllFileChecker
	{

		internal static void Cleanup()
		{
			string[] remModFiles = Directory.GetFiles(FileUtils.ModsFolder, "*.dll.rem");
			foreach (string remModFile in remModFiles)
			{
				File.Delete(remModFile);
			}

			string[] remPluginFiles = Directory.GetFiles(FileUtils.PluginsFolder, "*.dll.rem");
			foreach (string remPluginFile in remPluginFiles)
			{
				File.Delete(remPluginFile);
			}

		}


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
					string _filename = Path.GetFileName(filename);



					if (string.IsNullOrEmpty(filename))
					{
						continue;
					}

					//if(_filename.ToLowerInvariant() == "autoupdatingplugin.dll")
					//{
					//	continue;
					//}

					if (_filename.EndsWith(".dev.dll"))
					{
						Logger.Msg($"Skipping development mod '{_filename}'");
						continue;
					}
//					Logger.Msg($"Found '{_filename}'");


					try
					{
						string? modName;
						string? modNameOriginal;
						string? modVersion;
						using (AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly(filename))
						{

							CustomAttribute? melonInfoAttribute = assembly.CustomAttributes.FirstOrDefault(a =>
								a.AttributeType.Name == "MelonModInfoAttribute" || a.AttributeType.Name == "MelonInfoAttribute");

							if (melonInfoAttribute == null)
							{
								continue;
							}


							modName = FileUtils.GetCleanName(melonInfoAttribute.ConstructorArguments[1].Value as string);
							modNameOriginal = modName;
							modVersion = melonInfoAttribute.ConstructorArguments[2].Value as string;

							//Logger.Msg($"found '{modName}' > '{modVersion}' > {assembly.Name.Version.ToString()}");

							if (new VersionData(assembly.Name.Version.ToString()) != new VersionData(modVersion))
							{
								Logger.Warning($"Version MisMatch: {modName} MelonInfo = {modVersion} Assembly = {assembly.Name.Version.ToString()}");
							}
							if (new VersionData(assembly.Name.Version.ToString()) > new VersionData(modVersion))
							{
								modVersion = assembly.Name.Version.ToString();
								Logger.Warning($"Using Assembly Version: {modName} {assembly.Name.Version.ToString()}");
							}
						}

						bool isReplaced = APIList.IsReplaced(modName);
						string replacementName = APIList.GetReplacementName(modName);

						Logger.Debug($"Replaced? '{modName}' >> {isReplaced} >> {replacementName}.");

						if (isReplaced)
						{
							Logger.Warning($"'{modName}' is obsolete.");
							if (!InstalledModList.installedMods.ContainsKey(replacementName)) {
								Logger.Warning($"- It will be replaced with '{replacementName}'.");
								modName = replacementName;
							}
						}

						modName = FileUtils.GetCleanName(modName);

						if (InstalledModList.installedMods.TryGetValue(modName, out InstalledModDetail? installedModDetail))
						{
							if (installedModDetail.Files[0].version > new VersionData(modVersion))
							{
								File.Delete(filename); // Delete duplicated mods
								Logger.Msg("Deleted duplicated mod " + modName);
							}
							else
							{
								if (!InstalledModList.installedMods.ContainsKey(replacementName))
								{
									File.Delete(installedModDetail.Files[0].filepath); // Delete duplicated mods
									installedModDetail.Files.RemoveAt(0);
									Logger.Msg($"Deleted obsolete mod `{modNameOriginal}`, replaced by `{modName}`");
									installedModDetail.Files.Add(new InstalledFileDetail(modName, modVersion, filename, InstalledFileType.DLL));
									if (isReplaced)
									{
										installedModDetail.TriggerOutdated();
									}
								} else
								{
									Logger.Warning($"Naming MisMatch: {modName} > {replacementName}");
								}
							}
						}
						else
						{
							InstalledModDetail newModDetail = new InstalledModDetail(modName);
							newModDetail.Files.Add(new InstalledFileDetail(modName, modVersion, filename, InstalledFileType.DLL));
							InstalledModList.installedMods.Add(modName, newModDetail);
							if (isReplaced)
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
