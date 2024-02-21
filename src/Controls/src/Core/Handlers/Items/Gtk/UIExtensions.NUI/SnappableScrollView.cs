using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Gtk;

#pragma warning disable CS0067 // Event is never used

namespace Gtk.UIExtensions.NUI;

/// <summary>
/// A ScrollView that implemented snap points
/// </summary>
internal class SnappableScrollView : ScrollableBase
{
	delegate float UserAlphaFunctionDelegate(float progress);

	UserAlphaFunctionDelegate? _customScrollingAlphaFunctionDelegate;

	Func<float, float>? _scrollingAlpha;
	// AlphaFunction? _customScrollingAlphaFunction;
	// Animation? _snapAnimation;

	int _currentItemIndex = -1;

	public SnappableScrollView(CollectionView cv) : base(cv)
	{
		CollectionView = cv;

		_customScrollingAlphaFunctionDelegate = new UserAlphaFunctionDelegate(ScrollingAlpha);
		// _customScrollingAlphaFunction = new AlphaFunction(_customScrollingAlphaFunctionDelegate);

		ScrollDragStarted += OnDragStart;

		ScrollAnimationEnded += OnAnimationEnd;
		CollectionView.SizeAllocated += OnLayout;
	
	}


	public event EventHandler? SnapRequestFinished;

	public bool UseCostomScrolling { get; set; }

	public float MaximumVelocity { get; set; } = 8.5f;

	public float Friction { get; set; } = 0.015f;

	CollectionView CollectionView { get; }

	ICollectionViewLayoutManager LayoutManager => CollectionView.LayoutManager!;

	Rect ViewPort => CollectionView.ViewPort;

	double ViewPortStart => IsHorizontal ? ViewPort.X : ViewPort.Y;

	double ViewPortEnd => IsHorizontal ? ViewPort.Right : ViewPort.Bottom;

	double ViewPortSize => IsHorizontal ? ViewPort.Width : ViewPort.Height;

	bool IsHorizontal => ScrollingDirection == Direction.Horizontal;

	protected override void Decelerating(float velocity, Animation animation)
	{
		if (CollectionView.SnapPointsType == SnapPointsType.MandatorySingle)
		{
			// Only one item should be passed when scrolling by snap
			HandleMandatorySingle(velocity);
		}
		else
		{
			HandleNonMandatorySingle(velocity, animation);
		}
	}

	void CustomScrolling(float velocity, Animation animation)
	{
		float absVelocity = Math.Abs(velocity);
		float friction = Friction;

		float totalTime = Math.Abs(velocity / friction);
		float totalDistance = absVelocity * totalTime - (friction * (float)Math.Pow(totalTime, 2) / 2f);

		float currentPosition = (ScrollingDirection == Direction.Horizontal ? ContentContainer.PositionX() : ContentContainer.PositionY());
		float targetPosition = currentPosition + (velocity > 0 ? totalDistance : -totalDistance);
		float maximumScrollableSize = IsHorizontal ? ContentContainer.SizeWidth() - SizeWidth() : ContentContainer.SizeHeight() - SizeHeight();

		if (targetPosition > 0)
		{
			totalDistance -= targetPosition;
			targetPosition = 0;
		}
		else if (targetPosition < -maximumScrollableSize)
		{
			var overlapped = -maximumScrollableSize - targetPosition;
			totalDistance -= overlapped;
			targetPosition = -maximumScrollableSize;
		}

		if (totalDistance < 1)
		{
			base.Decelerating(0, animation);

			return;
		}

		_scrollingAlpha = (progress) =>
		{
			if (totalDistance == 0)
				return 1;

			var time = totalTime * progress;
			var distance = absVelocity * time - (friction * (float)Math.Pow(time, 2) / 2f);

			return Math.Min(distance / totalDistance, 1);
		};

		// animation.Duration = (int)totalTime;
		// animation.AnimateTo(ContentContainer, (ScrollingDirection == Direction.Horizontal) ? "PositionX" : "PositionY", targetPosition, _customScrollingAlphaFunction);
		// animation.Play();
	}


	float ScrollingAlpha(float progress)
	{
		return _scrollingAlpha?.Invoke(progress) ?? 1.0f;
	}

	void HandleNonMandatorySingle(float velocity, Animation animation)
	{
		if (Math.Abs(velocity) > MaximumVelocity)
		{
			velocity = MaximumVelocity * (velocity > 0 ? 1 : -1);
		}

		if (UseCostomScrolling)
		{
			CustomScrolling(velocity, animation);
		}
		else
		{
			if (CollectionView.SnapPointsType == SnapPointsType.None)
			{
				DecelerationRate = 0.998f;
			}
			else
			{
				// Adjust DecelerationRate to stop more quickly because it will be moved again by OnSnapRequest
				DecelerationRate = 0.992f;
			}

			base.Decelerating(velocity, animation);
		}
	}


	void HandleMandatorySingle(float velocity)
	{
		if (_currentItemIndex == -1)
			return;

		int currentItem = _currentItemIndex;

		if (Math.Abs(velocity) > 0.5)
		{
			if (velocity < 0)
			{
				currentItem = LayoutManager.NextRowItemIndex(currentItem);
			}
			else
			{
				currentItem = LayoutManager.PreviousRowItemIndex(currentItem);
			}
		}

		var itemBound = LayoutManager.GetItemBound(currentItem);
		var target = IsHorizontal ? itemBound.X : itemBound.Y;
		var itemSize = IsHorizontal ? itemBound.Width : itemBound.Height;
		var scrollingSize = IsHorizontal ? ContentContainer.SizeWidth() : ContentContainer.SizeHeight();

		// adjust align
		if (CollectionView.SnapPointsAlignment == SnapPointsAlignment.Center)
		{
			target -= (ViewPortSize - itemSize) / 2;
		}
		else if (CollectionView.SnapPointsAlignment == SnapPointsAlignment.End)
		{
			target -= (ViewPortSize - itemSize);
		}

		// adjust end of scroll area
		if (scrollingSize - target < ViewPortSize)
		{
			target = scrollingSize - ViewPortSize;
		}

		if (target < 0)
		{
			target = 0;
		}

		ScrollTo(target);
	}

	void OnDragStart(object? sender, ScrollEventArgs e)
	{
		if (CollectionView.SnapPointsType == SnapPointsType.MandatorySingle)
		{
			MarkCurrentItem();
		}
	}

	void MarkCurrentItem()
	{
		if (CollectionView.SnapPointsAlignment == SnapPointsAlignment.Start)
		{
			_currentItemIndex = CollectionView.LayoutManager!.GetVisibleItemIndex(CollectionView.ViewPort.X, CollectionView.ViewPort.Y);
			var bound = CollectionView.LayoutManager!.GetItemBound(_currentItemIndex);
			var padding = IsHorizontal ? bound.Width / 2 : bound.Height / 2;

			_currentItemIndex = CollectionView.LayoutManager!.GetVisibleItemIndex(
				(IsHorizontal ? padding : 0) + CollectionView.ViewPort.X,
				(IsHorizontal ? 0 : padding) + CollectionView.ViewPort.Y);
		}
		else if (CollectionView.SnapPointsAlignment == SnapPointsAlignment.Center)
		{
			_currentItemIndex = CollectionView.LayoutManager!.GetVisibleItemIndex(CollectionView.ViewPort.X + (CollectionView.ViewPort.Width / 2), CollectionView.ViewPort.Y + (CollectionView.ViewPort.Height / 2));
		}
		else
		{
			_currentItemIndex = CollectionView.LayoutManager!.GetVisibleItemIndex(CollectionView.ViewPort.X + CollectionView.ViewPort.Width, CollectionView.ViewPort.Y + CollectionView.ViewPort.Height);
			var bound = CollectionView.LayoutManager!.GetItemBound(_currentItemIndex);
			var padding = IsHorizontal ? bound.Width / 2 : bound.Height / 2;

			_currentItemIndex = CollectionView.LayoutManager!.GetVisibleItemIndex(
				(IsHorizontal ? -padding : 0) + CollectionView.ViewPort.X + CollectionView.ViewPort.Width,
				(IsHorizontal ? 0 : -padding) + CollectionView.ViewPort.Y + CollectionView.ViewPort.Height);
		}
	}

	void OnAnimationEnd(object? sender, ScrollEventArgs e)
	{
		OnSnapRequest();
	}

	void OnSnapRequest()
	{
		if (CollectionView.SnapPointsType == SnapPointsType.None)
			return;

		double target;

		if (CollectionView.SnapPointsAlignment == SnapPointsAlignment.Start)
		{
			var index = LayoutManager.GetVisibleItemIndex(ViewPort.X, ViewPort.Y);
			var bound = LayoutManager.GetItemBound(index);
			var itemSize = IsHorizontal ? bound.Width : bound.Height;
			var itemStart = IsHorizontal ? bound.X : bound.Y;

			if (ViewPortStart - itemStart > itemSize / 2)
			{
				index = LayoutManager.NextRowItemIndex(index);
			}

			bound = LayoutManager.GetItemBound(index);
			target = IsHorizontal ? bound.X : bound.Y;
		}
		else if (CollectionView.SnapPointsAlignment == SnapPointsAlignment.Center)
		{
			var index = LayoutManager.GetVisibleItemIndex(ViewPort.X + (ViewPort.Width / 2), ViewPort.Y + (ViewPort.Height / 2));
			var bound = LayoutManager.GetItemBound(index);
			var itemSize = IsHorizontal ? bound.Width : bound.Height;
			var itemStart = IsHorizontal ? bound.X : bound.Y;

			if (ViewPortStart + (ViewPortSize / 2) - (itemStart + itemSize / 2) > (itemSize / 2))
			{
				index = LayoutManager.NextRowItemIndex(index);
			}

			bound = LayoutManager.GetItemBound(index);
			itemSize = IsHorizontal ? bound.Width : bound.Height;
			target = IsHorizontal ? bound.X : bound.Y;
			target -= (ViewPortSize - itemSize) / 2;
		}
		else
		{
			var index = LayoutManager.GetVisibleItemIndex(ViewPort.Right, ViewPort.Bottom);
			var bound = LayoutManager.GetItemBound(index);
			var itemSize = IsHorizontal ? bound.Width : bound.Height;
			var itemEnd = IsHorizontal ? bound.Right : bound.Bottom;

			if (itemEnd - ViewPortEnd > itemSize / 2)
			{
				index = LayoutManager.PreviousRowItemIndex(index);
			}

			bound = LayoutManager.GetItemBound(index);
			itemSize = IsHorizontal ? bound.Width : bound.Height;

			target = IsHorizontal ? bound.X : bound.Y;
			target -= (ViewPortSize - itemSize);
		}

		ScrollTo(target);
	}

	void ScrollTo(double target)
	{
		// it is a ScrollTo api that do not raise ScrollAnimationStarted/Ended event
		var scrollingSize = IsHorizontal ? ContentContainer.SizeWidth() : ContentContainer.SizeHeight();

		if (scrollingSize - target < ViewPortSize)
		{
			target = scrollingSize - ViewPortSize;
		}

		if (target < 0)
		{
			target = 0;
		}

		// var animation = new Animation();
		// animation.Duration = 200;
		// animation.AnimateTo(ContentContainer, IsHorizontal ? "PositionX" : "PositionY", -(float)target);
		// animation.Finished += (s, e) => SnapRequestFinished?.Invoke(this, EventArgs.Empty);
		// animation.Play();
		// _snapAnimation = animation;
	}
}