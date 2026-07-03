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
			// tab bar icons to composite at zero opacity, resulting in missing tab icons.
			// We skip the initial alpha=0 on iOS 26+, which means there is no fade-in
			// animation on iOS 26+ — this is intentional to avoid the rendering issue.
			if (!(OperatingSystem.IsIOSVersionAtLeast(26) || OperatingSystem.IsMacCatalystVersionAtLeast(26)))
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
