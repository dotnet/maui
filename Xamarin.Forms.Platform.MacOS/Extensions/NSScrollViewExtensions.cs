using System.Threading.Tasks;
using AppKit;
using PointF = CoreGraphics.CGPoint;

namespace Xamarin.Forms.Platform.MacOS
{
	internal static class NSScrollViewExtensions
	{
		public static Task ScrollToPositionAsync(this NSScrollView scrollView, PointF point, bool animate,
			double duration = 0.5)
		{
			if (!animate)
			{
				var nsView = scrollView.DocumentView as NSView;
				nsView?.ScrollPoint(point);
				return Task.FromResult(true);
			}

			TaskCompletionSource<bool> source = new TaskCompletionSource<bool>();

			NSAnimationContext.BeginGrouping();

			NSAnimationContext.CurrentContext.CompletionHandler += () => { source.TrySetResult(true); };

			NSAnimationContext.CurrentContext.Duration = duration;

			var animator = scrollView.ContentView.Animator as NSView;

			animator?.SetBoundsOrigin(point);

			NSAnimationContext.EndGrouping();

			return source.Task;
		}
	}
}