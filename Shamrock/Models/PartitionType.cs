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
		public static readonly PartitionType Microsoft_Basic_data = new PartitionType(4, "Microsoft_Basic_data");
		public static readonly PartitionType Apple_CoreStorage = new PartitionType(5, "Apple_CoreStorage");
		public static readonly PartitionType Microsoft_Reserved = new PartitionType(6, "Microsoft_Reserved");
		public static readonly PartitionType Windows_Recovery = new PartitionType(7, "Windows_Recovery");
		public static readonly PartitionType Linux_Filesystem = new PartitionType(8, "Linux_Filesystem");
		public static readonly PartitionType Linux_Swap = new PartitionType(9, "Linux_Swap");
		public static readonly PartitionType None = new PartitionType(100, "None");


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
