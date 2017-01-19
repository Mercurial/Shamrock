// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace Shamrock
{
	[Register ("ViewController")]
	partial class ViewController
	{
		[Outlet]
		AppKit.NSButton btnBrowseAppFile { get; set; }

		[Outlet]
		AppKit.NSButton btnCreateInstaller { get; set; }

		[Outlet]
		AppKit.NSButton chkCreateDMG { get; set; }

		[Outlet]
		AppKit.NSComboBox cmbDisk { get; set; }

		[Outlet]
		AppKit.NSImageView imgLogo { get; set; }

		[Outlet]
		AppKit.NSButton rbMethodNew { get; set; }

		[Outlet]
		AppKit.NSButton rbMethodOld { get; set; }

		[Outlet]
		AppKit.NSTextField txtAppFile { get; set; }

		[Outlet]
		AppKit.NSTextField txtDiskName { get; set; }

		[Action ("btnBrowseAppFile_Clicked:")]
		partial void btnBrowseAppFile_Clicked (Foundation.NSObject sender);

		[Action ("btnCreateInstaller_Clicked:")]
		partial void btnCreateInstaller_Clicked (Foundation.NSObject sender);

		[Action ("chkCreateDMG_Changed:")]
		partial void chkCreateDMG_Changed (Foundation.NSObject sender);

		[Action ("rbMethodNew_Changed:")]
		partial void rbMethodNew_Changed (Foundation.NSObject sender);

		[Action ("rbMethodOld_Changed:")]
		partial void rbMethodOld_Changed (Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (btnBrowseAppFile != null) {
				btnBrowseAppFile.Dispose ();
				btnBrowseAppFile = null;
			}

			if (btnCreateInstaller != null) {
				btnCreateInstaller.Dispose ();
				btnCreateInstaller = null;
			}

			if (chkCreateDMG != null) {
				chkCreateDMG.Dispose ();
				chkCreateDMG = null;
			}

			if (cmbDisk != null) {
				cmbDisk.Dispose ();
				cmbDisk = null;
			}

			if (imgLogo != null) {
				imgLogo.Dispose ();
				imgLogo = null;
			}

			if (txtAppFile != null) {
				txtAppFile.Dispose ();
				txtAppFile = null;
			}

			if (txtDiskName != null) {
				txtDiskName.Dispose ();
				txtDiskName = null;
			}

			if (rbMethodOld != null) {
				rbMethodOld.Dispose ();
				rbMethodOld = null;
			}

			if (rbMethodNew != null) {
				rbMethodNew.Dispose ();
				rbMethodNew = null;
			}
		}
	}
}
