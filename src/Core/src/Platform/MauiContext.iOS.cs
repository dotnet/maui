using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Animations;
using UIKit;

namespace Microsoft.Maui
{
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