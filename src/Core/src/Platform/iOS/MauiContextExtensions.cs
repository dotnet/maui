namespace Microsoft.Maui
{
	internal static class MauiContextExtensions
	{
		public static FlowDirection GetFlowDirection(this IMauiContext mauiContext)
		{
			var window = (mauiContext as IMauiWindowContext)?.Window;
			if (window == null)
				return FlowDirection.LeftToRight;

			return window.EffectiveUserInterfaceLayoutDirection.ToFlowDirection();
		}
	}
}
