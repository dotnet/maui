using System;
using System.Threading.Tasks;

namespace Microsoft.Maui.Handlers
{
	public partial class ToolbarHandler : ElementHandler<IToolbar, MauiToolbar>
	{
		protected override MauiToolbar CreatePlatformElement() => new();

		protected override void ConnectHandler(MauiToolbar platformView)
		{
			platformView.IconPressed += OnIconPressed;
			base.ConnectHandler(platformView);
		}

		protected override void DisconnectHandler(MauiToolbar platformView)
		{
			platformView.IconPressed -= OnIconPressed;
			base.DisconnectHandler(platformView);
		}

		public static void MapTitle(IToolbarHandler handler, IToolbar toolbar)
		{
			handler.PlatformView.UpdateTitle(toolbar);
		}

		async void OnIconPressed(object? sender, EventArgs args)
		{
			if (VirtualView.BackButtonVisible && VirtualView.IsVisible)
			{
				// Delays invoking the BackButtonClicked
				// so the other attached events can be invoked before the pop behavior is done on a FlyoutPage.
				await Task.Delay(100);

				MauiContext?.GetPlatformWindow().GetWindow()?.BackButtonClicked();
			}
		}
	}
}
