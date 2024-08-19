

using System;
using System.Collections.Generic;
using System.Text.Json;

namespace AutoUpdatingPlugin
{
	class GlobalDependencyData
	{

		public List<GDEntry> Entries = new List<GDEntry>();

		public GlobalDependencyData(string data)
		{
			if(string.IsNullOrEmpty(data))
			{
				return;
			}

			Entries = JsonSerializer.Deserialize<List<GDEntry>>(data);
		}



	}

	class GDEntry
	{
		public string? Mod { get; set; } = null;
		public string[] Requires { get; set; } = Array.Empty<string>();
	}

}
