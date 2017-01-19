using System;
namespace Shamrock
{
	public sealed class PartitionScheme
	{
		private readonly string name;
		private readonly int value;

		public static readonly PartitionScheme APM = new PartitionScheme(1, "APM");
		public static readonly PartitionScheme MBR = new PartitionScheme(2, "MBR");
		public static readonly PartitionScheme GPT = new PartitionScheme(3, "GPT");

		private PartitionScheme(int value, String name)
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