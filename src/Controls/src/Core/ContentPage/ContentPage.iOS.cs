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
			Handler?.PlatformView is not MauiView mauiView ||
			!mauiView.TryGetSafeAreaForScrollDelegation(out var safeArea) ||
			SafeAreaScrollViewCoordinator.FindVerticalScrollContent(content) != content)
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
		Rect arrangedBounds,
		bool delegateTopSafeArea,
		double topSafeArea)
	{
		if (!delegateTopSafeArea || Content is not IView content)
		{
			_safeAreaScrollViewCoordinator?.Reset();
			return;
		}

		const double tolerance = 1;
		var contentFrame = content.Frame;
		if (contentFrame.Top < arrangedBounds.Top - tolerance ||
			contentFrame.Height < arrangedBounds.Height / 2)
		{
			_safeAreaScrollViewCoordinator?.Reset();
			return;
		}

		(_safeAreaScrollViewCoordinator ??= new()).TryDelegate(
			content,
			content,
			platformBounds,
			contentFrame.Top - platformBounds.Top,
			topSafeArea);
	}
}
