using System;
using System.Threading.Tasks;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class ButtonHandler : ViewHandler<IButton, UIButton>
	{
		static readonly UIControlState[] ControlStates = { UIControlState.Normal, UIControlState.Highlighted, UIControlState.Disabled };

		static UIColor? ButtonTextColorDefaultDisabled;
		static UIColor? ButtonTextColorDefaultHighlighted;
		static UIColor? ButtonTextColorDefaultNormal;

		// This appears to be the padding that Xcode has when "Default" content insets are used
		public readonly static Thickness DefaultPadding = new Thickness(12, 7);

		void SetupDefaults(UIButton platformView)
		{
			ButtonTextColorDefaultNormal ??= platformView.TitleColor(UIControlState.Normal);
			ButtonTextColorDefaultHighlighted ??= platformView.TitleColor(UIControlState.Highlighted);
			ButtonTextColorDefaultDisabled ??= platformView.TitleColor(UIControlState.Disabled);
		}

		protected override UIButton CreatePlatformView()
		{
			var button = new UIButton(UIButtonType.System);
			SetControlPropertiesFromProxy(button);
			return button;
		}

		protected override void ConnectHandler(UIButton platformView)
		{
			SetupDefaults(platformView);

			platformView.TouchUpInside += OnButtonTouchUpInside;
			platformView.TouchUpOutside += OnButtonTouchUpOutside;
			platformView.TouchDown += OnButtonTouchDown;

			base.ConnectHandler(platformView);
		}

		protected override void DisconnectHandler(UIButton platformView)
		{
			platformView.TouchUpInside -= OnButtonTouchUpInside;
			platformView.TouchUpOutside -= OnButtonTouchUpOutside;
			platformView.TouchDown -= OnButtonTouchDown;
			base.DisconnectHandler(platformView);
		}

		public static void MapStrokeColor(IButtonHandler handler, IButtonStroke buttonStroke)
		{
			handler.PlatformView?.UpdateStrokeColor(buttonStroke);
		}

		public static void MapStrokeThickness(IButtonHandler handler, IButtonStroke buttonStroke)
		{
			handler.PlatformView?.UpdateStrokeThickness(buttonStroke);
		}

		public static void MapCornerRadius(IButtonHandler handler, IButtonStroke buttonStroke)
		{
			handler.PlatformView?.UpdateCornerRadius(buttonStroke);
		}

		public static void MapText(IButtonHandler handler, IText button)
		{
			handler.PlatformView?.UpdateText(button);

			// Any text update requires that we update any attributed string formatting
			MapFormatting(handler, button);
		}

		public static void MapTextColor(IButtonHandler handler, ITextStyle button)
		{
			handler.PlatformView?.UpdateTextColor(button, ButtonTextColorDefaultNormal, ButtonTextColorDefaultHighlighted, ButtonTextColorDefaultDisabled);
		}

		public static void MapCharacterSpacing(IButtonHandler handler, ITextStyle button)
		{
			handler.PlatformView?.UpdateCharacterSpacing(button);
		}

		public static void MapPadding(IButtonHandler handler, IButton button)
		{
			handler.PlatformView?.UpdatePadding(button, DefaultPadding);
		}

		public static void MapFont(IButtonHandler handler, ITextStyle button)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();

			handler.PlatformView?.UpdateFont(button, fontManager);
		}

		public static void MapFormatting(IButtonHandler handler, IText button)
		{
			// Update all of the attributed text formatting properties
			handler.PlatformView?.UpdateCharacterSpacing(button);
		}

		void OnSetImageSource(UIImage? image)
		{
			if (image != null)
			{
				PlatformView.SetImage(image.ImageWithRenderingMode(UIImageRenderingMode.AlwaysOriginal), UIControlState.Normal);
			}
			else
			{
				PlatformView.SetImage(null, UIControlState.Normal);
			}
		}

		public static void MapImageSource(IButtonHandler handler, IImageButton image) =>
			MapImageSourceAsync(handler, image).FireAndForget(handler);

		public static Task MapImageSourceAsync(IButtonHandler handler, IImageButton image)
		{
			if (image.Source == null)
			{
				return Task.CompletedTask;
			}

			return handler.ImageSourceLoader.UpdateImageSourceAsync();
		}

		static void SetControlPropertiesFromProxy(UIButton platformView)
		{
			foreach (UIControlState uiControlState in ControlStates)
			{
				platformView.SetTitleColor(UIButton.Appearance.TitleColor(uiControlState), uiControlState); // If new values are null, old values are preserved.
				platformView.SetTitleShadowColor(UIButton.Appearance.TitleShadowColor(uiControlState), uiControlState);
				platformView.SetBackgroundImage(UIButton.Appearance.BackgroundImageForState(uiControlState), uiControlState);
			}
		}

		void OnButtonTouchUpInside(object? sender, EventArgs e)
		{
			VirtualView?.Released();
			VirtualView?.Clicked();
		}

		void OnButtonTouchUpOutside(object? sender, EventArgs e)
		{
			VirtualView?.Released();
		}

		void OnButtonTouchDown(object? sender, EventArgs e)
		{
			VirtualView?.Pressed();
		}
	}
}