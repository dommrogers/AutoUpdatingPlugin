using System.Linq;

namespace AutoUpdatingPlugin
{
	internal static class UpdateUtils
	{
		public static bool NeedsUpdated(InstalledModDetail installedMod, APIMod apiMod)
		{
			if (IsCorrectApi(installedMod, apiMod))
			{
				return installedMod.GetMinValidVersion() < apiMod.VersionData;
			}
			else
			{
				Logger.Error("Invalid correspondance between installation and api in UpdateUtils.NeedsUpdated");
				return false;
			}
		}

		public static bool IsCorrectApi(InstalledModDetail installedMod, APIMod apiMod)
		{
			if (installedMod is null || apiMod is null)
			{
				Logger.Error("Null argument in UpdateUtils.IsCorrectApi");
				return false;
			}

			if (installedMod.CleanName == apiMod.CleanName || apiMod.Aliases.Contains(installedMod.CleanName))
			{
				return true;
			}
			else
			{
				return false;
			}
		}


	}
}
