using System;
using UIKit;

namespace System.Maui.Platform.iOS
{
	public interface IShellFlyoutRenderer : IDisposable
	{
		UIViewController ViewController { get; }

		UIView View { get; }

		void AttachFlyout(IShellContext context, UIViewController content);
	}
}