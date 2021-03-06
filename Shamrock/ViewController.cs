﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AppKit;
using Foundation;
using Security;
using Shamrock.Models;

namespace Shamrock
{
	public partial class ViewController : NSViewController
	{
		private string _status = string.Empty;
		private List<MacOSDisk> disks = new List<MacOSDisk>();

		public string Status
		{
			private set
			{
				_status = value;
				btnCreateInstaller.Title = _status;
			}
			get { return this._status; }
		}

		public ViewController(IntPtr handle) : base(handle)
		{
		}

		public async override void ViewDidLoad()
		{
			base.ViewDidLoad();
			// Do any additional setup after loading the view.

			//Init Controls
			imgLogo.Image = new NSImage("logo.png", true);
			txtDiskName.Enabled = false;

			await RefreshDisksList();
			//await MacOSDiskHelper.MountVolumeAsync("/dev/disk2s1");
			pbInstall.Hidden = true;
		}

		async partial void BtnRefresh_Clicked(NSObject sender)
		{
			await RefreshDisksList();
		}

		async Task RefreshDisksList()
		{
			disks = await MacOSDiskHelper.ListDisksAsync();
			cmbDisk.RemoveAll();

			foreach (MacOSDisk disk in disks)
			{
				foreach (MacOSPartition partition in disk.Partitions)
				{
					if (partition.Type == PartitionType.Apple_HFS)
					{
						cmbDisk.Add(new NSString(partition.PartitionName));
						cmbDisk.StringValue = partition.PartitionName;
					}
				}
			}
		}

		partial void btnBrowseAppFile_Clicked(NSObject sender)
		{
			var dlg = NSOpenPanel.OpenPanel;
			dlg.CanChooseFiles = true;
			dlg.CanChooseDirectories = false;
			dlg.AllowedFileTypes = new string[] { "app" };

			if (dlg.RunModal() == 1)
			{
				// Nab the first file
				var url = dlg.Urls[0];

				if (url != null)
				{
					var path = url.Path;
					txtAppFile.StringValue = path;
				}
			}
		}


		/// <summary>
		/// Event Handler for when the chkCreateDMG checkbox checked status has changed
		/// </summary>
		/// <param name="sender">Sender.</param>
		partial void chkCreateDMG_Changed(NSObject sender)
		{
			if (chkCreateDMG.State == NSCellStateValue.On)
			{
				txtDiskName.Enabled = true;
				cmbDisk.Enabled = false;
			}
			else
			{
				txtDiskName.StringValue = string.Empty;
				txtDiskName.Enabled = false;
				cmbDisk.Enabled = true;
			}
		}

		async partial void btnCreateInstaller_Clicked(NSObject sender)
		{
			pbInstall.Hidden = false;
			pbInstall.StartAnimation(sender);
			btnCreateInstaller.Enabled = false;
			MacOSAppFile AppFile = new MacOSAppFile(txtAppFile.StringValue);

			//Initialise Vars

			//Make sure to replace white spaces in between words with underscores ( _ )
			string VolumeDiskName = string.Join("_", txtDiskName.StringValue.Split(' '));

			AttachVirtualDiskImageResult usbAttachResult = new AttachVirtualDiskImageResult(false, null, null, null);

			bool Aborted = false;

			MacOSDisk currentDisk;
			string EFI_Ident = String.Empty;

			//Check if Installer is a valid installer.app file
			if (!AppFile.IsValid)
			{
				var alert = new NSAlert()
				{
					AlertStyle = NSAlertStyle.Critical,
					InformativeText = "Invalid OSX/MacOS Installer.app File, please locate the correct installer and try again..",
					MessageText = "Invalid Installer.app File!",
				};
				alert.RunModal();
				EnableCreateButton();
				return;
			}

			//Check if we are using Virtual Disk Image instead of Physical USB Drive
			if (chkCreateDMG.State == NSCellStateValue.On)
			{
				//Check if user entered a disk name
				if (VolumeDiskName.Trim() == string.Empty)
				{
					var alert = new NSAlert()
					{
						AlertStyle = NSAlertStyle.Critical,
						InformativeText = "Invalid Image Name",
						MessageText = "Invalid!",
					};
					alert.RunModal();
					EnableCreateButton();
					return;
				}

				this.Status = "Creating Virtual Disk Image...";
				string path = $"{Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)}/{VolumeDiskName}.dmg";
				await MacOSDiskHelper.CreateVirtualDiskImageAsync(path, VolumeDiskName, 8, DiskSizeUnit.GB, DiskFileSystem.JHFSPlus);
				this.Status = "Attaching Virtual Disk Image...";
				usbAttachResult = await MacOSDiskHelper.AttachVirtualDiskImageAsync(path);
			}
			else
			{
				foreach (MacOSDisk disk in disks)
				{
					foreach (MacOSPartition part in disk.Partitions)
					{
						if (part.PartitionName == cmbDisk.StringValue)
						{
							VolumeDiskName = cmbDisk.StringValue;
							usbAttachResult = new AttachVirtualDiskImageResult(true, $"/dev/{disk.Identifier}", VolumeDiskName , $"/dev/{part.Identifier}");
							currentDisk = disk;
						}

						if (part.Type == PartitionType.EFI)
							EFI_Ident = part.Identifier;
					}
				}
			}


			this.Status = "Formatting Disk Image...";
			await MacOSDiskHelper.EraseDiskAsync(usbAttachResult.DiskName, VolumeDiskName, PartitionScheme.GPT, DiskFileSystem.JHFSPlus);
			usbAttachResult.VolumeName = $"/Volumes/{VolumeDiskName}";

			if (rbMethodOld.State != NSCellStateValue.On)
			{

				this.Status = "Creating Install Media to USB, this may take awhile...";
				await Task.Run(() =>
				{
					var defaults = AuthorizationFlags.Defaults;

					using (var auth = Authorization.Create(defaults))
					{
						//var args = new[] { "-c", $"\"\"\"/Applications/Install macOS Sierra.app/Contents/Resources/createinstallmedia\" --applicationpath \"{AppFile.Path}\" --volume \"{usbAttachResult.VolumeName}\" --no interaction\"\"" };
						if (File.Exists($"{Environment.CurrentDirectory}/createinstallmedia_temp"))
							File.Delete($"{Environment.CurrentDirectory}/createinstallmedia_temp");

						if (File.Exists($"{Environment.CurrentDirectory}/cim_done"))
							File.Delete($"{Environment.CurrentDirectory}/cim_done");
						
						if (File.Exists($"{Environment.CurrentDirectory}/cim_started"))
							File.Delete($"{Environment.CurrentDirectory}/cim_started");
						
						string script = File.ReadAllText($"{Environment.CurrentDirectory}/createinstallmedia");

						script = script.Replace("%resourceFolder%", Environment.CurrentDirectory);
						script = script.Replace("%appDir%", AppFile.Path);
						script = script.Replace("%volumePath%", usbAttachResult.VolumeName);

						File.WriteAllText($"{Environment.CurrentDirectory}/createinstallmedia_temp", script, new UTF8Encoding(false));

						ProcessStarter p = new ProcessStarter("/bin/chmod", $"755 createinstallmedia_temp");
						p.Start();

						var args = new[] { "-c", $"\"\"{Environment.CurrentDirectory}/createinstallmedia_temp\"\"" };
						auth.ExecuteWithPrivileges("/bin/sh", defaults, args);

						Thread.Sleep(5000);

						Aborted = !File.Exists($"{Environment.CurrentDirectory}/cim_started");

						while (!Aborted && !File.Exists($"{Environment.CurrentDirectory}/cim_done"))
						{
							Thread.Sleep(1000);
						}

					}

				});

				await MacOSDiskHelper.RenameVolumeAsync(usbAttachResult.VolumeIdentifier, VolumeDiskName);
			}
			else
			{
				this.Status = "Attaching InstallESD.dmg...";
				var attachESDResult = await MacOSDiskHelper.AttachVirtualDiskImageAsync(AppFile.InstallESDPath);
				this.Status = "Attaching BaseSystem.dmg...";
				var attachBaseResult = await MacOSDiskHelper.AttachVirtualDiskImageAsync($"{attachESDResult.VolumeName}/BaseSystem.dmg");
				this.Status = "Restoring BaseSystem to USB...";

				await Task.Run(() =>
				{
					var defaults = AuthorizationFlags.Defaults;

					using (var auth = Authorization.Create(defaults))
					{
						//var args = new[] { "-c", $"\"\"\"/Applications/Install macOS Sierra.app/Contents/Resources/createinstallmedia\" --applicationpath \"{AppFile.Path}\" --volume \"{usbAttachResult.VolumeName}\" --no interaction\"\"" };
						if (File.Exists($"{Environment.CurrentDirectory}/createinstallmedia_old_temp"))
							File.Delete($"{Environment.CurrentDirectory}/createinstallmedia_old_temp");

						if (File.Exists($"{Environment.CurrentDirectory}/cim_done"))
							File.Delete($"{Environment.CurrentDirectory}/cim_done");

						if (File.Exists($"{Environment.CurrentDirectory}/cim_started"))
							File.Delete($"{Environment.CurrentDirectory}/cim_started");

						string script = File.ReadAllText($"{Environment.CurrentDirectory}/createinstallmedia_old");

						script = script.Replace("%resourceFolder%", Environment.CurrentDirectory);
						script = script.Replace("%source%", attachBaseResult.VolumeName);
						script = script.Replace("%target%", usbAttachResult.VolumeName);

						File.WriteAllText($"{Environment.CurrentDirectory}/createinstallmedia_old_temp", script, new UTF8Encoding(false));

						ProcessStarter p = new ProcessStarter("/bin/chmod", $"755 createinstallmedia_old_temp");
						p.Start();

						var args = new[] { "-c", $"\"\"{Environment.CurrentDirectory}/createinstallmedia_old_temp\"\"" };
						auth.ExecuteWithPrivileges("/bin/sh", defaults, args);

						Thread.Sleep(5000);

						Aborted = !File.Exists($"{Environment.CurrentDirectory}/cim_started");

						while (!Aborted && !File.Exists($"{Environment.CurrentDirectory}/cim_done"))
						{
							Thread.Sleep(1000);
						}

					}

					//var defaults = AuthorizationFlags.Defaults;

					//using (var auth = Authorization.Create(defaults))
					//{

					//	//await MacOSDiskHelper.RestoreDiskAsync(attachBaseResult.VolumeName, usbAttachResult.VolumeName);

					//	var args = new[] { "-c", $"\"\"/usr/sbin/asr -source \"{attachBaseResult.VolumeName}\" -target \"{usbAttachResult.VolumeName}\" -erase -noprompt\"\"" };
					//	auth.ExecuteWithPrivileges("/bin/sh", defaults, args);

					//}

				});

				await MacOSDiskHelper.RenameVolumeAsync(usbAttachResult.VolumeIdentifier, VolumeDiskName);
				this.Status = "Unmounting BaseSystem...";
				await MacOSDiskHelper.UnmountVolumeAsync(attachBaseResult.VolumeName);
				this.Status = "Copying Installer Packages folder";
				await Task.Run(() =>
				{
					if (File.Exists($"{usbAttachResult.VolumeName}/System/Installation/Packages"))
						File.Delete($"{usbAttachResult.VolumeName}/System/Installation/Packages");
					//Use OS cp tool instead of C#, less codes and probably much faster
					ProcessStarter p = new ProcessStarter("/bin/cp", $"-R -p \"{attachESDResult.VolumeName}/Packages\" \"{usbAttachResult.VolumeName}/System/Installation/Packages\"");
					p.Start();
				});
				this.Status = "Copying BaseSystem.chunklist";
				await Task.Run(() =>
				{
					ProcessStarter p = new ProcessStarter("/bin/cp", $"-R -p \"{attachESDResult.VolumeName}/BaseSystem.chunklist\" \"{usbAttachResult.VolumeName}/BaseSystem.chunklist\"");
					p.Start();
				});
				this.Status = "Unmounting InstallESD";
				await MacOSDiskHelper.UnmountVolumeAsync(attachESDResult.VolumeName);
			}


			if (!Aborted)
			{
				var tDisks = await MacOSDiskHelper.ListDisksAsync();
				foreach (MacOSDisk disk in tDisks)
				{
					foreach (MacOSPartition part in disk.Partitions)
					{
						if (part.Type == PartitionType.EFI)
							await MacOSDiskHelper.UnmountVolumeAsync($"/dev/{part.Identifier}");
					}
				}

				this.Status = "Mounting EFI Partition";
				await MacOSDiskHelper.MountVolumeAsync(EFI_Ident);


				this.Status = "Copying CLOVER to EFI Partition";
				await Task.Run(() =>
				{
					ProcessStarter p = new ProcessStarter("/bin/cp", $"-R -p \"{Environment.CurrentDirectory}/CLOVER/3911/EFI\" \"/Volumes/EFI/\"");
					p.Start();
				});
			}
		

			EnableCreateButton();
			pbInstall.Hidden = true;
			pbInstall.StopAnimation(sender);

			if (Aborted)
			{

				var successAlert = new NSAlert()
				{
					AlertStyle = NSAlertStyle.Warning,
					InformativeText = "Install Creation has been aborted..",
					MessageText = "Aborted!",
				};
				successAlert.RunModal();
			}
			else
			{

				var successAlert = new NSAlert()
				{
					AlertStyle = NSAlertStyle.Informational,
					InformativeText = "OSX/MacOS Installer has been succesfully created..",
					MessageText = "Success!",
				};
				successAlert.RunModal();
			}
		}

		partial void rbMethodOld_Changed(NSObject sender)
		{
			if (rbMethodOld.State == NSCellStateValue.On)
			{
				rbMethodNew.State = NSCellStateValue.Off;
			}
			else
			{
				rbMethodNew.State = NSCellStateValue.On;
			}
		}

		partial void rbMethodNew_Changed(NSObject sender)
		{
			if (rbMethodNew.State == NSCellStateValue.On)
			{
				rbMethodOld.State = NSCellStateValue.Off;
			}
			else
			{
				rbMethodOld.State = NSCellStateValue.On;
			}
		}


		void EnableCreateButton()
		{
			this.Status = "CREATE INSTALLER";
			btnCreateInstaller.Enabled = true;
		}

		public override NSObject RepresentedObject
		{
			get
			{
				return base.RepresentedObject;
			}
			set
			{
				base.RepresentedObject = value;
				// Update the view, if already loaded.
			}
		}
	}
}
