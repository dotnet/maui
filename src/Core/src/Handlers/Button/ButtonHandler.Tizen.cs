using System;
using System.Threading.Tasks;
using Tizen.NUI.BaseComponents;
using Tizen.UIExtensions.NUI;

namespace Microsoft.Maui.Handlers
{
	public partial class ButtonHandler : ViewHandler<IButton, Button>
	{
		protected override Button CreatePlatformView() => new Button
		{
			Focusable = true,
		};

		protected override void ConnectHandler(Button platformView)
		{
			platformView.TouchEvent += OnTouch;
			platformView.Clicked += OnButtonClicked;
			base.ConnectHandler(platformView);
		}

		protected override void DisconnectHandler(Button platformView)
		{
			if (!platformView.HasBody())
				return;

			platformView.TouchEvent -= OnTouch;
			platformView.Clicked -= OnButtonClicked;
			base.DisconnectHandler(platformView);
		}

		public static void MapText(IButtonHandler handler, IText button)
		{
			handler.PlatformView?.UpdateText(button);
		}

		public static void MapTextColor(IButtonHandler handler, ITextStyle button)
		{
			handler.PlatformView?.UpdateTextColor(button);
		}

		public static void MapImageSource(IButtonHandler handler, IImage image) =>
			MapImageSourceAsync(handler, image).FireAndForget(handler);

		public static Task MapImageSourceAsync(IButtonHandler handler, IImage image)
		{
			if (image.Source == null)
			{
				return Task.CompletedTask;
			}
			return handler.ImageSourceLoader.UpdateImageSourceAsync();
		}

		public static void MapCharacterSpacing(IButtonHandler handler, ITextStyle button)
		{
			handler.PlatformView.UpdateCharacterSpacing(button);
		}

		public static void MapFont(IButtonHandler handler, ITextStyle button)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();
			handler.PlatformView.UpdateFont(button, fontManager);
		}

		[MissingMapper]
		public static void MapPadding(IButtonHandler handler, IButton button) { }

		public static void MapStrokeColor(IButtonHandler handler, IButtonStroke buttonStroke)
		{
			handler.PlatformView.UpdateStrokeColor(buttonStroke);
		}

		public static void MapStrokeThickness(IButtonHandler handler, IButtonStroke buttonStroke)
		{
			handler.PlatformView.UpdateStrokeThickness(buttonStroke);
		}

		public static void MapCornerRadius(IButtonHandler handler, IButtonStroke buttonStroke)
		{
			handler.PlatformView.UpdateCornerRadius(buttonStroke);
		}

		bool OnTouch(object source, View.TouchEventArgs e)
		{
			var state = e.Touch.GetState(0);

			if (state == Tizen.NUI.PointStateType.Down)
			{
				OnButtonPressed(source, e);
			}
			else if (state == Tizen.NUI.PointStateType.Up)
			{
				OnButtonReleased(source, e);
			}
			return false;
		}

		void OnButtonClicked(object? sender, EventArgs e)
		{
			VirtualView?.Clicked();
		}

		void OnButtonReleased(object? sender, EventArgs e)
		{
			VirtualView?.Released();
		}

		void OnButtonPressed(object? sender, EventArgs e)
		{
			VirtualView?.Pressed();
		}

		partial class ButtonImageSourcePartSetter
		{
			public override void SetImageSource(MauiImageSource? platformImage)
			{
				if (Handler?.PlatformView is not Button button)
					return;

				if (platformImage is null)
					return;

				button.Icon.ResourceUrl = platformImage?.ResourceUrl;
			}
		}
	}
}
