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

		static void MapTitle(IWindowHandler handler, Window window)
		{
			handler
				.PlatformView
				.UpdateTitle(window, window.GetCurrentlyPresentedMauiContext());
		}

		static void MapTitleBar(IWindowHandler handler, Window window)
		{
			handler
				.PlatformView
				.UpdateTitleBar(window, window.GetCurrentlyPresentedMauiContext());
		}

		Rect[] IWindow.TitleBarDragRectangles
		{
			get
			{
				return (this.Handler as IWindowHandler)?.PlatformView?
					.GetDefaultTitleBarDragRectangles(this.GetCurrentlyPresentedMauiContext());
			}
		}

		public IView TitleBar
		{
			get => (IView)GetValue(TitleBarProperty);
#pragma warning disable RS0036 // Annotate nullability of public types and members in the declared API
			set => SetValue(TitleBarProperty, value);
#pragma warning restore RS0036 // Annotate nullability of public types and members in the declared API
		}

		/// <summary>Bindable property for <see cref="TitleBar"/>.</summary>
		public static readonly BindableProperty TitleBarProperty = BindableProperty.Create(
			nameof(TitleBar), typeof(IView), typeof(Window), default(IView));
	}
}
