using System;
using System.Threading.Tasks;
using Foundation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Graphics;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class ButtonHandler : ViewHandler<IButton, UIButton>
	{
		static readonly UIControlState[] ControlStates = { UIControlState.Normal, UIControlState.Highlighted, UIControlState.Disabled };

		static UIColor? ButtonTextColorDefaultDisabled;
		static UIColor? ButtonTextColorDefaultHighlighted;
		static UIColor? ButtonTextColorDefaultNormal;

		protected override UIButton CreateNativeView()
		{
			var button = new UIButton(UIButtonType.System);
			SetControlPropertiesFromProxy(button);
			return button;
		}

		protected override void ConnectHandler(UIButton nativeView)
		{
			nativeView.TouchUpInside += OnButtonTouchUpInside;
			nativeView.TouchUpOutside += OnButtonTouchUpOutside;
			nativeView.TouchDown += OnButtonTouchDown;

			base.ConnectHandler(nativeView);
		}

		protected override void DisconnectHandler(UIButton nativeView)
		{
			nativeView.TouchUpInside -= OnButtonTouchUpInside;
			nativeView.TouchUpOutside -= OnButtonTouchUpOutside;
			nativeView.TouchDown -= OnButtonTouchDown;
			base.DisconnectHandler(nativeView);
		}

		void SetupDefaults(UIButton nativeView)
		{
			ButtonTextColorDefaultNormal = nativeView.TitleColor(UIControlState.Normal);
			ButtonTextColorDefaultHighlighted = nativeView.TitleColor(UIControlState.Highlighted);
			ButtonTextColorDefaultDisabled = nativeView.TitleColor(UIControlState.Disabled);
		}

		public static void MapText(IButtonHandler handler, IText button)
		{
			handler.TypedNativeView?.UpdateText(button);

			// Any text update requires that we update any attributed string formatting
			MapFormatting(handler, button);
		}

		public static void MapTextColor(IButtonHandler handler, ITextStyle button)
		{
			handler.TypedNativeView?.UpdateTextColor(button, ButtonTextColorDefaultNormal, ButtonTextColorDefaultHighlighted, ButtonTextColorDefaultDisabled);
		}

		public static void MapCharacterSpacing(IButtonHandler handler, ITextStyle button)
		{
			handler.TypedNativeView?.UpdateCharacterSpacing(button);
		}

		public static void MapPadding(IButtonHandler handler, IButton button)
		{
			handler.TypedNativeView?.UpdatePadding(button);
		}

		public static void MapFont(IButtonHandler handler, ITextStyle button)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();

			handler.TypedNativeView?.UpdateFont(button, fontManager);
		}

		public static void MapFormatting(IButtonHandler handler, IText button)
		{
			// Update all of the attributed text formatting properties
			handler.TypedNativeView?.UpdateCharacterSpacing(button);
		}

		void OnSetImageSource(UIImage? image)
		{
			if (image != null)
			{
				NativeView.SetImage(image.ImageWithRenderingMode(UIImageRenderingMode.AlwaysOriginal), UIControlState.Normal);
			}
			else
			{
				NativeView.SetImage(null, UIControlState.Normal);
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

		static void SetControlPropertiesFromProxy(UIButton nativeView)
		{
			foreach (UIControlState uiControlState in ControlStates)
			{
				nativeView.SetTitleColor(UIButton.Appearance.TitleColor(uiControlState), uiControlState); // If new values are null, old values are preserved.
				nativeView.SetTitleShadowColor(UIButton.Appearance.TitleShadowColor(uiControlState), uiControlState);
				nativeView.SetBackgroundImage(UIButton.Appearance.BackgroundImageForState(uiControlState), uiControlState);
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