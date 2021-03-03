using System;
using UIKit;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
{
	public interface IShellFlyoutRenderer : IDisposable
	{
		UIViewController ViewController { get; }

		UIView View { get; }

		void AttachFlyout(IShellContext context, UIViewController content);
	}
}