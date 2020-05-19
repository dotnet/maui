using System.Threading.Tasks;
using UIKit;

namespace System.Maui.Platform.iOS
{
	public class ShellItemTransition : IShellItemTransition
	{
		public Task Transition(IShellItemRenderer oldRenderer, IShellItemRenderer newRenderer)
		{
			TaskCompletionSource<bool> task = new TaskCompletionSource<bool>();
			var oldView = oldRenderer.ViewController.View;
			var newView = newRenderer.ViewController.View;
			newView.Alpha = 0;

			newView.Superview.InsertSubviewAbove(newView, oldView);

			UIView.Animate(0.5, 0, UIViewAnimationOptions.BeginFromCurrentState, () => newView.Alpha = 1, () => task.TrySetResult(true));

			return task.Task;
		}
	}
}