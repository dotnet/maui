namespace Microsoft.Maui.Controls.Platform
{
	internal static class ToolbarExtensions
	{
		public static void UpdateIsVisible(this MauiToolbar platformToolbar, Toolbar toolbar)
		{
			platformToolbar.Visible = toolbar.IsVisible;
		}

		public static void UpdateBackButtonVisibility(this MauiToolbar platformToolbar, Toolbar toolbar)
		{
			platformToolbar.BackButton.Visible = toolbar.BackButtonVisible;
		}
	}
}