using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls
{
	public partial class Window
	{
		internal UI.Xaml.Window NativeWindow =>
			(Handler?.NativeView as UI.Xaml.Window) ?? throw new InvalidOperationException("Window Handler should have a Window set.");

		public static void MapContent(WindowHandler handler, IWindow view)
		{
			if (view.Content is not Shell)
			{
				WindowHandler.MapContent(handler, view);
				return;
			}
			if (handler.NativeView.Content is UI.Xaml.Controls.Panel panel)
			{
				var nativeContent = view.Content.ToPlatform(handler.MauiContext!);
				panel.Children.Clear();
				panel.Children.Add(nativeContent);

				if (view.VisualDiagnosticsOverlay != null)
					view.VisualDiagnosticsOverlay.Initialize();

			}

		}
	}
}
