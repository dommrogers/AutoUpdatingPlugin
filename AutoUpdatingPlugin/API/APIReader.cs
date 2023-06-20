using MelonLoader.TinyJSON;
using System;
using System.Collections.Generic;

namespace AutoUpdatingPlugin
{
	internal static class APIReader
	{
		internal static readonly string[] compatibleFileTypes = new string[] { ".dll", ".modcomponent", ".modscene", ".zip" };
		internal static APIMod[] Deserialize(string text)
		{
			if (string.IsNullOrWhiteSpace(text))
			{
				return new APIMod[0];
			}

			List<APIMod> result = new List<APIMod>();

			ProxyObject mods = (ProxyObject)JSON.Load(text);

			foreach (KeyValuePair<string, Variant> mod in mods)
			{
				ProxyObject modData = (ProxyObject)mod.Value;
				APIMod apiMod = new APIMod();
				apiMod.name = modData["Name"];
				Logger.Msg(apiMod.name);
				apiMod.version = (VersionData)(string)modData["Version"];
				apiMod.type = (modData["Type"] == null) ? "" : modData["Type"];
				apiMod.aliases = MakeStringArray(modData["Aliases"] as ProxyArray);
				apiMod.dependencies = MakeStringArray(modData["Dependencies"] as ProxyArray);
				apiMod.enableUpdate = modData["Updater"]["enable_update"];
				apiMod.downloadlinks = MakeDownloadArray(modData["Updater"]["downloads"] as ProxyArray);
				result.Add(apiMod);
			}

			return result.ToArray();
		}

		public static string[] MakeStringArray(ProxyArray? proxy)
		{
			if (proxy == null)
			{
				Logger.Warning("null proxy array");
				return Array.Empty<string>();
			}
			string[] result = new string[proxy.Count];
			for (int i = 0; i < proxy.Count; i++)
			{
				result[i] = proxy[i];
			}
			return result;
		}

		public static string[] MakeDownloadArray(ProxyArray? proxy)
		{
			if (proxy == null)
			{
				return Array.Empty<string>();
			}

			List<string> result = new List<string>();
			for (int i = 0; i < proxy.Count; i++)
			{
				string link = proxy[i];
				if (IsCompatibleLink(link))
				{
					result.Add(link);
				}
				else
				{
					return new string[0];
				}
			}
			return result.ToArray();
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
