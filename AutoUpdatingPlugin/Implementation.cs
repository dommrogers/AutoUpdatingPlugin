using MelonLoader;

[assembly: MelonPriority(-999)]
namespace AutoUpdatingPlugin
{
    internal sealed class Implementation : MelonPlugin
    {

		static int loopCount = 0;

        public override void OnPreInitialization()
        {
            try
            {
                SelfUpdater.CheckVersion();

                AssetRipper.VersionUtilities.UnityVersion unityVersion = MelonLoader.InternalUtils.UnityInformationHandler.EngineVersion;
                if (unityVersion < AssetRipper.VersionUtilities.UnityVersion.Parse("2019.4.19"))
                {
                    Logger.Msg($"Skipping mod updates because TLD is outdated. Unity Version: {unityVersion}");
                    return;
                }

                
            }
            catch (System.Exception e)
            {
                Logger.Error("Failed to update mods:\n" + e);
            }

			UpdateMods();

        }

		

		public static void UpdateMods()
		{
			// extract first so we have a full idea of what is installed
			ZipFileHandler.ExtractZipFilesInDirectory(FileUtils.ModsFolder);

			APIList.FetchRemoteMods();

			UpdateModsStage2();
		}

		public static void UpdateModsStage2()
		{
			loopCount++;

			if (loopCount >= 5)
			{
				Logger.Error("Max loops of 5 hit, stopping...");
				return;
			}

			InstalledModList.ScanModFolder();

			IntersectedList.GenerateLists();

			ModUpdater.DownloadAndUpdateMods();

			int depCount = DependencyHandler.InstallAllMissingDependencies();

			ZipFileHandler.ExtractZipFilesInDirectory(FileUtils.ModsFolder);

			// perform update again if we have installed any deps.
			if (depCount > 0)
			{
				UpdateModsStage2();
			}
		}
	}
}
