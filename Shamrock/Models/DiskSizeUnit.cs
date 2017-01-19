using System;
namespace Shamrock.Models
{

	public sealed class DiskSizeUnit
	{

		private readonly string _name;
		private readonly int _value;

		public static readonly DiskSizeUnit KB = new DiskSizeUnit(1, "k");
		public static readonly DiskSizeUnit MB = new DiskSizeUnit(2, "m");
		public static readonly DiskSizeUnit GB = new DiskSizeUnit(3, "g");

		private DiskSizeUnit(int value, String name)
		{
			this._name = name;
			this._value = value;
		}

		public override String ToString()
		{
			return _name;
		}

	}
}
