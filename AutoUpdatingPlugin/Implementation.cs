using MelonLoader;
using MelonLoader.Utils;
using System.Reflection.Metadata.Ecma335;

[assembly: MelonPriority(-999)]
namespace AutoUpdatingPlugin
{
	public sealed class Implementation : MelonPlugin
	{

		internal static string GameFolder { get; set; } = null;

		static int loopCount = 0;
		internal static bool UseMelonLoader { get; set; } = true;

		internal static bool doUpdate = false;

		public override void OnPreInitialization()
		{
			Run();
		}


		public static void Run(string? gameDir = null, bool ml = true)
		{
			UseMelonLoader = ml;
			if (!UseMelonLoader && !string.IsNullOrEmpty(gameDir))
			{
				GameFolder = gameDir;
			}
			else
			{
				GameFolder = MelonEnvironment.GameRootDirectory;
			}

			// get the api data before anything else
			try
			{
				Logger.Debug("APIList.FetchRemoteMods");
				APIList.FetchRemoteMods();
			}
			catch (System.Exception e)
			{
				Logger.Error("Failed to get API data :\n" + e);
				return;
			}

			// cleanup any old .dll.rem files
			try
			{
				Logger.Debug("DllFileChecker.Cleanup");
				DllFileChecker.Cleanup();
			}
			catch (System.Exception e)
			{
				Logger.Error("Failed to cleanup .dll.rem files :\n" + e);
			}

			// do update checks
			try
			{
				Logger.Debug("UpdateMods");
				UpdateMods();
			}
			catch (System.Exception e)
			{
				Logger.Error("Failed to update mods :\n" + e);
			}

			// scan for incorrect source code
			try
			{
				Logger.Debug("SourceScanner.Scan");
				SourceScanner.Scan();
			}
			catch (System.Exception e)
			{
				Logger.Error("Failed to scan for source code :\n" + e);
			}

		}



		public static void UpdateMods()
		{
			// extract first so we have a full idea of what is installed
			ZipFileHandler.ExtractZipFilesInDirectory(FileUtils.ModsFolder);

			loopCount++;

			if (loopCount >= 5)
			{
				Logger.Error("Max loops of 5 hit, stopping...");
				return;
			}

			InstalledModList.ScanModFolder();

			IntersectedList.GenerateLists();

			ModUpdater.DownloadAndUpdateMods();

			if (ModUpdater.pluginUpdated)
			{
				ForceClose();
			}

			int depCount = DependencyHandler.InstallAllMissingDependencies();

			ZipFileHandler.ExtractZipFilesInDirectory(FileUtils.ModsFolder);

			// perform update again if we have installed any deps.
			if (depCount > 0)
			{
				Logger.Minor($"New Dependencies Installed ({depCount})");
				UpdateMods();
			}
		}

		internal static void ForceClose()
		{
			Logger.Warning("\n\n\n\n\n!! A plugin was updated, the game requires a restart...\n\n\n\n\nclosing game in 5 seconds");

			System.Diagnostics.Stopwatch? x = new System.Diagnostics.Stopwatch();
			x.Start();
			while (x.ElapsedMilliseconds < 2) { }
			System.Environment.Exit(0);
		}
	}
}
