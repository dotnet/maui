using Android.Graphics.Drawables;
using Google.Android.Material.ProgressIndicator;
using Microsoft.Maui.Graphics;
using ADrawable = Android.Graphics.Drawables.Drawable;
using APorterDuff = Android.Graphics.PorterDuff;
using APorterDuffColorFilter = Android.Graphics.PorterDuffColorFilter;
using AProgressBar = Android.Widget.ProgressBar;

namespace Microsoft.Maui.Platform
{
	public static class ProgressBarExtensions
	{
		public const int Maximum = 10000;

		public static void UpdateProgress(this AProgressBar platformProgressBar, IProgress progress)
		{
			platformProgressBar.Progress = (int)(progress.Progress * Maximum);
		}

		public static void UpdateProgressColor(this AProgressBar platformProgressBar, IProgress progress)
		{
			if (platformProgressBar is LinearProgressIndicator materialProgressBar)
			{
				materialProgressBar.UpdateLinearProgressIndicatorColor(progress);
			}
			else
			{
				platformProgressBar.UpdateProgressBarColor(progress);
			}
		}

		static void UpdateProgressBarColor(this AProgressBar platformProgressBar, IProgress progress)
		{
			Color color = progress.ProgressColor;

			var drawable = platformProgressBar.Indeterminate
				? platformProgressBar.IndeterminateDrawable
				: platformProgressBar.ProgressDrawable?.GetProgressLayer();

			if (color is null)
			{
				drawable?.ClearColorFilter();
			}
			else
			{
				var mode = platformProgressBar.Indeterminate
					? platformProgressBar.IndeterminateTintMode
					: platformProgressBar.ProgressTintMode;

				drawable?.SetColorFilter(new APorterDuffColorFilter(color.ToPlatform(), mode ?? APorterDuff.Mode.SrcIn!));
			}
		}

		static ADrawable GetProgressLayer(this ADrawable progressDrawable)
		{
			progressDrawable.Mutate();

			return progressDrawable is LayerDrawable layerDrawable
				? layerDrawable.FindDrawableByLayerId(global::Android.Resource.Id.Progress) ?? progressDrawable
				: progressDrawable;
		}

		static void UpdateLinearProgressIndicatorColor(this LinearProgressIndicator materialProgressBar, IProgress progress)
		{
			Color color = progress.ProgressColor;

			if (color is null)
			{
				// Reset to theme default by passing empty array - Material3's setIndicatorColor() 
				// automatically resolves this to theme's colorPrimary when length == 0
				materialProgressBar.SetIndicatorColor([]);
			}
			else
			{
				var colorArray = new int[] { color.ToPlatform() };
				materialProgressBar.SetIndicatorColor(colorArray);
			}
		}
	}
}