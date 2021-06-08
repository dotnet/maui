using System;
using Tizen.UIExtensions.ElmSharp;

namespace Microsoft.Maui.Handlers
{
	public partial class ButtonHandler : ViewHandler<IButton, Button>
	{
		protected override Button CreateNativeView() => new Button(NativeParent);

		protected override void ConnectHandler(Button nativeView)
		{
			nativeView.Released += OnButtonClicked;
			nativeView.Clicked += OnButtonReleased;
			nativeView.Pressed += OnButtonPressed;

			base.ConnectHandler(nativeView);
		}

		protected override void DisconnectHandler(Button nativeView)
		{
			nativeView.Released -= OnButtonClicked;
			nativeView.Clicked -= OnButtonReleased;
			nativeView.Pressed -= OnButtonPressed;

			base.DisconnectHandler(nativeView);
		}

		public static void MapText(ButtonHandler handler, IButton button)
		{
			handler.NativeView?.UpdateText(button);
		}

		public static void MapTextColor(ButtonHandler handler, IButton button)
		{
			handler.NativeView?.UpdateTextColor(button);
		}

		[MissingMapper]
		public static void MapCharacterSpacing(ButtonHandler handler, IButton button) { }

		[MissingMapper]
		public static void MapFont(ButtonHandler handler, IButton button) { }

		[MissingMapper]
		public static void MapPadding(ButtonHandler handler, IButton button) { }

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
	}
}