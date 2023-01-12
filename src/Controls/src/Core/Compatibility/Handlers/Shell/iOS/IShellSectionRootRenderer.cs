#nullable disable
using System;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Platform.Compatibility
{
	public interface IShellSectionRootRenderer : IDisposable
	{
		bool ShowNavBar { get; }

		UIViewController ViewController { get; }
	}
}