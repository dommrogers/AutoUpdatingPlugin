using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AutoUpdatingPlugin
{
	internal class APIMod
	{
		public string name;
		public VersionData version;
		public string type;
		public bool enableUpdate;
		public string[] downloadlinks;
		public string[] aliases;
		public string[] dependencies;
		public bool canCheckDependencies = true;

		public override string ToString() => name;

		internal bool ContainsDllFile()
		{
			if (downloadlinks is null || downloadlinks.Length == 0)
			{
				return false;
			}

			foreach (string link in downloadlinks)
			{
				if (link.EndsWith(".dll"))
				{
					return true;
				}
			}
			return false;
		}

		internal bool ContainsModComponentFile()
		{
			if (downloadlinks is null || downloadlinks.Length == 0)
			{
				return false;
			}

			foreach (string link in downloadlinks)
			{
				if (link.EndsWith(".modcomponent"))
				{
					return true;
				}
			}
			return false;
		}

		internal bool ContainsModSceneFile()
		{
			if (downloadlinks is null || downloadlinks.Length == 0)
			{
				return false;
			}

			foreach (string link in downloadlinks)
			{
				if (link.EndsWith(".modscene"))
				{
					return true;
				}
			}
			return false;
		}

		internal bool ContainsZipFile()
		{
			if (downloadlinks is null || downloadlinks.Length == 0)
			{
				return false;
			}

			foreach (string link in downloadlinks)
			{
				if (link.EndsWith(".zip"))
				{
					return true;
				}
			}
			return false;
		}

		internal void ValidateDependencies(string[] validDependencies)
		{
			foreach (string? dependency in dependencies)
			{
				if (!validDependencies.Contains(dependency))
				{
					canCheckDependencies = false;
					Logger.Warning($"Dependency checking for {name} has been disabled because of invalid dependency references.");
					return;
				}
			}
		}

		internal bool CanUseToUpdate()
		{
			return enableUpdate && downloadlinks != null && downloadlinks.Length > 0 && !ContainsModSceneFile();
		}

		internal string[] GetFileNames()
		{
			if (downloadlinks is null || downloadlinks.Length == 0)
			{
				return new string[0];
			}

			List<string> result = new List<string>(downloadlinks.Length);

			foreach (string link in downloadlinks)
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