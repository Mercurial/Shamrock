using System;
using System.IO;

namespace Shamrock.Models
{
	public class MacOSAppFile
	{

		public string Path
		{
			private set;
			get;
		}
		public MacOSAppFile(string path)
		{
			this.Path = path;
		}


		public bool IsValid
		{
			get
			{
				return File.Exists(this.InstallESDPath);
			}
		}

		public string InstallESDPath
		{
			get
			{
				return $"{this.Path}/Contents/SharedSupport/InstallESD.dmg";
			}
		}

		public string CreateInstallMediaBinPath
		{
			get
			{
				return $"{this.Path}/Contents/Resources/createinstallmedia";
			}
		}
	}
}
