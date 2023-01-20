#nullable disable
using System;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Platform.Compatibility
{
	public interface IShellItemRenderer : IDisposable
	{
		ShellItem ShellItem { get; set; }

		UIViewController ViewController { get; }
	}
}