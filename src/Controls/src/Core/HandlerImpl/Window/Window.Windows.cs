#nullable disable
using System;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Controls
{
	public partial class Window
	{
		Rect[] _titleBarDragRectangles;
		internal UI.Xaml.Window NativeWindow =>
			(Handler?.PlatformView as UI.Xaml.Window) ?? throw new InvalidOperationException("Window Handler should have a Window set.");

		void UpdateTitleBarDragRectangles(Rect[] titleBarDragRectangles)
		{
			_titleBarDragRectangles = titleBarDragRectangles;
			Handler?.UpdateValue(nameof(IWindow.TitleBarDragRectangles));
		}

		static void MapWindowTitle(IWindowHandler handler, IWindow window)
		{
			if (window is Window controlsWindow)
			{
				handler
					.PlatformView
					.UpdateTitle(window, controlsWindow.GetCurrentlyPresentedMauiContext());
			}

			WindowHandler.MapTitle(handler, window);
		}

		Rect[] IWindow.TitleBarDragRectangles
		{
			get
			{
				return (this.Handler as IWindowHandler)?.PlatformView?
					.GetDefaultTitleBarDragRectangles(this.GetCurrentlyPresentedMauiContext());
			}
		}
	}
}
