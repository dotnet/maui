using System;
using UIKit;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
{
	public interface IShellSectionRootHeader : IDisposable
	{
		UIViewController ViewController { get; }
		ShellSection ShellSection { get; set; }
	}
}