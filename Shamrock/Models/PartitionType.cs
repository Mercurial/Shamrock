using System;
namespace Shamrock.Models
{
	public sealed class PartitionType
	{
		private readonly string name;
		private readonly int value;

		public static readonly PartitionType EFI = new PartitionType(1, "EFI");
		public static readonly PartitionType Apple_HFS = new PartitionType(2, "Apple_HFS");
		public static readonly PartitionType Apple_Boot = new PartitionType(3, "Apple_Boot");

		private PartitionType(int value, String name)
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
