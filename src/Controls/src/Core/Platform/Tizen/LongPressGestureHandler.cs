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

			var position = new Point(
				e.LongPressGesture.LocalPoint.X.ToScaledDP(),
				e.LongPressGesture.LocalPoint.Y.ToScaledDP());

			if (e.LongPressGesture.State == NGestureStateType.Started)
			{
				Recognizer.SendLongPressing(View, GestureStatus.Started, position);
			}
			else if (e.LongPressGesture.State == NGestureStateType.Finished)
			{
				// LongPress completed successfully
				Recognizer.SendLongPressed(View, position);
				Recognizer.SendLongPressing(View, GestureStatus.Completed, position);
			}
			else if (e.LongPressGesture.State == NGestureStateType.Cancelled)
			{
				Recognizer.SendLongPressing(View, GestureStatus.Canceled, position);
			}
		}
	}
}
