using System;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Platform.Compatibility
{
	public interface IShellFlyoutContentRenderer
	{
		UIViewController ViewController { get; }

		event EventHandler WillAppear;

		event EventHandler WillDisappear;
	}
}