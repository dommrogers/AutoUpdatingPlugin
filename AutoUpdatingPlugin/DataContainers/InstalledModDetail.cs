using System.Collections.Generic;

namespace AutoUpdatingPlugin
{
	internal class InstalledModDetail
	{
		public string Name { get; private set; }
		public string CleanName => FileUtils.GetCleanName(Name);
		public List<InstalledFileDetail> Files = new List<InstalledFileDetail>();
		public bool Outdated { get; private set; }
		public bool Remove { get; private set; }

		public string Version => GetMinValidVersion().ToString();
		public string Type { get; private set; }

		public InstalledModDetail(string name)
		{
			this.Name = name;
			this.Outdated = false;
			this.Remove = false;
		}

		public override string ToString() => CleanName;

		private VersionData[] GetVersionList()
		{
			VersionData[]? result = new VersionData[Files.Count];
			for (int i = 0; i < result.Length; i++)
			{
				result[i] = Files[i].version;
			}
			return result;
		}

		/// <summary>
		/// If there is a valid version in the version list, it returns the least one.<br/>
		/// Otherwise, it returns the Zero version.
		/// </summary>
		public VersionData GetMinValidVersion()
		{
			VersionData[]? versions = GetVersionList();
			if (versions is null || versions.Length == 0)
			{
				return VersionData.ZERO;
			}

			int currentMin = 0;
			for (int i = 1; i < versions.Length; i++)
			{
				if (versions[i] is null || !versions[i].IsValidSemver)
				{
					continue;
				}
				if (versions[currentMin] > versions[i])
				{
					currentMin = i;
				}
			}

			if (versions[currentMin] is null || !versions[currentMin].IsValidSemver)
			{
				return VersionData.ZERO;
			}
			else
			{
				return versions[currentMin];
			}
		}

		public bool CanBeUpdated() => GetMinValidVersion().IsValidSemver;

		public void TriggerOutdated() => Outdated = true;
		public void TriggerRemoval() => Remove = true;
	}
}
