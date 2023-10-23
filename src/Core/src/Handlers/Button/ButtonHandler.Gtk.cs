using System;
using Gtk;
using Microsoft.Maui.Graphics.Platform.Gtk;

namespace Microsoft.Maui.Handlers
{

	// https://docs.gtk.org/gtk3/class.Button.html

	public partial class ButtonHandler : ViewHandler<IButton, Button>
	{

		protected override Button CreatePlatformView()
		{
			return Button.NewWithLabel(string.Empty);
		}

		protected override void ConnectHandler(Button nativeView)
		{
			nativeView.Clicked += OnButtonClicked;
			nativeView.ButtonPressEvent += OnButtonPressEvent;
			nativeView.ButtonReleaseEvent += OnButtonReleaseEvent;
		}

		protected override void DisconnectHandler(Button nativeView)
		{
			nativeView.Clicked -= OnButtonClicked;
			nativeView.ButtonPressEvent -= OnButtonPressEvent;
			nativeView.ButtonReleaseEvent -= OnButtonReleaseEvent;
		}

		public static void MapText(IButtonHandler handler, ITextButton button)
		{
			handler.PlatformView?.UpdateText(button);
		}

		public static void MapTextColor(IButtonHandler handler, ITextStyle button)
		{
			handler.PlatformView?.UpdateTextColor(button.TextColor);
		}

		public static void MapCharacterSpacing(IButtonHandler handler, ITextStyle button)
		{
			if (handler.PlatformView.Child is Label nativeView)
			{
				nativeView.Attributes = nativeView.Attributes.AttrListFor(button.CharacterSpacing);
			}
		}

		[MissingMapper]
		public static void MapImageSource(IButtonHandler handler, IImage image) { }

		[MissingMapper]
		public static void MapFont(IButtonHandler handler, IButton button) { }

		public static void MapPadding(IButtonHandler handler, IButton button)
		{
			handler.PlatformView.WithPadding(button.Padding);
		}

		[MissingMapper]
		public static void MapStrokeColor(IButtonHandler handler, IButtonStroke buttonStroke) { }

		[MissingMapper]
		public static void MapStrokeThickness(IButtonHandler handler, IButtonStroke buttonStroke) { }

		[MissingMapper]
		public static void MapCornerRadius(IButtonHandler handler, IButtonStroke buttonStroke) { }

		void OnButtonPressEvent(object? o, ButtonPressEventArgs args)
		{
			InvokeEvent(() => VirtualView?.Pressed());
		}

		void OnButtonReleaseEvent(object? o, ButtonReleaseEventArgs args)
		{
			InvokeEvent(() => VirtualView?.Released());
		}

		void OnButtonClicked(object? sender, EventArgs e)
		{
			InvokeEvent(() => VirtualView?.Clicked());
		}

		void OnSetImageSource(object? obj) { }
	}

}