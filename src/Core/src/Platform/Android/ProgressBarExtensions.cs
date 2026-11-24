using Android.Graphics.Drawables;
using Google.Android.Material.ProgressIndicator;
using Microsoft.Maui.Graphics;
using ADrawable = Android.Graphics.Drawables.Drawable;
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

			// Use a color filter instead of a tint list: setting ProgressTintList replaces the tint
			// that the theme puts on the platform drawable, and setting it back to null does not
			// restore that tint. Clearing the filter brings back the theme color.
			var drawable = platformProgressBar.Indeterminate
				? platformProgressBar.IndeterminateDrawable
				: platformProgressBar.ProgressDrawable?.GetProgressLayer();

			if (color is null)
			{
				drawable?.ClearColorFilter();
			}
			else
			{
				drawable?.SetColorFilter(color, FilterMode.SrcIn);
			}
		}

		// Only color the progress layer, so the track keeps its color.
		static ADrawable GetProgressLayer(this ADrawable progressDrawable)
		{
			progressDrawable.Mutate();

			return progressDrawable is LayerDrawable layerDrawable
				? layerDrawable.FindDrawableByLayerId(Android.Resource.Id.Progress) ?? progressDrawable
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