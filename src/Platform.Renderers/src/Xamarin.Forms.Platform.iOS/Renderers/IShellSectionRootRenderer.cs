using System;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	public interface IShellSectionRootRenderer : IDisposable
	{
		bool ShowNavBar { get; }

		UIViewController ViewController { get; }
	}
}