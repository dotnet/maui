using Tizen.UIExtensions.ElmSharp;
using EColor = ElmSharp.Color;
using EProgressBar = ElmSharp.ProgressBar;

namespace Microsoft.Maui.Handlers
{
	public partial class ActivityIndicatorHandler : ViewHandler<IActivityIndicator, EProgressBar>
	{
		protected virtual EColor DefaultColor => ThemeConstants.ProgressBar.ColorClass.Default;

		protected override EProgressBar CreatePlatformView()
		{
			var progressBar = new EProgressBar(NativeParent) { IsPulseMode = true }.SetSmallStyle();
			progressBar.Color = DefaultColor;
			return progressBar;
		}

		public static void MapIsRunning(IActivityIndicatorHandler handler, IActivityIndicator activityIndicator)
		{
			handler.PlatformView?.UpdateIsRunning(activityIndicator);
		}

		public static void MapColor(IActivityIndicatorHandler handler, IActivityIndicator activityIndicator)
		{
			handler.PlatformView?.UpdateColor(activityIndicator);
		}
	}
}