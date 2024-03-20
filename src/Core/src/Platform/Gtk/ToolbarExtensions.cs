namespace Microsoft.Maui.Platform;

public static class ToolbarExtensions
{
	public static void UpdateTitle(this MauiToolbar platformView, IToolbar toolbar)
	{
		platformView.Title = toolbar.Title ?? string.Empty;

	}
	
	public static void UpdateIsVisible(this MauiToolbar platformView, IToolbar toolbar)
	{
		var wasVisible = platformView.Visible;
		platformView.Visible = toolbar.IsVisible;
		if (platformView.IsVisible)
			platformView.ShowAll();
	}

	public static void UpdateBackButtonVisibility(this MauiToolbar platformView, IToolbar toolbar)
	{
		platformView.BackButton.Visible = toolbar.BackButtonVisible;
	}
}