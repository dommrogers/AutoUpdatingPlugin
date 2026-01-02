using Il2CppSystem.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Newtonsoft.Json;

namespace AutoUpdatingPlugin
{
	internal static class APIReader
	{
		internal static readonly string[] compatibleFileTypes = new string[] { ".dll", ".modcomponent", ".modscene", ".zip", ".rar", ".7z" };
		internal static APIMod[] Deserialize(string text)
		{
			if (string.IsNullOrWhiteSpace(text))
			{
				return new APIMod[0];
			}

			APIMod[] apiEntries = JsonConvert.DeserializeObject<APIMod[]>(text);

#if DEBUG
			File.WriteAllText(Path.Combine(FileUtils.PluginsFolder, "api.json"), text);
			File.WriteAllText(Path.Combine(FileUtils.PluginsFolder, "apiEntries.json"), JsonConvert.SerializeObject(apiEntries, Formatting.Indented));
#endif

			return apiEntries;
		}

		public static bool IsCompatibleLink(string? link)
		{
			if (string.IsNullOrWhiteSpace(link))
			{
				return false;
			}

			foreach (string fileType in compatibleFileTypes)
			{
				if (link.EndsWith(fileType))
				{
					return true;
				}
			}
			return false;
		}
	}
}
