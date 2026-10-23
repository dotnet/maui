using Foundation;
using ObjCRuntime;
using UIKit;
using TapTrace = Microsoft.Maui.Controls.Platform.Issue17389TapTrace;
using UiSampleFlags = Microsoft.Maui.Controls.Platform.Issue17389TapTrace.ApplicationSample;

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
			try
			{
				if (TapTrace.IsRecording)
					ObserveBeforeDispatch(uiEvent);
			}
			catch
			{
				TapTrace.ApplicationObserverFailed();
			}
			finally
			{
				base.SendEvent(uiEvent);
			}
		}

		void ObserveBeforeDispatch(UIEvent uiEvent)
		{
			if (!TapTrace.ReserveApplicationEvent(out var generation))
				return;
			var samples = UiSampleFlags.None;
			long eventType = 0, eventHandle = 0, handle = 0, targetHandle = 0, windowHandle = 0;
			double eventTime = 0, x = 0, y = 0, touchTime = 0;
			int count = 0, phase = 0, targetKind = 0, applicationState = 0, fault = 0;
			bool touchSetPresent = false, isKey = false;
			try
			{
				eventHandle = (long)(nint)uiEvent.Handle;
				samples |= UiSampleFlags.Argument;
				eventType = (long)uiEvent.Type;
				samples |= UiSampleFlags.Type;
				eventTime = uiEvent.Timestamp;
				samples |= UiSampleFlags.Timestamp;
				applicationState = (int)ApplicationState;
				samples |= UiSampleFlags.ApplicationState;
				if (eventType == (long)UIEventType.Touches)
				{
					var touches = uiEvent.AllTouches;
					touchSetPresent = touches is not null;
					samples |= UiSampleFlags.TouchSet;
					if (touches is not null)
					{
						count = checked((int)touches.Count);
						samples |= UiSampleFlags.TouchCount;
						if (count == 1)
						{
							foreach (UITouch touch in touches)
							{
								handle = (long)(nint)touch.Handle;
								samples |= UiSampleFlags.TouchArgument;
								phase = (int)touch.Phase;
								samples |= UiSampleFlags.TouchPhase;
								touchTime = touch.Timestamp;
								samples |= UiSampleFlags.TouchTimestamp;
								var point = touch.LocationInView(null);
								x = (double)point.X;
								y = (double)point.Y;
								samples |= UiSampleFlags.TouchLocation;
								var target = touch.View;
								targetHandle = target is null ? 0 : (long)(nint)target.Handle;
								samples |= UiSampleFlags.Target;
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
								samples |= UiSampleFlags.TargetKind;
								var window = touch.Window;
								windowHandle = window is null ? 0 : (long)(nint)window.Handle;
								samples |= UiSampleFlags.Window;
								if (window is not null)
								{
									isKey = window.IsKeyWindow;
									samples |= UiSampleFlags.WindowKey;
								}
							}
						}
					}
				}
			}
			catch
			{
				fault = 1;
				TapTrace.ApplicationObserverFailed();
			}
			TapTrace.RecordApplicationBoundaryEvent(generation, samples, fault, eventType, eventHandle,
				eventTime, touchSetPresent, count, handle, phase, touchTime, x, y, targetHandle,
				targetKind, windowHandle, isKey, applicationState);
		}
	}
}