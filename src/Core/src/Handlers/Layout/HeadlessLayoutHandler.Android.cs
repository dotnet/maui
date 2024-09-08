using System;
using System.Linq;

namespace Microsoft.Maui.Handlers;

internal partial class HeadlessLayoutHandler : IHeadlessLayoutHandler
{
	public void Add(IView view)
	{
		PlatformHandler.Add(view);
	}

	public void Remove(IView view)
	{
		var platformView = PlatformView;
		if (view is { Handler: HeadlessLayoutHandler headlessLayoutHandler })
		{
			var subviews = CollectPlatformViews(headlessLayoutHandler.VirtualView);
			foreach (var subview in subviews)
			{
				platformView.RemoveView(subview);
			}
			return;
		}

		platformView.RemoveView(view.ToPlatform());
	}

	public void Clear()
	{
		var platformView = PlatformView;
		var subviews = CollectPlatformViews(VirtualView);
		foreach (var subview in subviews)
		{
			platformView.RemoveView(subview);
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
			platformView.AddView(childPlatformView, targetIndex++);
		}
	}

	public void MoveSubviews(int targetIndex)
	{
		var platformView = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by set virtual view.");
		var virtualView = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by set virtual view.");

		var subviews = CollectPlatformViews(virtualView).ToArray();

		if (subviews.Length == 0 || platformView.IndexOfChild(subviews[0]) == targetIndex)
		{
			return;
		}

		foreach (var subview in subviews)
		{
			platformView.RemoveView(subview);
		}
		
		foreach (var subview in subviews)
		{
			platformView.AddView(subview, targetIndex++);
		}
	}
}