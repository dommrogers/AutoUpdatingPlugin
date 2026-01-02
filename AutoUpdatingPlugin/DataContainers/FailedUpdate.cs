namespace AutoUpdatingPlugin
{
	internal enum FailedUpdateReason
	{
		Unknown,
		DownloadError,
		SaveError,
		Disabled
	}

	internal class FailedUpdateInfo
	{
		public InstalledModDetail mod;
		public FailedUpdateReason reason;
		public string message;

		public FailedUpdateInfo(InstalledModDetail mod, FailedUpdateReason reason, string message)
		{
			this.mod = mod;
			this.reason = reason;
			this.message = message;
		}
	}
}