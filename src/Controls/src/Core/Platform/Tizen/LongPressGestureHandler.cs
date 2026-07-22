using Microsoft.Maui.Graphics;
using NGestureDetector = Tizen.NUI.GestureDetector;
using NGestureStateType = Tizen.NUI.Gesture.StateType;
using NLongPressGestureDetector = Tizen.NUI.LongPressGestureDetector;

namespace Microsoft.Maui.Controls.Platform
{
	public class LongPressGestureHandler : GestureHandler
	{
		public LongPressGestureHandler(IGestureRecognizer recognizer) : base(recognizer)
		{
			NativeDetector.Detected += OnDetected;
			
			// Configure minimum holding time from LongPressGestureRecognizer properties
			if (Recognizer is LongPressGestureRecognizer longPress)
			{
				NativeDetector.SetMinimumHoldingTime((uint)longPress.MinimumPressDuration);
			}
		}

		new LongPressGestureRecognizer Recognizer => (LongPressGestureRecognizer)base.Recognizer;
		new NLongPressGestureDetector NativeDetector => (NLongPressGestureDetector)base.NativeDetector;

		protected override NGestureDetector CreateNativeDetector(IGestureRecognizer recognizer)
		{
			var longPress = (LongPressGestureRecognizer)recognizer;
			
			// Create detector with touch count
			// Tizen supports NumberOfTouches configuration
			return new NLongPressGestureDetector((uint)longPress.NumberOfTouchesRequired);
		}

		void OnDetected(object source, NLongPressGestureDetector.DetectedEventArgs e)
		{
			e.Handled = false; // Allow gesture coexistence
			
			if (View == null)
				return;

			var originPoint = new Point(
				e.LongPressGesture.LocalPoint.X.ToScaledDP(),
				e.LongPressGesture.LocalPoint.Y.ToScaledDP());

			Func<IElement?, Point?> getPosition = (relativeTo) => CalculatePosition(relativeTo, originPoint, View);

			if (e.LongPressGesture.State == NGestureStateType.Started)
			{
				Recognizer.SendLongPressing(View, GestureStatus.Started, getPosition);
			}
			else if (e.LongPressGesture.State == NGestureStateType.Finished)
			{
				// LongPress completed successfully
				Recognizer.SendLongPressed(View, getPosition);
				Recognizer.SendLongPressing(View, GestureStatus.Completed, getPosition);
			}
			else if (e.LongPressGesture.State == NGestureStateType.Cancelled)
			{
				Recognizer.SendLongPressing(View, GestureStatus.Canceled, getPosition);
			}
		}

		static Point? CalculatePosition(IElement? relativeTo, Point originPoint, View virtualView)
		{
			// If relativeTo is null or same as the view, return position relative to the view
			if (relativeTo == null || relativeTo == virtualView)
				return originPoint;

			// Calculate position relative to another element
			var targetViewScreenLocation = virtualView.GetLocationOnScreen();
			if (!targetViewScreenLocation.HasValue)
				return null;

			var windowX = targetViewScreenLocation.Value.X + originPoint.X;
			var windowY = targetViewScreenLocation.Value.Y + originPoint.Y;

			var relativeViewLocation = ((View)relativeTo).GetLocationOnScreen();
			if (!relativeViewLocation.HasValue)
				return new Point(windowX, windowY);

			return new Point(windowX - relativeViewLocation.Value.X, windowY - relativeViewLocation.Value.Y);
		}
	}
}
