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

		public static void MapText(IButtonHandler handler, IButton button)
		{
			handler.PlatformView?.UpdateText(button);
		}

		public static void MapTextColor(IButtonHandler handler, IButton button)
		{
			handler.PlatformView?.UpdateTextColor(button.TextColor);
		}

		public static void MapCharacterSpacing(IButtonHandler handler, IButton button)
		{
			if (handler.PlatformView.Child is Label nativeView)
			{
				nativeView.Attributes = nativeView.Attributes.AttrListFor(button.CharacterSpacing);
			}
		}

		[MissingMapper]
		public static void MapImageSource(IButtonHandler handler, IImage image) { }

		public static void MapFont(IButtonHandler handler, IButton button)
		{
			handler.MapFont(button);
		}

		public static void MapPadding(IButtonHandler handler, IButton button)
		{
			handler.PlatformView.WithPadding(button.Padding);
		}

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

	}

}