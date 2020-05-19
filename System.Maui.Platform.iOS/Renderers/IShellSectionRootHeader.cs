using System;
using UIKit;

namespace System.Maui.Platform.iOS
{
	public interface IShellSectionRootHeader : IDisposable
	{
		UIViewController ViewController { get; }
		ShellSection ShellSection { get; set; }
	}
}