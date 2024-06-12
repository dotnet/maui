using System;
using System.Collections.Generic;
using UIKit;

namespace Microsoft.Maui.Platform;

public class StackNavigationManager
{
	IMauiContext MauiContext { get; }

	IReadOnlyList<IView> NavigationStack { get; set; } = [];
	IStackNavigationView? NavigationView { get; set; }
	UINavigationController? NavigationController { get; set; }

	public StackNavigationManager(IMauiContext mauiContext)
	{
		MauiContext = mauiContext;
	}

	public virtual void Connect(IStackNavigationView virtualView, UINavigationController navigationController)
	{
		NavigationView = virtualView;
		NavigationController = navigationController;
	}

	public virtual void Disconnect(IStackNavigationView virtualView, UINavigationController navigationController)
	{
		// TODO: anything else to clean up here
		NavigationView = null;
		NavigationController = null;
	}

	public virtual void RequestNavigation(NavigationRequest request)
	{
		if (request.NavigationStack.Count == 0)
		{
			throw new InvalidOperationException("NavigationStack cannot be empty.");
		}

		if (NavigationController == null)
		{
			throw new InvalidOperationException("NavigationController cannot be null.");
		}

		var currentNavStack = NavigationStack;
		var incomingNavStack = request.NavigationStack;
		var isInitialNavigation = currentNavStack.Count == 0 && incomingNavStack.Count == 1;

		//if (isInitialNavigation)
		//{
		//	SyncNativeStackWithNewStack(request);
		//	NavigationStack = new List<IView>(request.NavigationStack);
		//	NavigationView?.NavigationFinished(NavigationStack);
		//	return;
		//}


		if (isInitialNavigation || currentNavStack.Count < incomingNavStack.Count && incomingNavStack.Count - currentNavStack.Count == 1)
		{
			NavigationStack = new List<IView>(request.NavigationStack);
			NavigationController!.PushViewController(incomingNavStack[incomingNavStack.Count - 1].ToUIViewController(MauiContext), request.Animated);
			//NavigationView?.NavigationFinished(NavigationStack);
			return;
		}

		if (currentNavStack.Count > incomingNavStack.Count && currentNavStack.Count - incomingNavStack.Count == 1)
		{
			var currentTop = currentNavStack[currentNavStack.Count - 1];
			var incomingTop = incomingNavStack[incomingNavStack.Count - 1];

			if (currentTop != incomingTop && currentNavStack.Count - incomingNavStack.Count == 1)
			{
				var topViewController = NavigationController!.TopViewController; // currentTop.ToUIViewController(MauiContext);
				NavigationStack = new List<IView>(request.NavigationStack);
				topViewController.NavigationController?.PopViewController(request.Animated);
				//NavigationController!.PopViewController(request.Animated);
				return;
			}

			// otherwise, this changes a page/pages not on the top of the stack, so just sync the stacks
		}

		// The incoming and current stacks are the same length, so just sync the stacks
		NavigationStack = new List<IView>(request.NavigationStack);
		SyncNativeStackWithNewStack(request);
		//NavigationView?.NavigationFinished(NavigationStack);
		return;
	}

	void SyncNativeStackWithNewStack(NavigationRequest request)
	{
		var newStack = new List<UIViewController>();
		foreach (var page in request.NavigationStack)
		{
			UIViewController? viewController = null;

			if (page is IElement element)
			{
				var handler = page.Handler;
				viewController = page.ToUIViewController(MauiContext);

				if (handler is FlyoutViewHandler flyoutHandler)
				{
					System.Diagnostics.Trace.WriteLine($"Pushing a FlyoutPage onto a NavigationPage is not a supported UI pattern on iOS. " +
						"Please see https://developer.apple.com/documentation/uikit/uisplitviewcontroller for more details.");
				}
			}
			else
			{
				throw new InvalidOperationException("Page must be an IElement");
			}

			if (viewController == null)
			{
				throw new InvalidOperationException("ViewController cannot be null.");
			}

			//var wrapper = new ParentViewController(page.Handler as NavigationViewHandler 
			//	?? throw new InvalidOperationException($"Could not convert handler to {nameof(NavigationViewHandler)}"));

			//var containerViewController = new ParentViewController();
			//containerViewController.View!.AddSubview(viewController.View!);
			//containerViewController.AddChildViewController(viewController);
			//viewController.DidMoveToParentViewController(containerViewController);

			

			newStack.Add(viewController);
		}
		NavigationController!.SetViewControllers([.. newStack], request.Animated);
	}
}
