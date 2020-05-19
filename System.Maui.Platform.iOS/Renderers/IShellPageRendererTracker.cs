using System;
using UIKit;

namespace System.Maui.Platform.iOS
{
	public interface IShellPageRendererTracker : IDisposable
	{
		bool IsRootPage { get; set; }

		UIViewController ViewController { get; set; }

		Page Page { get; set; }
	}
}