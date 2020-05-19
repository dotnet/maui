using System;
using UIKit;

namespace System.Maui.Platform.iOS
{
	public interface IShellTabBarAppearanceTracker : IDisposable
	{
		void ResetAppearance(UITabBarController controller);
		void SetAppearance(UITabBarController controller, ShellAppearance appearance);
		void UpdateLayout(UITabBarController controller);
	}
}