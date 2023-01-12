#nullable disable
using System;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Platform.Compatibility
{
	public interface IShellTabBarAppearanceTracker : IDisposable
	{
		void ResetAppearance(UITabBarController controller);
		void SetAppearance(UITabBarController controller, ShellAppearance appearance);
		void UpdateLayout(UITabBarController controller);
	}
}