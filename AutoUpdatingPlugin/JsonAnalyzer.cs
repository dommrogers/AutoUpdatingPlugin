using MelonLoader.TinyJSON;
using System;

namespace AutoUpdatingPlugin
{
	internal static class JsonAnalyzer
	{
		//internal static BuildInfoDetail GetBuildInfoFromJson(string jsonText, string fileName)
		//{
		//	BuildInfoDetail result = new BuildInfoDetail();

		//	if (!string.IsNullOrWhiteSpace(jsonText))
		//	{
		//		try
		//		{
		//			ProxyObject? dict = JSON.Load(jsonText) as ProxyObject;

		//			TrySetString(dict, "Name", ref result.Name);
		//			TrySetString(dict, "Version", ref result.Version);
		//			//TrySetString(dict, "Author", ref result.Author);
		//			//TrySetString(dict, "Description", ref result.Description);
		//		}
		//		catch (Exception e)
		//		{
		//			Logger.Error($"Encountered an error while attempting to parse the build info from {fileName}: {e}");
		//			return new BuildInfoDetail();
		//		}
		//	}

		//	return result;
		//}

		//private static void TrySetString(ProxyObject dict, string key, ref string field)
		//{
		//	foreach (System.Collections.Generic.KeyValuePair<string, Variant> pair in dict)
		//	{
		//		if (pair.Key == key)
		//		{
		//			field = pair.Value;
		//		}
		//	}
		//}
	}
}
