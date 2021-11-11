#nullable enable
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;

namespace Microsoft.Maui.Handlers
{
	public partial class ButtonHandler : ViewHandler<IButton, MauiButton>
	{
		static UI.Xaml.Thickness? DefaultPadding;
		static UI.Xaml.Media.Brush? DefaultForeground;
		static UI.Xaml.Media.Brush? DefaultBackground;

		PointerEventHandler? _pointerPressedHandler;
		PointerEventHandler? _pointerReleasedHandler;

		protected override MauiButton CreateNativeView() 
			=> new MauiButton();

		protected override void ConnectHandler(MauiButton nativeView)
		{
			_pointerPressedHandler = new PointerEventHandler(OnPointerPressed);
			_pointerReleasedHandler = new PointerEventHandler(OnPointerReleased);

			nativeView.Click += OnClick;
			nativeView.AddHandler(UI.Xaml.UIElement.PointerPressedEvent, _pointerPressedHandler, true);
			nativeView.AddHandler(UI.Xaml.UIElement.PointerReleasedEvent, _pointerReleasedHandler, true);

			SetupDefaults(nativeView);

			base.ConnectHandler(nativeView);
		}

		void SetupDefaults(MauiButton nativeView)
		{
			DefaultPadding = (UI.Xaml.Thickness)MauiWinUIApplication.Current.Resources["ButtonPadding"];
			DefaultForeground = (UI.Xaml.Media.Brush)MauiWinUIApplication.Current.Resources["ButtonForegroundThemeBrush"];
			DefaultBackground = (UI.Xaml.Media.Brush)MauiWinUIApplication.Current.Resources["ButtonBackgroundThemeBrush"];
		}

		protected override void DisconnectHandler(MauiButton nativeView)
		{
			nativeView.Click -= OnClick;
			nativeView.RemoveHandler(UI.Xaml.UIElement.PointerPressedEvent, _pointerPressedHandler);
			nativeView.RemoveHandler(UI.Xaml.UIElement.PointerReleasedEvent, _pointerReleasedHandler);

			_pointerPressedHandler = null;
			_pointerReleasedHandler = null;

			base.DisconnectHandler(nativeView);
		}

		// This is a Windows-specific mapping
		public static void MapBackground(IButtonHandler handler, IButton button)
		{
			handler.TypedNativeView?.UpdateBackground(button, DefaultBackground);
		}

		public static void MapBorderColor(IButtonHandler handler, IButton button)
		{
			handler.TypedNativeView?.UpdateBorderColor(button);
		}

		public static void MapBorderWidth(IButtonHandler handler, IButton button)
		{
			handler.TypedNativeView?.UpdateBorderWidth(button);
		}

		public static void MapCornerRadius(IButtonHandler handler, IButton button)
		{
			handler.TypedNativeView?.UpdateCornerRadius(button);
		}
		
		public static void MapText(IButtonHandler handler, IText button)
		{
			handler.TypedNativeView?.UpdateText(button);
		}

		public static void MapTextColor(IButtonHandler handler, ITextStyle button)
		{
			handler.TypedNativeView?.UpdateTextColor(button, DefaultForeground);
		}

		public static void MapCharacterSpacing(IButtonHandler handler, ITextStyle button)
		{
			handler.TypedNativeView?.UpdateCharacterSpacing(button.CharacterSpacing);
		}

		public static void MapFont(IButtonHandler handler, ITextStyle button)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();

			handler.TypedNativeView?.UpdateFont(button, fontManager);
		}

		public static void MapPadding(IButtonHandler handler, IButton button)
		{
			handler.TypedNativeView?.UpdatePadding(button, DefaultPadding);
		}

		public static void MapImageSource(IButtonHandler handler, IButton image) =>
			handler
				.ImageSourceLoader
				.UpdateImageSourceAsync()
				.FireAndForget(handler);

		void OnSetImageSource(ImageSource? obj)
		{
			NativeView.UpdateImageSource(VirtualView, obj);
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

		void OnPointerReleased(object sender, PointerRoutedEventArgs e)
		{
			VirtualView?.Released();
		}
	}
}