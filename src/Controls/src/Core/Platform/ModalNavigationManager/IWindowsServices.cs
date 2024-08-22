using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Maui.Controls
{
#pragma warning disable RS0016 // Add public types and members to the declared API
	public interface IWindowsServices
	{
		Window? GetWindow();
	}

	// At some point we can expand this and add Modal NAvigationManager / alertmanager etc
	public class WindowsServices : IWindowsServices
	{
		WeakReference<Window>? _window;

		public Window? GetWindow()
		{
			if (_window is not null)
				return _window?.GetTargetOrDefault();

			var window = WindowCoreServices.Current?.Window;

			if (window is Window win)
			{
				_window = new WeakReference<Window>(win);
				return win;
			}

			return null;

		}
	}
#pragma warning restore RS0016 // Add public types and members to the declared API
}