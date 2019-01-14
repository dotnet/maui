using System;
using CoreGraphics;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	internal class SnapHelpers
	{
		public static CGPoint AdjustContentOffset(CGPoint proposedContentOffset, CGRect itemFrame,
			CGRect viewport, SnapPointsAlignment alignment, UICollectionViewScrollDirection scrollDirection)
		{
			var offset = GetViewportOffset(itemFrame, viewport, alignment, scrollDirection);
			return new CGPoint(proposedContentOffset.X - offset.X, proposedContentOffset.Y - offset.Y);
		}

		public static CGPoint FindAlignmentTarget(SnapPointsAlignment snapPointsAlignment,
			CGPoint contentOffset, UICollectionView collectionView, UICollectionViewScrollDirection scrollDirection)
		{
			var inset = collectionView.ContentInset;
			var bounds = collectionView.Bounds;

			switch (scrollDirection)
			{
				case UICollectionViewScrollDirection.Vertical:
					var y = FindAlignmentTarget(snapPointsAlignment, contentOffset.Y, inset.Top,
						contentOffset.Y + bounds.Height, inset.Bottom);
					return new CGPoint(contentOffset.X, y);
				case UICollectionViewScrollDirection.Horizontal:
					var x = FindAlignmentTarget(snapPointsAlignment, contentOffset.X, inset.Left,
						contentOffset.X + bounds.Width, inset.Right);
					return new CGPoint(x, contentOffset.Y);
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public static UICollectionViewLayoutAttributes FindBestSnapCandidate(UICollectionViewLayoutAttributes[] items,
			CGRect viewport, CGPoint alignmentTarget)
		{
			UICollectionViewLayoutAttributes bestCandidate = null;

			foreach (UICollectionViewLayoutAttributes item in items)
			{
				if (!IsAtLeastHalfVisible(item, viewport))
				{
					continue;
				}

				bestCandidate = bestCandidate == null ? item : Nearer(bestCandidate, item, alignmentTarget);
			}

			return bestCandidate;
		}

		static nfloat Area(CGRect rect)
		{
			return rect.Height * rect.Width;
		}

		static CGPoint Center(CGRect rect)
		{
			return new CGPoint(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
		}

		static nfloat DistanceSquared(CGRect rect, CGPoint target)
		{
			var rectCenter = Center(rect);

			return (target.X - rectCenter.X) * (target.X - rectCenter.X) +
					(target.Y - rectCenter.Y) * (target.Y - rectCenter.Y);
		}

		static int Clamp(int n, int min, int max)
		{
			if (n < min)
			{
				return min;
			}

			if (n > max)
			{
				return max;
			}

			return n;
		}

		static nfloat FindAlignmentTarget(SnapPointsAlignment snapPointsAlignment, nfloat start, nfloat startInset,
			nfloat end, nfloat endInset)
		{
			switch (snapPointsAlignment)
			{
				case SnapPointsAlignment.Center:
					var viewPortStart = start + startInset;
					var viewPortEnd = end - endInset;
					var viewPortSize = viewPortEnd - viewPortStart;

					return viewPortStart + (viewPortSize / 2);

				case SnapPointsAlignment.End:
					return end - endInset;

				case SnapPointsAlignment.Start:
				default:
					return start + startInset;
			}
		}

		static CGPoint GetViewportOffset(CGRect itemFrame, CGRect viewport, SnapPointsAlignment snapPointsAlignment,
			UICollectionViewScrollDirection scrollDirection)
		{
			if (scrollDirection == UICollectionViewScrollDirection.Horizontal)
			{
				if (snapPointsAlignment == SnapPointsAlignment.Start)
				{
					return new CGPoint(viewport.Left - itemFrame.Left, 0);
				}

				if (snapPointsAlignment == SnapPointsAlignment.End)
				{
					return new CGPoint(viewport.Right - itemFrame.Right, 0);
				}

				var centerViewport = Center(viewport);
				var centerItem = Center(itemFrame);

				return new CGPoint(centerViewport.X - centerItem.X, 0);
			}

			if (snapPointsAlignment == SnapPointsAlignment.Start)
			{
				return new CGPoint(0, viewport.Top - itemFrame.Top);
			}

			if (snapPointsAlignment == SnapPointsAlignment.End)
			{
				return new CGPoint(0, viewport.Bottom - itemFrame.Bottom);
			}

			var centerViewport1 = Center(viewport);
			var centerItem1 = Center(itemFrame);

			return new CGPoint(0, centerViewport1.Y - centerItem1.Y);
		}

		static bool IsAtLeastHalfVisible(UICollectionViewLayoutAttributes item, CGRect viewport)
		{
			var itemFrame = item.Frame;
			var visibleArea = Area(CGRect.Intersect(itemFrame, viewport));

			return visibleArea >= Area(itemFrame) / 2;
		}

		static UICollectionViewLayoutAttributes Nearer(UICollectionViewLayoutAttributes a,
			UICollectionViewLayoutAttributes b,
			CGPoint target)
		{
			var dA = DistanceSquared(a.Frame, target);
			var dB = DistanceSquared(b.Frame, target);

			if (dA < dB)
			{
				return a;
			}

			return b;
		}

		public static UICollectionViewLayoutAttributes FindNextItem(UICollectionViewLayoutAttributes[] items,
			UICollectionViewScrollDirection direction, int step, CGPoint scrollingVelocity, int currentIndex)
		{
			var velocity = direction == UICollectionViewScrollDirection.Horizontal
				? scrollingVelocity.X
				: scrollingVelocity.Y;

			if (velocity == 0)
			{
				// The user isn't scrolling at all, just stay where we are
				return items[currentIndex];
			}

			// Move the index up or down by increment, depending on the velocity
			if (velocity > 0)
			{
				currentIndex = currentIndex + step;
			}
			else if (velocity < 0)
			{
				currentIndex = currentIndex - step;
			}

			// Make sure we're not out of bounds
			currentIndex = Clamp(currentIndex, 0, items.Length - 1);

			return items[currentIndex];
		}
	}
}