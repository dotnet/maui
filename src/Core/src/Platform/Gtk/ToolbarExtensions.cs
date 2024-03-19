namespace Microsoft.Maui.Platform;

public static class ToolbarExtensions
{
	public static void UpdateTitle(this MauiToolbar platformView, IToolbar toolbar)
	{
		platformView.Title = toolbar.Title ?? string.Empty;
		if (platformView.IsVisible)
			platformView.ShowAll();
	}
}