using System;

namespace AutoUpdatingPlugin.ConsoleApp
{
	internal class Program
	{
		static void Main(string[] args)
		{
			Implementation.UpdateMods(@"C:\Program Files (x86)\Steam\steamapps\common\TheLongDark", false);
		}
	}
}
