using System;
using UIKit;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
{
	public interface IShellSectionRootRenderer : IDisposable
	{
		bool ShowNavBar { get; }

		UIViewController ViewController { get; }
	}
}