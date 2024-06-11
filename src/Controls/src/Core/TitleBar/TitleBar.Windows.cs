namespace Microsoft.Maui.Controls
{
	public partial class TitleBar
	{
		internal override void OnIsVisibleChanged(bool oldValue, bool newValue)
		{
			base.OnIsVisibleChanged(oldValue, newValue);

			var navRootManager = Handler?.MauiContext?.GetNavigationRootManager();
			navRootManager?.SetTitleBarVisibility(newValue);
		}
	}
}
