using Tizen.UIExtensions.ElmSharp;

namespace Microsoft.Maui.Platform
{
	public static class MauiIndicatorViewExtensions
	{
		public static void UpdateIndicatorCount(this IndicatorView platformView, IIndicatorView indicator)
		{
			platformView.ClearIndex();
			platformView.AppendIndex(indicator.Count);
			platformView.Update(0);
			platformView.UpdatePosition(indicator);
		}

		public static void UpdatePosition(this IndicatorView platformView, IIndicatorView indicator)
		{
			platformView.UpdateSelectedIndex(indicator.Position);
		}
	}
}
