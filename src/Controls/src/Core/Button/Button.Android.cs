#nullable disable
using Google.Android.Material.Button;
using Microsoft.Maui.Controls.Platform;

namespace Microsoft.Maui.Controls
{
	public partial class Button
	{
		MaterialButton _materialButton;

		private protected override void OnHandlerChangedCore()
		{
			base.OnHandlerChangedCore();

			if (Handler != null)
				Connect();
			else
				Disconnect();
		}

		void Connect()
		{
			if (Handler is ButtonHandler buttonHandler && buttonHandler.PlatformView is MaterialButton materialButton)
			{
				_materialButton = materialButton;
				_materialButton.LayoutChange += OnButtonLayoutChange;
			}
		}

		void Disconnect()
		{
			if (_materialButton != null)
			{
				_materialButton.LayoutChange -= OnButtonLayoutChange;
				_materialButton = null;
			}
		}

		public static void MapText(IButtonHandler handler, Button button)
		{
			handler.PlatformView?.UpdateText(button);
		}

		public static void MapText(ButtonHandler handler, Button button) =>
			MapText((IButtonHandler)handler, button);

		public static void MapLineBreakMode(IButtonHandler handler, Button button)
		{
			handler.PlatformView?.UpdateLineBreakMode(button);
		}

		void OnButtonLayoutChange(object sender, Android.Views.View.LayoutChangeEventArgs e)
		{
			Handler?.UpdateValue(nameof(ContentLayout));
		}
	}
}