using System;
using System.Linq;
using Xamarin.UITest;
using Xamarin.UITest.Queries;

namespace Xamarin.Forms.Core.UITests
{
	internal static class Gestures
	{
		public static bool ScrollForElement (this IApp app, string query, Drag drag, int maxSteps = 25)
		{
			int count = 0;

			int centerTolerance = 50;

			Func<AppQuery, AppQuery> elementQuery = q => q.Raw (query);

			// Visible elements
			if (app.Query (elementQuery).Length > 1) {
				throw new UITestQueryMultipleResultsException (query);
			}

			// check to see if the element is visible already
			if (app.Query (elementQuery).Length == 1) {
				// centering an element whos CenterX is close to the bounding rectangle's center X can sometime register the swipe as a tap
				float elementDistanceToDragCenter = Math.Abs (app.Query (elementQuery).First ().Rect.CenterY - drag.DragBounds.CenterY);
 				if (elementDistanceToDragCenter > centerTolerance)
					app.CenterElementInView (elementQuery, drag.DragBounds, drag.DragDirection);
				return true;
			}

			// loop until element is seen
			while (app.Query (elementQuery).Length == 0 && count < maxSteps) {
				app.DragCoordinates (drag.XStart, drag.YStart, drag.XEnd, drag.YEnd);
				count++;
			}

			if (count != maxSteps) {
				// centering an element whos CenterX is close to the bounding rectangle's center X can sometime register the swipe as a tap
				float elementDistanceToDragCenter = Math.Abs (app.Query (elementQuery).First ().Rect.CenterY - drag.DragBounds.CenterY);
				if (elementDistanceToDragCenter > centerTolerance)
					app.CenterElementInView (elementQuery, drag.DragBounds, drag.DragDirection);
				return true;
			}

			count = 0;
			drag.DragDirection = drag.OppositeDirection;

			while (app.Query (elementQuery).Length == 0 && count < maxSteps) {
				app.DragCoordinates (drag.XStart, drag.YStart, drag.XEnd, drag.YEnd);
				count++;
			}

			if (count != maxSteps) {
				app.CenterElementInView (elementQuery, drag.DragBounds, drag.DragDirection);
				return true;
			}

			return false;
		}
			
		static void CenterElementInView (this IApp app, Func<AppQuery, AppQuery> element, AppRect containingView, Drag.Direction direction)
		{
			// TODO Implement horizontal centering

			if (direction == Drag.Direction.BottomToTop || direction == Drag.Direction.TopToBottom) {

				var elementBounds = app.Query (element).First ().Rect;

				bool elementCenterBelowContainerCenter = elementBounds.CenterY > containingView.CenterY;
				bool elementCenterAboveContainerCenter = elementBounds.CenterY < containingView.CenterY;

				var displacementToCenter = Math.Abs (elementBounds.CenterY - containingView.CenterY) / 2;

				// avoid drag as touch
				if (displacementToCenter < 50)
					return;

				if (elementCenterBelowContainerCenter) {
			
					var drag = new Drag (
						containingView,
						containingView.CenterX, containingView.CenterY + displacementToCenter,
						containingView.CenterX, containingView.CenterY - displacementToCenter,
						Drag.Direction.BottomToTop
						);

					app.DragCoordinates (drag.XStart, drag.YStart, drag.XEnd, drag.YEnd);

				} else if (elementCenterAboveContainerCenter) {

					var drag = new Drag (
						containingView,
						containingView.CenterX, containingView.CenterY - displacementToCenter,
						containingView.CenterX, containingView.CenterY + displacementToCenter,
						Drag.Direction.TopToBottom
						);

					app.DragCoordinates (drag.XStart, drag.YStart, drag.XEnd, drag.YEnd);
				}
			} 
		}

		public static void Pan (this IApp app, Drag drag)
		{
			app.DragCoordinates (drag.XStart, drag.YStart, drag.XEnd, drag.YEnd);
		}

		public static void ActivateContextMenu(this IApp app, string target)
		{
#if __IOS__
			var element = app.WaitForElement(target);
			var rect = element[0].Rect;
			var appRect = app.RootViewRect();

			app.DragCoordinates(rect.X + (0.85f * appRect.Width), 
				rect.CenterY, 
				rect.X + (0.25f * rect.Width),
				rect.CenterY);
#else
			app.TouchAndHold(target);
#endif

		}
	}
}