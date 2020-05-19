using System;
using UIKit;

namespace System.Maui.Platform.iOS
{
	public interface IShellItemRenderer : IDisposable
	{
		ShellItem ShellItem { get; set; }

		UIViewController ViewController { get; }
	}
}