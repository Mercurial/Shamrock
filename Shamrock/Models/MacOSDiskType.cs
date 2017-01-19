using System;
namespace Shamrock.Models
{
	public sealed class MacOSDiskType
	{
		private readonly string name;
		private readonly int value;

		public static readonly MacOSDiskType Internal = new MacOSDiskType(1, "Internal");
		public static readonly MacOSDiskType Virtual = new MacOSDiskType(2, "Virtual");

		private MacOSDiskType(int value, String name)
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
