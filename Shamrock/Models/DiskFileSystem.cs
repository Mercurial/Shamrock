using System;
namespace Shamrock.Models
{

	public sealed class DiskFileSystem
	{

		private readonly string name;
		private readonly int value;

		public static readonly DiskFileSystem JHFSPlus = new DiskFileSystem(1, "JHFS+");

		private DiskFileSystem(int value, String name)
		{
			this.name = name;
			this.value = value;
		}

		public override String ToString()
		{
			return name;
		}

	}
}
