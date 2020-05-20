using Android.Views;
using AProgressBar = Android.Widget.ProgressBar;

namespace System.Maui.Platform
{
	public partial class ActivityIndicatorRenderer : AbstractViewRenderer<IActivityIndicator, AProgressBar>
	{
		protected override AProgressBar CreateView()
		{
			return new AProgressBar(Context) { Indeterminate = true };
		}

		public static void MapPropertyIsRunning(IViewRenderer renderer, IActivityIndicator activityIndicator)
		{
			if (!(renderer.NativeView is AProgressBar aProgressBar))
				return;

			aProgressBar.Visibility = activityIndicator.IsRunning ? ViewStates.Visible : ViewStates.Invisible;
		}

		public static void MapPropertyColor(IViewRenderer renderer, IActivityIndicator activityIndicator)
		{
			if (!(renderer.NativeView is AProgressBar aProgressBar))
				return;

			Color color = activityIndicator.Color;

			if (!color.IsDefault)
				aProgressBar.IndeterminateDrawable?.SetColorFilter(color.ToNative(), FilterMode.SrcIn);
			else
				aProgressBar.IndeterminateDrawable?.ClearColorFilter();
		}
	}
}