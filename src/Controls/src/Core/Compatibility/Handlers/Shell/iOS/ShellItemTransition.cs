#nullable disable
using System;
using System.Threading.Tasks;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Platform.Compatibility
{
	public class ShellItemTransition : IShellItemTransition
	{
		public Task Transition(IShellItemRenderer oldRenderer, IShellItemRenderer newRenderer)
		{
			TaskCompletionSource<bool> task = new TaskCompletionSource<bool>();
			var oldView = oldRenderer.ViewController.View;
			var newView = newRenderer.ViewController.View;

			oldView.Layer.RemoveAllAnimations();

			// On iOS 26+, setting newView.Alpha = 0 before the fade-in causes Liquid Glass
			// tab bar icons to composite while the parent view has zero alpha, resulting in
			// icons not rendering. Skip setting alpha to 0 so
			// the tab bar view stays at its default alpha=1 when iOS 26 composites its icons.
			if (!OperatingSystem.IsIOSVersionAtLeast(26))
			{
				newView.Alpha = 0;
			}

			oldView.Superview.InsertSubviewAbove(newView, oldView);

			UIView.Animate(0.5, 0, UIViewAnimationOptions.BeginFromCurrentState, () => newView.Alpha = 1, () =>
			{
				task.TrySetResult(true);
			});

			return task.Task;
		}
	}
}
