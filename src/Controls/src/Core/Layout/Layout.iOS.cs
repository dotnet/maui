#nullable disable
using System;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	public partial class Layout : ISafeAreaLayout
	{
		SafeAreaScrollViewCoordinator _safeAreaScrollViewCoordinator;

		Size ISafeAreaLayout.CrossPlatformArrange(Rect bounds, Thickness safeArea, bool delegateTopInset)
		{
			var safeBounds = new Rect(
				bounds.X + safeArea.Left,
				bounds.Y + safeArea.Top,
				bounds.Width - safeArea.HorizontalThickness,
				bounds.Height - safeArea.VerticalThickness);

			var arranged = CrossPlatformArrange(safeBounds);

			if (!delegateTopInset || safeArea.Top <= 0)
			{
				_safeAreaScrollViewCoordinator?.Reset();
				return arranged;
			}

			const double tolerance = 1;
			IView scrollHost = null;
			IView scrollContent = null;
			double contentTopInset = 0;
			var candidateCount = 0;

			foreach (var child in this)
			{
				if (child.Visibility != Visibility.Visible ||
					child.Opacity <= 0.01 ||
					SafeAreaScrollViewCoordinator.FindVerticalScrollContent(child) is not IView foundScrollContent)
					continue;

				var frame = child.Frame;
				var occupiesMostOfSafeHeight = frame.Height >= safeBounds.Height / 2;
				var beginsWithinSafeBounds = frame.Top >= safeBounds.Top - tolerance;
				var fixedTopSiblingsAreOverlays = FixedTopSiblingsAreOverlays(child, frame, safeBounds, tolerance);

				if (beginsWithinSafeBounds && occupiesMostOfSafeHeight && fixedTopSiblingsAreOverlays)
				{
					candidateCount++;
					scrollHost = child;
					scrollContent = foundScrollContent;
					contentTopInset = frame.Top - bounds.Top;

					if (candidateCount > 1)
						break;
				}
			}

			if (candidateCount != 1)
			{
				_safeAreaScrollViewCoordinator?.Reset();
				return arranged;
			}

			(_safeAreaScrollViewCoordinator ??= new()).TryDelegate(
				scrollHost,
				scrollContent,
				bounds,
				contentTopInset,
				safeArea.Top);

			return arranged;
		}

		void ISafeAreaLayout.DisconnectSafeArea() => _safeAreaScrollViewCoordinator?.Reset();

		bool FixedTopSiblingsAreOverlays(IView scrollHost, Rect scrollFrame, Rect safeBounds, double tolerance)
		{
			foreach (var sibling in this)
			{
				if (ReferenceEquals(sibling, scrollHost) ||
					sibling.Visibility != Visibility.Visible ||
					sibling.Opacity <= 0.01)
					continue;

				var siblingFrame = sibling.Frame;
				var isFixedAboveScrollHost =
					siblingFrame.Top >= safeBounds.Top - tolerance &&
					siblingFrame.Bottom <= scrollFrame.Top + tolerance;

				if (isFixedAboveScrollHost &&
					SafeAreaScrollViewCoordinator.ContainsNativeScrollView(sibling))
					return false;

				if (isFixedAboveScrollHost && sibling.ZIndex <= scrollHost.ZIndex)
					return false;
			}

			return true;
		}

		/// <summary>
		/// Maps the abstract InputTransparent property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="layout">The associated <see cref="Layout"/> instance.</param>
		[Obsolete]
		public static void MapInputTransparent(LayoutHandler handler, Layout layout) { }

		/// <summary>
		/// Maps the abstract InputTransparent property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="layout">The associated <see cref="Layout"/> instance.</param>
		[Obsolete]
		public static void MapInputTransparent(ILayoutHandler handler, Layout layout) { }
	}
}
