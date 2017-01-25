using System;
using System.IO;
using AppKit;

namespace Shamrock
{
	static class MainClass
	{
		static void Main(string[] args)
		{
			try
			{
				NSApplication.Init();
				NSApplication.Main(args);
			}
			catch(Exception ex)
			{
				FileStream fs = File.Open($"{Environment.CurrentDirectory}/error.log", FileMode.Append);
				StreamWriter writer = new StreamWriter(fs);
				writer.WriteLineAsync($"======= Error - {DateTime.Now.ToString()} =======");
				writer.WriteLineAsync(ex.Message);
				writer.WriteLineAsync($"================= Stack Trace ===================");
				writer.WriteLineAsync(ex.StackTrace);
				writer.WriteLineAsync($"=================================================");
			}
		}
	}
}
