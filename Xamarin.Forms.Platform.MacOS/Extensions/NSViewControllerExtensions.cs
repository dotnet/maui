using System;
using System.Threading.Tasks;
using AppKit;

namespace Xamarin.Forms.Platform.MacOS
{
	internal static class NSViewControllerExtensions
	{
		public static Task<T> HandleAsyncAnimation<T>(this NSViewController container, NSViewController fromViewController,
			NSViewController toViewController, NSViewControllerTransitionOptions transitonOption,
			Action animationFinishedCallback, T result)
		{
			var tcs = new TaskCompletionSource<T>();

			container.TransitionFromViewController(fromViewController, toViewController, transitonOption, () =>
			{
				tcs.SetResult(result);
				animationFinishedCallback?.Invoke();
			});

			return tcs.Task;
		}
	}
}