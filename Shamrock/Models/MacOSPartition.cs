using System;
namespace Shamrock.Models
{
	public class MacOSPartition
	{
		public MacOSDisk Disk { get; set; }
		public string Identifier { get; set; }
		public int Size { get; set; }
		public DiskSizeUnit SizeUnit { get; set; }
		public string PartitionName { get; set; }
		public PartitionType Type { get; set; }

		public MacOSPartition(MacOSDisk disk)
		{
			this.Disk = disk;
		}
	}
}
