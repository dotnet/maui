using Microsoft.Maui.Graphics;
using Microsoft.UI.Windowing;

using PlatformWindow = Microsoft.UI.Xaml.Window;

namespace Microsoft.Maui.Embedding;

internal partial class EmbeddedWindowHandler
{
	protected override void ConnectHandler(PlatformWindow platformView)
	{
		base.ConnectHandler(platformView);

		var appWindow = platformView.GetAppWindow();
		if (appWindow is not null)
		{
			// then pass the actual size back to the user
			UpdateVirtualViewFrame(appWindow);

			// THEN attach the event to reduce churn
			appWindow.Changed += OnWindowChanged;
		}
	}

	protected override void DisconnectHandler(PlatformWindow platformView)
	{
		var appWindow = platformView.GetAppWindow();
		if (appWindow is not null)
			appWindow.Changed -= OnWindowChanged;

		base.DisconnectHandler(platformView);
	}

	void OnWindowChanged(AppWindow sender, AppWindowChangedEventArgs args)
	{
		if (!args.DidSizeChange && !args.DidPositionChange)
			return;

		UpdateVirtualViewFrame(sender);
	}

	void UpdateVirtualViewFrame(AppWindow appWindow)
	{
		var size = appWindow.Size;
		var pos = appWindow.Position;

		var density = PlatformView.GetDisplayDensity();

		VirtualView.FrameChanged(new Rect(
			pos.X / density, pos.Y / density,
			size.Width / density, size.Height / density));
	}
}
