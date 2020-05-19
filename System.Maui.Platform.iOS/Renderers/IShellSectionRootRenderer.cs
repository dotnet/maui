using System;
using UIKit;

namespace System.Maui.Platform.iOS
{
	public interface IShellSectionRootRenderer : IDisposable
	{
		bool ShowNavBar { get; }

		UIViewController ViewController { get; }
	}
}