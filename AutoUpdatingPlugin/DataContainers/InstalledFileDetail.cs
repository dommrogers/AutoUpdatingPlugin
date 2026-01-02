namespace AutoUpdatingPlugin
{
	internal enum InstalledFileType
	{
		DLL,
		ModComponent,
		ModScene,
		Zip
	}

	internal class InstalledFileDetail
	{
		public string name;
		public VersionData version;
		public string filepath;
		public InstalledFileType fileType;

		public InstalledFileDetail(string name, string version, string filepath, InstalledFileType fileType)
		{
			this.name = name;
			this.filepath = filepath;
			this.fileType = fileType;
			this.version = new VersionData(version);
		}

		public override string ToString() => name;
	}
}
