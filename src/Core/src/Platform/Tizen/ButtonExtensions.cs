using Tizen.UIExtensions.ElmSharp;

namespace Microsoft.Maui.Platform
{
	public static class ButtonExtensions
	{
		public static void UpdateTextColor(this Button platformButton, ITextStyle button)
		{
			platformButton.TextColor = button.TextColor.ToPlatform();
		}

		public static void UpdateText(this Button platformButton, IText button)
		{
			platformButton.Text = button.Text ?? "";
		}
	}
}
