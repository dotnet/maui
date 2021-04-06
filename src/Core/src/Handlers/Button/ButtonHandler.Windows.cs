using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace Microsoft.Maui.Handlers
{
	public partial class ButtonHandler : ViewHandler<IButton, Button>
	{
		static UI.Xaml.Thickness? DefaultPadding;
		static UI.Xaml.Media.Brush? DefaultForeground;
		static UI.Xaml.Media.Brush? DefaultBackground;

		PointerEventHandler? _pointerPressedHandler;

		protected override Button CreateNativeView() => new Button();

		protected override void SetupDefaults(Button nativeView)
		{
			DefaultPadding = (UI.Xaml.Thickness)MauiWinUIApplication.Current.Resources["ButtonPadding"];
			DefaultForeground = (UI.Xaml.Media.Brush)MauiWinUIApplication.Current.Resources["ButtonForegroundThemeBrush"];
			DefaultBackground = (UI.Xaml.Media.Brush)MauiWinUIApplication.Current.Resources["ButtonBackgroundThemeBrush"];

			base.SetupDefaults(nativeView);
		}

		protected override void ConnectHandler(Button nativeView)
		{
			_pointerPressedHandler = new PointerEventHandler(OnPointerPressed);

			nativeView.Click += OnClick;
			nativeView.AddHandler(UI.Xaml.UIElement.PointerPressedEvent, _pointerPressedHandler, true);

			base.ConnectHandler(nativeView);
		}

		protected override void DisconnectHandler(Button nativeView)
		{
			nativeView.Click -= OnClick;
			nativeView.RemoveHandler(UI.Xaml.UIElement.PointerPressedEvent, _pointerPressedHandler);

			_pointerPressedHandler = null;

			base.DisconnectHandler(nativeView);
		}

		public static void MapBackgroundColor(ButtonHandler handler, IButton button)
		{
			handler.NativeView?.UpdateBackgroundColor(button, DefaultBackground);
		}

		public static void MapText(ButtonHandler handler, IButton button)
		{
			handler.NativeView?.UpdateText(button);
		}

		public static void MapTextColor(ButtonHandler handler, IButton button)
		{
			handler.NativeView?.UpdateTextColor(button, DefaultForeground);
		}

		public static void MapCharacterSpacing(ButtonHandler handler, IButton button) { }

		public static void MapFont(ButtonHandler handler, IButton button)
		{
			_ = handler.Services ?? throw new InvalidOperationException($"{nameof(Services)} should have been set by base class.");

			var fontManager = handler.Services.GetRequiredService<IFontManager>();

			handler.NativeView?.UpdateFont(button, fontManager);
		}

		public static void MapPadding(ButtonHandler handler, IButton button)
		{
			handler.NativeView?.UpdatePadding(button, DefaultPadding);
		}

		void OnClick(object sender, UI.Xaml.RoutedEventArgs e)
		{
			VirtualView?.Clicked();
			VirtualView?.Released();
		}

		void OnPointerPressed(object sender, PointerRoutedEventArgs e)
		{
			VirtualView?.Pressed();
		}
	}
}