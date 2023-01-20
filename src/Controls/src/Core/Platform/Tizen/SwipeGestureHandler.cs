using NGestureDetector = Tizen.NUI.GestureDetector;
using NGestureStateType = Tizen.NUI.Gesture.StateType;
using NPanGestureDetector = Tizen.NUI.PanGestureDetector;

namespace Microsoft.Maui.Controls.Platform
{
	public class SwipeGestureHandler : GestureHandler
	{
		double _totalX;
		double _totalY;

		public SwipeGestureHandler(IGestureRecognizer recognizer) : base(recognizer)
		{
			NativeDetector.Detected += OnDetected;
		}

		ISwipeGestureController Controller => (ISwipeGestureController)base.Recognizer;
		new SwipeGestureRecognizer Recognizer => (SwipeGestureRecognizer)base.Recognizer;

		new NPanGestureDetector NativeDetector => (NPanGestureDetector)base.NativeDetector;

		protected override NGestureDetector CreateNativeDetector(IGestureRecognizer recognizer)
		{
			return new NPanGestureDetector();
		}

		void OnDetected(object source, NPanGestureDetector.DetectedEventArgs e)
		{
			e.Handled = false;
			if (e.PanGesture.State == NGestureStateType.Started)
			{
				_totalX = 0;
				_totalY = 0;
			}
			else if (e.PanGesture.State == NGestureStateType.Continuing)
			{
				_totalX += e.PanGesture.Displacement.X;
				_totalY += e.PanGesture.Displacement.Y;
				Controller.SendSwipe(View, _totalX.ToScaledDP(), _totalY.ToScaledDP());
			}
			else if (e.PanGesture.State == NGestureStateType.Finished)
			{
				Controller.DetectSwipe(View, Recognizer.Direction);
			}
		}

	}
}