#nullable disable
using System;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Platform.Compatibility
{
	public interface IShellPageRendererTracker : IDisposable
	{
		bool IsRootPage { get; set; }

		UIViewController ViewController { get; set; }

		Page Page { get; set; }
	}
}