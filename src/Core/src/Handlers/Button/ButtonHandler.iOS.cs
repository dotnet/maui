using System;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class ButtonHandler : ViewHandler<IButton, UIButton>
	{
		static readonly UIControlState[] ControlStates = {
			UIControlState.Normal, UIControlState.Highlighted, UIControlState.Disabled };

		// This appears to be the padding that Xcode has when "Default" content insets are used
		public readonly static Thickness DefaultPadding = new Thickness(12, 7);

		// Because we can't inherit from Button we use the container to handle
		// Life cycle events and things like monitoring focus changed
		public override bool NeedsContainer => true;

		Proxy _proxy;

		protected override UIButton CreatePlatformView()
		{
			var button = new UIButton(UIButtonType.System);
			
			foreach (UIControlState uiControlState in ControlStates)
			{
				button.SetTitleColor(UIButton.Appearance.TitleColor(uiControlState), uiControlState); // If new values are null, old values are preserved.
				button.SetTitleShadowColor(UIButton.Appearance.TitleShadowColor(uiControlState), uiControlState);
				button.SetBackgroundImage(UIButton.Appearance.BackgroundImageForState(uiControlState), uiControlState);
			}

			return button;
		}

		protected override void SetupContainer()
		{
			base.SetupContainer();
			if (ContainerView is WrapperView wrapperView)
			{
				wrapperView.CrossPlatformLayout = VirtualView as ICrossPlatformLayout;
			}
		}

		protected override void ConnectHandler(UIButton platformView)
		{
			_proxy = new Proxy(this);
			platformView.TouchUpInside += _proxy.OnButtonTouchUpInside;
			platformView.TouchUpOutside += _proxy.OnButtonTouchUpOutside;
			platformView.TouchDown += _proxy.OnButtonTouchDown;
			platformView.TouchCancel += _proxy.OnButtonTouchCancel;

			base.ConnectHandler(platformView);
		}

		protected override void DisconnectHandler(UIButton platformView)
		{
			platformView.TouchUpInside -= _proxy.OnButtonTouchUpInside;
			platformView.TouchUpOutside -= _proxy.OnButtonTouchUpOutside;
			platformView.TouchDown -= _proxy.OnButtonTouchDown;
			platformView.TouchCancel -= _proxy.OnButtonTouchCancel;

			_proxy = null;

			base.DisconnectHandler(platformView);
		}

#if MACCATALYST
		//TODO: make this public on NET8
		internal static void MapBackground(IButtonHandler handler, IButton button)
		{
			//If this is a Mac optimized interface
			if (OperatingSystem.IsIOSVersionAtLeast(15) && UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Mac)
			{
				var config = handler.PlatformView?.Configuration ?? UIButtonConfiguration.BorderedButtonConfiguration;
				if (button?.Background is Paint paint)
				{
					if (paint is SolidPaint solidPaint)
					{
						Color backgroundColor = solidPaint.Color;

						if (backgroundColor == null)
							config.BaseBackgroundColor = ColorExtensions.BackgroundColor;
						else
							config.BaseBackgroundColor = backgroundColor.ToPlatform();

					}
				}
				if (handler.PlatformView != null)
					handler.PlatformView.Configuration = config;
			}
			else
			{
				handler.PlatformView?.UpdateBackground(button);
			}
		}
#endif

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
			//If this is a Mac optimized interface
			if (OperatingSystem.IsIOSVersionAtLeast(15) && UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Mac)
			{
				var config = handler.PlatformView?.Configuration ?? UIButtonConfiguration.BorderedButtonConfiguration;
				if (button?.TextColor != null && handler.PlatformView != null)
					config.BaseForegroundColor = button?.TextColor.ToPlatform();
				if (handler.PlatformView != null)
					handler.PlatformView.Configuration = config;
			}
			else
			{
				handler.PlatformView?.UpdateTextColor(button);
			}
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

		public static void MapImageSource(IButtonHandler handler, IImage image) =>
			MapImageSourceAsync(handler, image).FireAndForget(handler);

		public static Task MapImageSourceAsync(IButtonHandler handler, IImage image)
		{
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

		class Proxy
		{
			WeakReference<ButtonHandler> _handler;

			public Proxy(ButtonHandler handler)
			{
				_handler = new WeakReference<ButtonHandler>(handler);
			}

			ButtonHandler? Handler => _handler.TryGetTarget(out var handler) ? handler : null;

			public void OnButtonTouchCancel(object? sender, EventArgs e)
			{
				Handler?.VirtualView?.Released();
			}

			public void OnButtonTouchUpInside(object? sender, EventArgs e)
			{
				Handler?.VirtualView?.Released();
				Handler?.VirtualView?.Clicked();
			}

			public void OnButtonTouchUpOutside(object? sender, EventArgs e)
			{
				Handler?.VirtualView?.Released();
			}

			public void OnButtonTouchDown(object? sender, EventArgs e)
			{
				Handler?.VirtualView?.Pressed();
			}
		}

		partial class ButtonImageSourcePartSetter
		{
			public override void SetImageSource(UIImage? platformImage)
			{
				if (Handler?.PlatformView is not UIButton button)
					return;

				platformImage = platformImage?.ImageWithRenderingMode(UIImageRenderingMode.AlwaysOriginal);

				button.SetImage(platformImage, UIControlState.Normal);
			}
		}
	}
}