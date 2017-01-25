using System;
using System.Collections.Generic;
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

		public static async Task<List<MacOSDisk>> ListDisksAsync()
		{
			var result = new List<MacOSDisk>();

			await Task.Run(() =>
			{
				ProcessStarter p = new ProcessStarter("/usr/sbin/diskutil", $"list");
				p.Start();

				if (p.Error.Trim() != string.Empty)
					return;

				string output = p.Output;
				//string output = File.ReadAllText($"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}/diskutil2.txt");
				string[] lines = output.Split('\n');

				bool isNewDisk = false;
				string identifier = string.Empty;
				PartitionScheme scheme;
				int size = 0;
				DiskSizeUnit unit = DiskSizeUnit.GB;
				MacOSDiskType type = MacOSDiskType.Internal;

				foreach (string line in lines)
				{
					if (line.Contains("/dev/"))
					{
						isNewDisk = true;

						if (line.Contains("internal, physical"))
							type = MacOSDiskType.Internal;
						else
							type = MacOSDiskType.Virtual;
						
						continue;
					}

					if (isNewDisk)
					{
						if (line.Contains("0:"))
						{
							var info = line.Split(' ');
							List<string> tempInfo = new List<string>();

							foreach (string i in info) if (i.Trim() != string.Empty) tempInfo.Add(i);
							info = tempInfo.ToArray();

							switch (info[1].Trim())
							{
								case "GUID_partition_scheme":
									scheme = PartitionScheme.GPT;
									break;
								//@todo add APM and MBR support
								default:
									scheme = PartitionScheme.APM;
									break;
							}

							size = (int)double.Parse(info[info.Length - 3].Replace("*", string.Empty).Replace("+", ""));
							switch (info[info.Length - 2])
							{
								case "GB":
									unit = DiskSizeUnit.GB;
									break;
								case "MB":
									unit = DiskSizeUnit.MB;
									break;
								case "KB":
									unit = DiskSizeUnit.KB;
									break;
							}

							identifier = info[info.Length - 1].Trim();

							result.Add(new MacOSDisk()
							{
								Identifier = identifier,
								Partitions = new List<MacOSPartition>(),
								Scheme = scheme,
								Size = size,
								SizeUnit = unit,
								Type = type
							});
							isNewDisk = false;
						}
					}
					else if (line.Trim() == string.Empty)
					{
						continue;
					}
					else
					{
						try
						{
							var lastDisk = result[result.Count - 1];
							MacOSPartition partition = new MacOSPartition(lastDisk);
							var info = line.Split(' ');

							List<string> tempInfo = new List<string>();

							foreach (string i in info) if (i.Trim() != string.Empty) tempInfo.Add(i);
							info = tempInfo.ToArray();

							partition.Identifier = info[info.Length - 1];
							partition.PartitionName = info[2];

							double psize = 0;
							int x = 0;
							while (!double.TryParse(info[3 + x], out psize))
							{
								partition.PartitionName += $" {info[3 + x]}";
								x++;
							}

							switch (info[1].Trim())
							{
								case "EFI":
									partition.Type = PartitionType.EFI;
									break;
								case "Apple_HFS":
									partition.Type = PartitionType.Apple_HFS;
									break;
								// Too Lazy to handle all types of partition for now :D
								default:
									partition.Type = PartitionType.None;
									break;
							}

							partition.Size = (int)psize;
							switch (info[info.Length - 2])
							{
								case "GB":
									partition.SizeUnit = DiskSizeUnit.GB;
									break;
								case "MB":
									partition.SizeUnit = DiskSizeUnit.MB;
									break;
								case "KB":
									partition.SizeUnit = DiskSizeUnit.KB;
									break;
							}

							lastDisk.Partitions.Add(partition);

						}
						catch(Exception ex)
						{
							continue;
						}

					}
				}

			});

			return result;
		}


		public static async Task<bool> MountVolumeAsync(string volumeIdent)
		{
			bool result = false;
			ProcessStarter p = new ProcessStarter("/usr/sbin/diskutil", $"mount {volumeIdent}");
			p.Start();

			if (p.Error.Trim() != string.Empty)
				return false;

			string output = p.Output;

			return output.Contains("mounted");
		}


	}


	public class AttachVirtualDiskImageResult
	{
		public bool AttachSuccess { get; set; }
		public string DiskName { get; set; }
		public string VolumeName { get; set; }
		public string VolumeIdentifier { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Shamrock.Models.AttachVirtualDiskImageResult"/> class.
		/// </summary>
		/// <param name="success">If set to <c>true</c> success.</param>
		/// <param name="diskName">Disk name.</param>
		/// <param name="volumeName">Volume name.</param>
		/// <param name="volumeIdent">Volume Identifier.</param>
		public AttachVirtualDiskImageResult(bool success, string diskName, string volumeName, string volumeIdent )
		{
			this.AttachSuccess = success;
			this.DiskName = diskName;
			this.VolumeName = volumeName;
			this.VolumeIdentifier = volumeIdent;
		}
	}

}
