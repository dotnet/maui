using Android.Content.Res;
using Microsoft.Maui.Graphics;
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
			Color color = progress.ProgressColor;

			if (color == null)
			{
				(platformProgressBar.Indeterminate ? platformProgressBar.IndeterminateDrawable :
					platformProgressBar.ProgressDrawable)?.ClearColorFilter();
			}
			else
			{
				var tintList = ColorStateList.ValueOf(color.ToPlatform());

				if (platformProgressBar.Indeterminate)
					platformProgressBar.IndeterminateTintList = tintList;
				else
					platformProgressBar.ProgressTintList = tintList;
			}
		}

		// TODO: Material3 - make it public in .net 11
		internal static void UpdateProgress(this MaterialProgressBar materialProgressBar, IProgress progress)
		{
			materialProgressBar.Progress = (int)(progress.Progress * Maximum);
		}

		// TODO: Material3 - make it public in .net 11
		internal static void UpdateProgressColor(this MaterialProgressBar materialProgressBar, IProgress progress)
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