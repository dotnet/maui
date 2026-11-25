#nullable disable
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;

namespace Microsoft.Maui.Controls
{
	public partial class Button
	{
		Application _themeApplication;

		private protected override void OnHandlerChangingCore(HandlerChangingEventArgs args)
		{
			base.OnHandlerChangingCore(args);

			if (_themeApplication is not null)
				_themeApplication.RequestedThemeChanged -= OnRequestedThemeChanged;

			_themeApplication = args.NewHandler is not null ? Application.Current : null;
			if (_themeApplication is not null)
				_themeApplication.RequestedThemeChanged += OnRequestedThemeChanged;
		}

		void OnRequestedThemeChanged(object sender, AppThemeChangedEventArgs args)
		{
			if (sender is not Application application)
				return;

			Dispatcher.DispatchIfRequired(() =>
			{
				if (ReferenceEquals(_themeApplication, application) &&
					application.RequestedTheme == args.RequestedTheme &&
					Handler is ButtonHandler handler)
					handler.RefreshDefaultTheme(args.RequestedTheme);
			});
		}

		public static void MapText(IButtonHandler handler, Button button)
		{
			handler.PlatformView?.UpdateText(button);
		}

		public static void MapText(ButtonHandler handler, Button button) =>
			MapText((IButtonHandler)handler, button);

		public static void MapLineBreakMode(IButtonHandler handler, Button button)
		{
			handler.PlatformView?.UpdateLineBreakMode(button);
		}

		public static void MapRippleColor(IButtonHandler handler, Button button)
		{
			var color = button?.OnThisPlatform()?.GetRippleColor();
			handler.PlatformView?.UpdateRippleColor(color);
		}
	}
}
