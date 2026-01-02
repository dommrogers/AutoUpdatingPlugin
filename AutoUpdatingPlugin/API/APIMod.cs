using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine.Rendering.RenderGraphModule.NativeRenderPassCompiler;

namespace AutoUpdatingPlugin
{

	internal class APIMod
	{
		public string Name { get; set; }
		public string Type { get; set; }
		public string Author { get; set; }
		public string[] Aliases { get; set; }
		public string[] Replaces { get; set; }
		public string[] Dependencies { get; set; }
		public string Version { get; set; }
		public VersionData VersionData => new VersionData(Version);
		public string Download { get; set; }
		public string[] Downloads { get; set; }
		public bool AutoUpdate { get; set; }
		public bool canCheckDependencies { get; set; } = true;

		internal string CleanName => FileUtils.GetCleanName(Name);

		internal bool DownloadContainsExtension(string ext = ".dll")
		{
			if (Downloads is null || Downloads.Length == 0)
			{
				return false;
			}

			foreach (string link in Downloads)
			{
				if (link.EndsWith(ext))
				{
					return true;
				}
			}
			return false;
		}

		internal void ValidateDependencies(string[] validDependencies)
		{
			foreach (string? dependency in Dependencies)
			{
				string depName = FileUtils.GetCleanName(dependency);
			Logger.Debug($"Dependency checking for {CleanName} >> {depName} {validDependencies.Contains(depName)}");

				if (!validDependencies.Contains(depName))
				{
					canCheckDependencies = false;
					Logger.Warning($"Dependency checking for {CleanName} has been disabled because of invalid dependency references -> {depName}.");
					return;
				}
			}
		}

		internal bool CanUseToUpdate()
		{
			return AutoUpdate && Downloads != null && Downloads.Length > 0;
		}

		internal string[] GetFileNames()
		{
			if (Downloads is null || Downloads.Length == 0)
			{
				return new string[0];
			}

			List<string> result = new List<string>(Downloads.Length);

			foreach (string link in Downloads)
			{
				if (!string.IsNullOrWhiteSpace(link))
				{
					result.Add(Path.GetFileName(link));
				}
			}

			return result.ToArray();
		}

	}

}