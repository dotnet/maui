using System.Threading.Tasks;
using Xamarin.Forms.Platform;

namespace Xamarin.Forms
{
	[RenderWith(typeof(_ProgressBarRenderer))]
	public class ProgressBar : View
	{
		public static readonly BindableProperty ProgressProperty = BindableProperty.Create("Progress", typeof(double), typeof(ProgressBar), 0d, coerceValue: (bo, v) => ((double)v).Clamp(0, 1));

		public double Progress
		{
			get { return (double)GetValue(ProgressProperty); }
			set { SetValue(ProgressProperty, value); }
		}

		public Task<bool> ProgressTo(double value, uint length, Easing easing)
		{
			var tcs = new TaskCompletionSource<bool>();

			this.Animate("Progress", d => Progress = d, Progress, value, length: length, easing: easing, finished: (d, finished) => tcs.SetResult(finished));

			return tcs.Task;
		}
	}
}