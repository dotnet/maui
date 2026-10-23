using Foundation;
using ObjCRuntime;
using UIKit;
using TapTrace = Microsoft.Maui.Controls.Platform.Issue17389TapTrace;

namespace Maui.Controls.Sample.Platform
{
	public class Application
	{
		static void Main(string[] args) => UIApplication.Main(args, typeof(InputTraceApplication), typeof(AppDelegate));
	}

	[Register("InputTraceApplication")]
	public class InputTraceApplication : UIApplication
	{
		public InputTraceApplication(NativeHandle handle) : base(handle)
		{
		}

		public override void SendEvent(UIEvent uiEvent)
		{
			if (TapTrace.IsRecording)
				ObserveBeforeDispatch(uiEvent);
			base.SendEvent(uiEvent);
		}

		void ObserveBeforeDispatch(UIEvent uiEvent)
		{
			var eventType = uiEvent.Type;
			if (eventType != UIEventType.Touches || !TapTrace.ReserveApplicationEvent(out var generation))
				return;
			var touches = uiEvent.AllTouches;
			var count = touches is null ? 0 : checked((int)touches.Count);
			long handle = 0, targetHandle = 0, windowHandle = 0;
			double x = 0, y = 0, touchTime = 0;
			int phase = 0, targetKind = 0;
			bool sampled = false, keySampled = false, isKey = false;
			if (touches is not null && count == 1)
			{
				foreach (UITouch touch in touches)
				{
					handle = (long)(nint)touch.Handle;
					phase = (int)touch.Phase;
					touchTime = touch.Timestamp;
					var point = touch.LocationInView(null);
					x = (double)point.X;
					y = (double)point.Y;
					var target = touch.View;
					targetHandle = target is null ? 0 : (long)(nint)target.Handle;
					targetKind = target switch
					{
						UILabel => 1,
						UIScrollView => 2,
						UIWindow => 3,
						Microsoft.Maui.Platform.LayoutView => 4,
						Microsoft.Maui.Platform.MauiView => 5,
						null => 0,
						_ => 6
					};
					var window = touch.Window;
					windowHandle = window is null ? 0 : (long)(nint)window.Handle;
					keySampled = window is not null;
					isKey = window?.IsKeyWindow ?? false;
					sampled = true;
				}
			}
			TapTrace.RecordApplicationEvent(generation, (int)eventType, (long)(nint)uiEvent.Handle,
				uiEvent.Timestamp, touches is not null, count, sampled, handle, phase, touchTime, x, y, targetHandle,
				targetKind, windowHandle, keySampled, isKey, (int)ApplicationState);
		}
	}
}