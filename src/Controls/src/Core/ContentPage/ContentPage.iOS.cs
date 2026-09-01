#nullable disable
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;

namespace Microsoft.Maui.Controls;

public partial class ContentPage
{
	SafeAreaScrollViewCoordinator _safeAreaScrollViewCoordinator;

	private protected override void OnHandlerChangingCore(HandlerChangingEventArgs args)
	{
		if (args.OldHandler is not null)
			_safeAreaScrollViewCoordinator?.Reset();

		base.OnHandlerChangingCore(args);
	}

	partial void AdjustCrossPlatformArrangeBounds(
		ref Rect bounds,
		ref bool delegateTopSafeArea,
		ref double topSafeArea)
	{
		if (Content is not IView content ||
			SafeAreaScrollViewCoordinator.FindVerticalScrollContent(content) != content ||
			Handler?.PlatformView is not MauiView mauiView ||
			!mauiView.TryGetSafeAreaForScrollDelegation(out var safeArea))
		{
			return;
		}

		bounds = new Rect(
			bounds.X + safeArea.Left,
			bounds.Y + safeArea.Top,
			bounds.Width - safeArea.Left - safeArea.Right,
			bounds.Height - safeArea.Top - safeArea.Bottom);
		delegateTopSafeArea = true;
		topSafeArea = safeArea.Top;
	}

	partial void ApplyCrossPlatformArrangeSafeArea(
		Rect platformBounds,
		bool delegateTopSafeArea,
		double topSafeArea)
	{
		if (!delegateTopSafeArea || Content is not IView content)
		{
			_safeAreaScrollViewCoordinator?.Reset();
			return;
		}

		(_safeAreaScrollViewCoordinator ??= new()).TryDelegate(
			content,
			content,
			platformBounds,
			topSafeArea);
	}
}
