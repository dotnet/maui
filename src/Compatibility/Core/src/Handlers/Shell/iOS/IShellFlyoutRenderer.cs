using System;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Platform.Compatibility
{
	public interface IShellFlyoutRenderer : IDisposable
	{
		UIViewController ViewController { get; }

		UIView View { get; }

		void AttachFlyout(IShellContext context, UIViewController content);
	}
}