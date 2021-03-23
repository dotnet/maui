using System;
using Microsoft.Extensions.DependencyInjection;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class ButtonHandler : AbstractViewHandler<IButton, UIButton>
	{
		static readonly UIControlState[] ControlStates = { UIControlState.Normal, UIControlState.Highlighted, UIControlState.Disabled };

		static UIColor? ButtonTextColorDefaultDisabled;
		static UIColor? ButtonTextColorDefaultHighlighted;
		static UIColor? ButtonTextColorDefaultNormal;

		protected override UIButton CreateNativeView()
		{
			SetControlPropertiesFromProxy();
			return new UIButton(UIButtonType.System);
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

		protected override void SetupDefaults(UIButton nativeView)
		{
			ButtonTextColorDefaultNormal = nativeView.TitleColor(UIControlState.Normal);
			ButtonTextColorDefaultHighlighted = nativeView.TitleColor(UIControlState.Highlighted);
			ButtonTextColorDefaultDisabled = nativeView.TitleColor(UIControlState.Disabled);

			base.SetupDefaults(nativeView);
		}

		public static void MapText(ButtonHandler handler, IButton button)
		{
			handler.TypedNativeView?.UpdateText(button);
		}

		public static void MapTextColor(ButtonHandler handler, IButton button)
		{
			handler.TypedNativeView?.UpdateTextColor(button, ButtonTextColorDefaultNormal, ButtonTextColorDefaultHighlighted, ButtonTextColorDefaultDisabled);
		}
		public static void MapPadding(ButtonHandler handler, IButton button)
		{
			handler.TypedNativeView?.UpdatePadding(button);
		}

		public static void MapFont(ButtonHandler handler, IButton button)
		{
			_ = handler.Services ?? throw new InvalidOperationException($"{nameof(Services)} should have been set by base class.");

			var fontManager = handler.Services.GetRequiredService<IFontManager>();

			handler.TypedNativeView?.UpdateFont(button, fontManager);
		}

		void SetControlPropertiesFromProxy()
		{
			if (TypedNativeView == null)
				return;

			foreach (UIControlState uiControlState in ControlStates)
			{
				TypedNativeView.SetTitleColor(UIButton.Appearance.TitleColor(uiControlState), uiControlState); // If new values are null, old values are preserved.
				TypedNativeView.SetTitleShadowColor(UIButton.Appearance.TitleShadowColor(uiControlState), uiControlState);
				TypedNativeView.SetBackgroundImage(UIButton.Appearance.BackgroundImageForState(uiControlState), uiControlState);
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