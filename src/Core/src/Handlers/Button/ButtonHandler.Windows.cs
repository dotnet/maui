#nullable enable
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace Microsoft.Maui.Handlers
{
	public partial class ButtonHandler : ViewHandler<IButton, MauiButton>
	{
		static UI.Xaml.Thickness? DefaultPadding;
		static UI.Xaml.Media.Brush? DefaultForeground;
		static UI.Xaml.Media.Brush? DefaultBackground;

		PointerEventHandler? _pointerPressedHandler;

		protected override MauiButton CreateNativeView() => new MauiButton();

		protected override void SetupDefaults(MauiButton nativeView)
		{
			DefaultPadding = (UI.Xaml.Thickness)MauiWinUIApplication.Current.Resources["ButtonPadding"];
			DefaultForeground = (UI.Xaml.Media.Brush)MauiWinUIApplication.Current.Resources["ButtonForegroundThemeBrush"];
			DefaultBackground = (UI.Xaml.Media.Brush)MauiWinUIApplication.Current.Resources["ButtonBackgroundThemeBrush"];

			base.SetupDefaults(nativeView);
		}

		protected override void ConnectHandler(MauiButton nativeView)
		{
			_pointerPressedHandler = new PointerEventHandler(OnPointerPressed);

			nativeView.Click += OnClick;
			nativeView.AddHandler(UI.Xaml.UIElement.PointerPressedEvent, _pointerPressedHandler, true);

			base.ConnectHandler(nativeView);
		}

		protected override void DisconnectHandler(MauiButton nativeView)
		{
			nativeView.Click -= OnClick;
			nativeView.RemoveHandler(UI.Xaml.UIElement.PointerPressedEvent, _pointerPressedHandler);

			_pointerPressedHandler = null;

			base.DisconnectHandler(nativeView);
		}

		// This is a Windows-specific mapping
		public static void MapBackgroundColor(ButtonHandler handler, IButton button)
		{
			handler.NativeView?.UpdateBackgroundColor(button, DefaultBackground);
		}

		// This is a Windows-specific mapping
		public static void MapCornerRadius(ButtonHandler handler, IButton button)
		{
			handler?.NativeView?.UpdateCornerRadius(button);
		}

		// This is a Windows-specific mapping
		[MissingMapper("Missing interface. Also missing Mapper.")]
		public static void MapLineBreakMode(ButtonHandler handler, IButton button)
		{
			handler.NativeView?.UpdateLineBreakMode();
		}

		public static void MapText(ButtonHandler handler, IButton button)
		{
			handler.NativeView?.UpdateText(button);
		}

		public static void MapTextColor(ButtonHandler handler, IButton button)
		{
			handler.NativeView?.UpdateTextColor(button, DefaultForeground);
		}

		[PortHandler("Has no effect due to internal bug in WinUI. Probably will work after it's fixed.")]
		[PortHandler("See: https://github.com/microsoft/microsoft-ui-xaml/issues/3490")]
		public static void MapCharacterSpacing(ButtonHandler handler, IButton button) 
		{
			handler.NativeView?.UpdateCharacterSpacing(button);
		}

		public static void MapFont(ButtonHandler handler, IButton button)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();

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