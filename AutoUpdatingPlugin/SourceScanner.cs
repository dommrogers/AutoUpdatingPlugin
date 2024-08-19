using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoUpdatingPlugin
{
	internal static class SourceScanner
	{
		internal static void Scan()
		{

			string[] csFiles = Directory.GetFiles(FileUtils.ModsFolder, "*.cs", SearchOption.AllDirectories);

			List<string> csFolders = new List<string>();

			foreach (string csFile in csFiles)
			{
				string path = csFile.Replace(FileUtils.ModsFolder+"\\", null);
				string[] parts = path.Split(new char[] { '\\' });
//				Logger.Msg($"Source Files Found: ({string.Join("|", parts)}) " + parts.Length);

				string sourceFolder = parts[0];
				if(!string.IsNullOrEmpty(sourceFolder))
				{
					if(!csFolders.Contains(sourceFolder))
					{
						csFolders.Add(sourceFolder);
					}
				}
			}

			foreach (string csFolder in csFolders)
			{
				Logger.Error($"Source folder detected in Mods: {csFolder}");
			}




		}



	}
}
