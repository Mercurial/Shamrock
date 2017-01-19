using System;
using System.Diagnostics;

namespace Shamrock.Models
{

	public class ProcessStarter
	{
		public string Output { get; set; }
		public string Error { get; set; }

		public string BinPath { get; private set; }
		public string Arguments { get; private set; }

		public ProcessStarter(string bin, string args)
		{
			BinPath = bin;
			Arguments = args;
		}

		public void Start(bool admin = false)
		{
			Process p = new Process();
			p.StartInfo = new ProcessStartInfo()
			{
				FileName = this.BinPath,
				Arguments = this.Arguments,
				//Arguments = $"info",
				UseShellExecute = false,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				CreateNoWindow = false
			};
			if (admin)
			{
				p.StartInfo.Verb = "runas";
				p.StartInfo.UseShellExecute = true;
				p.StartInfo.RedirectStandardOutput = false;
				p.StartInfo.RedirectStandardError = false;
			}
			p.Start();
			p.WaitForExit();
			if (!admin)
			{
				this.Output = p.StandardOutput.ReadToEnd();
				this.Error = p.StandardError.ReadToEnd();
			}
			p.Close();
		}
	}
}
