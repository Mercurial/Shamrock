using System;
using System.Collections.Generic;

namespace Shamrock.Models
{
	public class MacOSDisk
	{
		public string Identifier { get; set; }
		public MacOSDiskType Type { get; set; }
		public PartitionScheme Scheme { get; set; }
		public int Size { get; set; }
		public DiskSizeUnit SizeUnit { get; set; }
		public List<MacOSPartition> Partitions { get; set; }

		public MacOSDisk()
		{
			
		}
	}


}
