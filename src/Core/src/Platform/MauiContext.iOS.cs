using System;
using System.Collections.Generic;
using System.Text;
using UIKit;

namespace Microsoft.Maui
{
	public partial class MauiContext
	{
		readonly WeakReference<UIWindow>? _window;
		public MauiContext(IServiceProvider services, UIWindow window) : this(services)
		{
			_window = new WeakReference<UIWindow>(window ?? throw new ArgumentNullException(nameof(window)));
		}


		public UIWindow? Window
		{
			get
			{
				if (_window == null)
					return null;

				UIWindow? window;
				if (_window.TryGetTarget(out window))
				{
					return window;
				}

				return null;
			}
		}
	}
}
