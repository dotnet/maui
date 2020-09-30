using System;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	public interface IShellItemRenderer : IDisposable
	{
		ShellItem ShellItem { get; set; }

		UIViewController ViewController { get; }
	}
}