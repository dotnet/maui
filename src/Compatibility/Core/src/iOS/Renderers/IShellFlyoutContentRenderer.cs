using System;
using UIKit;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
{
	public interface IShellFlyoutContentRenderer
	{
		UIViewController ViewController { get; }

		event EventHandler WillAppear;

		event EventHandler WillDisappear;
	}
}