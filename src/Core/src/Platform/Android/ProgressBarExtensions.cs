using Android.Content.Res;
using Android.OS;
using AProgressBar = Android.Widget.ProgressBar;

namespace Microsoft.Maui
{
	public static class ProgressBarExtensions
	{
		public const int Maximum = 10000;

		public static void UpdateProgress(this AProgressBar nativeProgressBar, IProgress progress)
		{
			nativeProgressBar.Progress = (int)(progress.Progress * Maximum);
		}
	}
}