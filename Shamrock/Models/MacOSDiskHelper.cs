using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Shamrock.Models
{
	static public class MacOSDiskHelper
	{
		public static async Task<bool> CreateVirtualDiskImageAsync(string path, string volumeName, int size, DiskSizeUnit sizeUnit, DiskFileSystem fileSystem)
		{
			bool result = false;
			await Task.Run(() =>
			{
				if (File.Exists(path))
					File.Delete(path);

				ProcessStarter p = new ProcessStarter("/usr/bin/hdiutil", $"create -size {size}{sizeUnit} -fs {fileSystem} -volname {volumeName} {path}");
				p.Start();
				result = p.Output.Contains("created");
			});
			return result;
		}

		public static async Task<AttachVirtualDiskImageResult> AttachVirtualDiskImageAsync(string path)
		{
			AttachVirtualDiskImageResult result = new AttachVirtualDiskImageResult(false, null, null, null);

			if (!File.Exists(path))
				return result;
			
			await Task.Run(() =>
			{
				ProcessStarter p = new ProcessStarter("/usr/bin/hdiutil", $"attach \"{path}\"");
				p.Start();

				if (p.Error.Trim() != string.Empty)
					return;

				string diskName = string.Empty;
				string volumeName = string.Empty;
				string volumeDisk = string.Empty;

				string output = p.Output;
				string[] outputArr = output.Split('\n');

				//Get DiskName and VolumeName from command output
				foreach (string line in outputArr)
				{
					if (line.Contains("GUID_partition_scheme"))
					{
						diskName = line.Split('\t')[0].Trim();
					}
					if (line.Contains("Apple_HFS") && line.Contains("/Volumes"))
					{
						volumeName = line.Split('\t')[2].Trim();
						volumeDisk = line.Split('\t')[0].Trim();
					}
				}

				result = new AttachVirtualDiskImageResult(true, diskName, volumeName, volumeDisk);
			});
			return result;
		}

		public static async Task<bool> UnmountVolumeAsync(string volumePath)
		{
			bool result = false;

			await Task.Run(() =>
			{
				ProcessStarter p = new ProcessStarter("/usr/bin/hdiutil", $"unmount \"{volumePath}\"");
				p.Start();

				if (p.Error.Trim() != string.Empty)
					return;

				result = true;
			});
			return result;
		}

		public static async Task<bool> EraseDiskAsync(string diskName, string partitionName, PartitionScheme scheme, DiskFileSystem fileSystem)
		{
			bool result = false;

			await Task.Run(() =>
			{
				ProcessStarter p = new ProcessStarter("/usr/sbin/diskutil", $"partitionDisk {diskName} {scheme} {fileSystem} \"{partitionName}\" 100%");
				p.Start();

				if (p.Error.Trim() != string.Empty)
					return;

				result = true;
			});

			return result;
		}

		public static async Task<bool> RestoreDiskAsync(string sourcePath, string targetPath)
		{
			bool result = false;

			await Task.Run(() =>
			{
				ProcessStarter p = new ProcessStarter("/usr/sbin/asr", $"-source \"{sourcePath}\" -target \"{targetPath}\" -erase -noprompt");
				p.Start();

				if (p.Error.Trim() != string.Empty)
					return;

				result = true;
			});

			return result;
		}


		public static async Task<bool> RenameVolumeAsync(string volumeDiskToRename, string newName)
		{
			bool result = false;

			await Task.Run(() =>
			{
				ProcessStarter p = new ProcessStarter("/usr/sbin/diskutil", $"rename {volumeDiskToRename} \"{newName}\"");
				p.Start();

				if (p.Error.Trim() != string.Empty)
					return;

				result = true;
			});

			return result;
		}

	}


	public class AttachVirtualDiskImageResult
	{
		public bool AttachSuccess { get; set; }
		public string DiskName { get; set; }
		public string VolumeName { get; set; }
		public string VolumeDisk { get; set; }

		public AttachVirtualDiskImageResult(bool success, string diskName, string volumeName, string volumeDisk )
		{
			this.AttachSuccess = success;
			this.DiskName = diskName;
			this.VolumeName = volumeName;
			this.VolumeDisk = volumeDisk;
		}
	}

}
