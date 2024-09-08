using System;
using System.Linq;
using PlatformView = UIKit.UIView;
using LayoutPlatformView = Microsoft.Maui.Platform.LayoutView;

namespace Microsoft.Maui.Handlers;

internal partial class HeadlessLayoutHandler : IHeadlessLayoutHandler
{
	public void Add(IView view)
	{
		PlatformHandler.Add(view);
	}

	public void Remove(IView view)
	{
		if (view is { Handler: HeadlessLayoutHandler headlessLayoutHandler })
		{
			var subviews = CollectPlatformViews(headlessLayoutHandler.VirtualView);
			foreach (var subview in subviews)
			{
				subview.RemoveFromSuperview();
			}
			return;
		}

		view.ToPlatform().RemoveFromSuperview();
	}

	public void Clear()
	{
		var subviews = CollectPlatformViews(VirtualView);
		foreach (var subview in subviews)
		{
			subview.RemoveFromSuperview();
		}
	}

	public void Insert(int index, IView view)
	{
		PlatformHandler.Insert(/* ignored */-1, view);
	}

	public void Update(int index, IView view)
	{
		PlatformHandler.Update(/* ignored */-1, view);
	}

	public void CreateSubviews(ref int targetIndex)
	{
		var mauiContext = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by set virtual view.");
		var platformView = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by set virtual view.");
		var virtualView = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by set virtual view.");

		foreach (var child in virtualView.OrderByZIndex())
		{
			var childHandler = ((IElement)child).ToHandler(mauiContext);

			if (childHandler is IHeadlessLayoutHandler headlessLayoutHandler)
			{
				headlessLayoutHandler.CreateSubviews(ref targetIndex);
				continue;
			}

			var childPlatformView = childHandler.ToPlatform();
			platformView.InsertSubview(childPlatformView, targetIndex++);
			
			if (child.FlowDirection == FlowDirection.MatchParent)
			{
				childPlatformView.UpdateFlowDirection(child);
			}
		}
	}

	public void MoveSubviews(int targetIndex)
	{
		var platformView = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by set virtual view.");
		var virtualView = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by set virtual view.");

		var subviews = CollectPlatformViews(virtualView).ToArray();

		if (subviews.Length == 0 || platformView.Subviews.IndexOf(subviews[0]) == targetIndex)
		{
			return;
		}

		foreach (var subview in subviews)
		{
			subview.RemoveFromSuperview();
		}
		
		foreach (var subview in subviews)
		{
			platformView.InsertSubview(subview, targetIndex++);
		}
	}
}