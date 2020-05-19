using System;
using System.Threading.Tasks;
using AppKit;

namespace System.Maui.Platform.MacOS
{
	internal static class NSViewControllerExtensions
	{
		public static Task<T> HandleAsyncAnimation<T>(this NSViewController container, NSViewController fromViewController,
			NSViewController toViewController, NSViewControllerTransitionOptions transitionOption,
			Action animationFinishedCallback, T result)
		{
			var tcs = new TaskCompletionSource<T>();

			container.TransitionFromViewController(fromViewController, toViewController, transitionOption, () =>
			{
				tcs.SetResult(result);
				animationFinishedCallback?.Invoke();
			});

			return tcs.Task;
		}
	}
}
