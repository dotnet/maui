#nullable disable
using System;
using Microsoft.Maui.Controls.Platform;

namespace Microsoft.Maui.Controls
{
	public partial class InputView
	{
		internal static void MapFocus(IViewHandler handler, IView view, object args)
		{
			handler.ShowKeyboardIfFocused(view);
		}

		private protected override void OnHandlerChangingCore(HandlerChangingEventArgs args)
		{
			base.OnHandlerChangingCore(args);

			if (Application.Current is null)
			{
				return;
			}

			if (args.NewHandler is null)
			{
				Application.Current.RequestedThemeChanged -= OnRequestedThemeChanged;
			}

			if (args.NewHandler is not null && args.OldHandler is null)
			{
				Application.Current.RequestedThemeChanged += OnRequestedThemeChanged;
			}
		}

		void OnRequestedThemeChanged(object sender, AppThemeChangedEventArgs e)
		{
			OnPropertyChanged(nameof(TextColor));
		}
	}
}
