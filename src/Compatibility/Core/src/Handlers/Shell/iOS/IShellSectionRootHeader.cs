using System;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Platform.Compatibility
{
	public interface IShellSectionRootHeader : IDisposable
	{
		UIViewController ViewController { get; }
		ShellSection ShellSection { get; set; }
	}
}