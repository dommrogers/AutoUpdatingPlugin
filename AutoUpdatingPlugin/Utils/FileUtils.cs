using Mono.Cecil;
using System;
using System.ComponentModel.Design.Serialization;
using System.IO;
using System.Linq;

namespace AutoUpdatingPlugin
{
	internal static class FileUtils
	{
		static FileUtils()
		{
            GameDirectory = MelonLoader.MelonUtils.GameDirectory;
			PluginsFolder = Path.Combine(GameDirectory, "Plugins");
			ModsFolder = Path.Combine(GameDirectory, "Mods");
		}
		internal static string GameDirectory { get; }
		internal static string PluginsFolder { get; }
		internal static string ModsFolder { get; }

		public static string GetDestination(string link)
		{
			return string.IsNullOrWhiteSpace(link)
				? throw new ArgumentException("Invalid link argument")
				: Path.Combine(ModsFolder, Path.GetFileName(link));
		}

		public static string GetDestinationPlugin(string link)
		{
			return string.IsNullOrWhiteSpace(link)
				? throw new ArgumentException("Invalid link argument")
				: Path.Combine(PluginsFolder, Path.GetFileName(link));
		}

		internal static string GetPathSelf()
		{

			return Path.Combine(FileUtils.PluginsFolder, BuildInfo.Name + ".dll");

			foreach (string filename in Directory.GetFiles(PluginsFolder, "*.dll"))
			{
				try
				{
					using AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly(filename);

					CustomAttribute? melonInfoAttribute = assembly.CustomAttributes.FirstOrDefault(a =>
						a.AttributeType.Name == "MelonModInfoAttribute" || a.AttributeType.Name == "MelonInfoAttribute");

					if (melonInfoAttribute == null)
					{
						Logger.Msg("melonInfoAttribute = null");
						continue;
					}

					foreach (CustomAttributeArgument caa in melonInfoAttribute.ConstructorArguments)
					{
						Logger.Msg($"{filename} -> {caa.Value}");
					}

					string? name = melonInfoAttribute.ConstructorArguments[1].Value as string;

					if (name == BuildInfo.Name)
					{
						return filename;
					}
				}
				catch (Exception)
				{
					Logger.Msg("Failed to read assembly " + filename);
				}
			}

			return "";
		}
	}
}
