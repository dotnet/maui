using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace Microsoft.Maui.Handlers
{
	public partial class ButtonHandler : ViewHandler<IButton, Button>
	{
		static UI.Xaml.Thickness? DefaultPadding;

		PointerEventHandler? _pointerPressedHandler;

		protected override Button CreateNativeView() => new Button();

		protected override void SetupDefaults(Button nativeView)
		{
			DefaultPadding = (UI.Xaml.Thickness)MauiWinUIApplication.Current.Resources["ButtonPadding"];

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

		public static void MapBackground(ButtonHandler handler, IButton button) =>
			handler.TypedNativeView?.UpdateBackground(button);

		public static void MapText(ButtonHandler handler, IButton button) =>
			handler.TypedNativeView?.UpdateText(button);

		public static void MapTextColor(ButtonHandler handler, IButton button) =>
			handler.TypedNativeView?.UpdateTextColor(button);

		[MissingMapper]
		public static void MapCharacterSpacing(ButtonHandler handler, IButton button) { }

		public static void MapFont(ButtonHandler handler, IButton button)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();

			handler.NativeView?.UpdateFont(button, fontManager);
		}

		public static void MapPadding(ButtonHandler handler, IButton button) =>
			handler.TypedNativeView?.UpdatePadding(button, DefaultPadding);

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