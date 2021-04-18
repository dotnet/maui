using System;
using Gtk;

namespace Microsoft.Maui.Handlers
{
	public partial class ButtonHandler : ViewHandler<IButton, Button>
	{
		protected override Button CreateNativeView()
		{
			return new Button();
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

		public static void MapText(ButtonHandler handler, IButton button)
		{
			handler.NativeView?.UpdateText(button);
		}

		[MissingMapper]
		public static void MapTextColor(ButtonHandler handler, IButton button) { }

		[MissingMapper]
		public static void MapCharacterSpacing(ButtonHandler handler, IButton button) { }

		[MissingMapper]
		public static void MapFont(ButtonHandler handler, IButton button) { }

		[MissingMapper]
		public static void MapPadding(ButtonHandler handler, IButton button) { }

		void OnButtonPressEvent(object? o, ButtonPressEventArgs args)
		{
			VirtualView?.Pressed();
		}

		void OnButtonReleaseEvent(object? o, ButtonReleaseEventArgs args)
		{
			VirtualView?.Released();
		}

		void OnButtonClicked(object? sender, EventArgs e)
		{
			VirtualView?.Clicked();
		}
	}
}