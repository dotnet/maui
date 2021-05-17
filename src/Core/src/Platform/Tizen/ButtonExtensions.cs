using Tizen.UIExtensions.ElmSharp;

namespace Microsoft.Maui
{
	public static class ButtonExtensions
	{
		public static void UpdateTextColor(this Button nativeButton, IButton button)
		{
			nativeButton.TextColor = button.TextColor.ToNative();
		}

		public static void UpdateText(this Button nativeButton, IButton button)
		{
			nativeButton.Text = button.Text ?? "";
		}
	}
}
