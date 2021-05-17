using Tizen.UIExtensions.ElmSharp;
using EColor = ElmSharp.Color;
using EProgressBar = ElmSharp.ProgressBar;

namespace Microsoft.Maui.Handlers
{
	public partial class ActivityIndicatorHandler : ViewHandler<IActivityIndicator, EProgressBar>
	{
		protected virtual EColor DefaultColor => ThemeConstants.ProgressBar.ColorClass.Default;

		protected override EProgressBar CreateNativeView()
		{
			var progressBar = new EProgressBar(NativeParent) { IsPulseMode = true }.SetSmallStyle();
			progressBar.Color = DefaultColor;
			return progressBar;
		}

		public static void MapIsRunning(ActivityIndicatorHandler handler, IActivityIndicator activityIndicator)
		{
			handler.NativeView?.UpdateIsRunning(activityIndicator);
		}

		public static void MapColor(ActivityIndicatorHandler handler, IActivityIndicator activityIndicator)
		{
			handler.NativeView?.UpdateColor(activityIndicator);
		}
	}
}