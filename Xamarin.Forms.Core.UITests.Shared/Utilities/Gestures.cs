using System;
using System.Linq;
using Xamarin.UITest;
using Xamarin.UITest.Queries;

namespace Xamarin.Forms.Core.UITests
{
	internal static class Gestures
	{
		public static bool ScrollForElement(this IApp app, string query, Drag drag, int maxSteps = 25)
		{
			Func<AppQuery, AppQuery> elementQuery = q => q.Raw(query);

			int centerTolerance = 50;
			var appResults = app.QueryNTimes(elementQuery, 10, null);

			// Visible elements
			if (appResults.Length > 1)
				throw new UITestQueryMultipleResultsException(query);

			appResults = app.QueryNTimes(elementQuery, maxSteps, () => app.DragCoordinates(drag.XStart, drag.YStart, drag.XEnd, drag.YEnd));
			if (appResults.Length > 0)
			{
				// centering an element whos CenterX is close to the bounding rectangle's center X can sometime register the swipe as a tap
				float elementDistanceToDragCenter = Math.Abs(appResults.First().Rect.CenterY - drag.DragBounds.CenterY);
				if (elementDistanceToDragCenter > centerTolerance)
					app.CenterElementInView(appResults.First().Rect, drag.DragBounds, drag.DragDirection);
				return true;
			}

			drag.DragDirection = drag.OppositeDirection;
			appResults = app.QueryNTimes(elementQuery, maxSteps, () => app.DragCoordinates(drag.XStart, drag.YStart, drag.XEnd, drag.YEnd));

			if (appResults.Length > 0)
			{
				app.CenterElementInView(appResults.First().Rect, drag.DragBounds, drag.DragDirection);
				return true;
			}

			return false;
		}

		static void CenterElementInView(this IApp app, AppRect elementBounds, AppRect containingView, Drag.Direction direction)
		{
			// TODO Implement horizontal centering

			if (direction == Drag.Direction.BottomToTop || direction == Drag.Direction.TopToBottom)
			{
				bool elementCenterBelowContainerCenter = elementBounds.CenterY > containingView.CenterY;
				bool elementCenterAboveContainerCenter = elementBounds.CenterY < containingView.CenterY;

				var displacementToCenter = Math.Abs(elementBounds.CenterY - containingView.CenterY) / 2;

				// avoid drag as touch
				if (displacementToCenter < 50)
					return;

				if (elementCenterBelowContainerCenter)
				{

					var drag = new Drag(
						containingView,
						containingView.CenterX, containingView.CenterY + displacementToCenter,
						containingView.CenterX, containingView.CenterY - displacementToCenter,
						Drag.Direction.BottomToTop
						);

					app.DragCoordinates(drag.XStart, drag.YStart, drag.XEnd, drag.YEnd);

				}
				else if (elementCenterAboveContainerCenter)
				{

					var drag = new Drag(
						containingView,
						containingView.CenterX, containingView.CenterY - displacementToCenter,
						containingView.CenterX, containingView.CenterY + displacementToCenter,
						Drag.Direction.TopToBottom
						);

					app.DragCoordinates(drag.XStart, drag.YStart, drag.XEnd, drag.YEnd);
				}
			}
		}

		public static void Pan(this IApp app, Drag drag)
		{
			app.DragCoordinates(drag.XStart, drag.YStart, drag.XEnd, drag.YEnd);
		}

		public static void ActivateContextMenu(this IApp app, string target)
		{
#if __IOS__
			var element = app.WaitForElement(target);
			var rect = element[0].Rect;
			var appRect = app.RootViewRect();
			var width = Math.Max(250, rect.Width);

			if ((rect.X + width) > appRect.Width)
			{
				width = appRect.Width - rect.X;
			}

			app.DragCoordinates(rect.X + (0.95f * width),
				rect.CenterY,
				rect.X + (0.05f * width),
				rect.CenterY);
#elif __ANDROID__
			app.TouchAndHold(target);
#elif __WINDOWS__
			// Since we know we're on desktop for the moment, just use ContextClick. If we get this running
			// on actual touch devices at some point, we'll need to check for that and use TouchAndHold
			app.Invoke("ContextClick", target);
#endif

		}

		public static void DismissContextMenu(this IApp app)
		{
#if __IOS__
			var screenbounds = app.RootViewRect();
			app.TapCoordinates(screenbounds.CenterX, screenbounds.CenterY);
#elif __ANDROID__
			app.Back();
#elif __WINDOWS__
			var screenbounds = app.RootViewRect();
			app.TapCoordinates (screenbounds.CenterX, screenbounds.CenterY);
#endif
		}
	}
}