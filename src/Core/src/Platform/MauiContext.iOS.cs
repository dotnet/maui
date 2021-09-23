using System;
using UIKit;

namespace Microsoft.Maui
{
	public partial class MauiApplicationContext
	{
		public MauiApplicationContext(IServiceProvider services, UIApplicationDelegate applicationDelegate)
			: this(services)
		{
			ApplicationDelegate = applicationDelegate ?? throw new ArgumentNullException(nameof(applicationDelegate));
		}

		public UIApplicationDelegate? ApplicationDelegate { get; }
	}

	public partial class MauiContext
	{
		readonly WeakReference<UIWindow>? _window;

		public MauiContext(IServiceProvider services, UIWindow window)
			: this(services)
		{
			_window = new WeakReference<UIWindow>(window ?? throw new ArgumentNullException(nameof(window)));
		}

		public UIWindow? Window
		{
			get
			{
				if (_window == null)
					return null;

				return _window.TryGetTarget(out var window) ? window : null;
			}
		}
	}
}