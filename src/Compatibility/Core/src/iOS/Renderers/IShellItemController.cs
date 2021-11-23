using System;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
{
	public interface IShellItemRenderer : IDisposable
	{
		ShellItem ShellItem { get; set; }

		UIViewController ViewController { get; }
	}
}