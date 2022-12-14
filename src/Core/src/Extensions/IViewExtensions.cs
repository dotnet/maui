namespace Microsoft.Maui.Extensions
{
	// TODO: Make this public on .NET 8
	internal static class IViewExtensions
	{
		internal static bool GetIsEnabled(this IView view)
		{
			bool parentIsEnabled = true;

			if (view.Parent is IView parentView)
			{
				parentIsEnabled = parentView.GetIsEnabled();
			}

			return parentIsEnabled && view.IsEnabled;
		}
	}
}