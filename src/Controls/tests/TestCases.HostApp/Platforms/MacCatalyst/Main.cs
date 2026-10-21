#nullable enable
using System.Text;
using Foundation;
using ObjCRuntime;
using UIKit;

namespace Maui.Controls.Sample.Platform
{
	public class Application
	{
		static void Main(string[] args) => UIApplication.Main(args, typeof(InputTraceApplication), typeof(AppDelegate));
	}

	[Register("InputTraceApplication")]
	public class InputTraceApplication : UIApplication
	{
		const int MaxTrackedTouches = 16;
		const int MaxTargetCharacters = 2048;
		readonly TouchTrace?[] _pending = new TouchTrace?[MaxTrackedTouches];
		long _sequence;
		int _evictedRecords;
		int _restartedRecords;

		public InputTraceApplication(NativeHandle handle) : base(handle)
		{
		}

		public override void SendEvent(UIEvent uiEvent)
		{
			base.SendEvent(uiEvent);

			var path = Environment.GetEnvironmentVariable("ISSUE17389_TAP_LOG");
			if (string.IsNullOrEmpty(path) || uiEvent.Type != UIEventType.Touches || uiEvent.AllTouches is not { } touches)
				return;

			foreach (UITouch touch in touches)
			{
				var phase = touch.Phase;
				var handle = touch.Handle;
				var location = touch.LocationInView(null);
				int slot = FindSlot(handle);
				var trace = _pending[slot];
				if (trace is not null && trace.Handle == handle && phase == UITouchPhase.Began)
				{
					_restartedRecords++;
					trace = null;
				}

				if (trace is null || trace.Handle != handle)
				{
					trace = new TouchTrace
					{
						Handle = handle,
						FirstPhase = phase,
						FirstTimestamp = uiEvent.Timestamp,
						FirstLocation = location,
						FirstKeyWindow = touch.Window?.IsKeyWindow,
						FirstTarget = DescribeTarget(touch.View)
					};
					_pending[slot] = trace;
				}

				trace.LastSequence = ++_sequence;
				switch (phase)
				{
					case UITouchPhase.Began:
						trace.Began++;
						break;
					case UITouchPhase.Moved:
						trace.Moved++;
						trace.LastMovedTimestamp = uiEvent.Timestamp;
						trace.LastMovedLocation = location;
						break;
					case UITouchPhase.Stationary:
						trace.Stationary++;
						trace.LastStationaryTimestamp = uiEvent.Timestamp;
						trace.LastStationaryLocation = location;
						break;
					case UITouchPhase.Ended:
						trace.Ended++;
						break;
					case UITouchPhase.Cancelled:
						trace.Cancelled++;
						break;
					default:
						trace.Other++;
						break;
				}

				if (phase is not (UITouchPhase.Ended or UITouchPhase.Cancelled))
					continue;

				// Drop retained state before terminal-only serialization and I/O.
				_pending[slot] = null;
				var message = new StringBuilder();
				message.Append(DateTime.UtcNow.ToString("O"));
				message.Append(" NativeTouchTerminal pid=").Append(Environment.ProcessId);
				message.Append(" touch=").Append(handle);
				message.Append(" eventType=").Append(uiEvent.Type);
				message.Append(" phase=").Append(touch.Phase);
				message.Append(" eventTimestamp=").Append(uiEvent.Timestamp);
				message.Append(" location=").Append(location);
				message.Append(" keyWindow=").Append(touch.Window?.IsKeyWindow);
				message.Append(" firstPhase=").Append(trace.FirstPhase);
				message.Append(" beganObserved=").Append(trace.Began != 0);
				message.Append(" firstTimestamp=").Append(trace.FirstTimestamp);
				message.Append(" firstLocation=").Append(trace.FirstLocation);
				message.Append(" firstKeyWindow=").Append(trace.FirstKeyWindow);
				message.Append(" phases=").Append(trace.Began).Append('/').Append(trace.Moved).Append('/')
					.Append(trace.Stationary).Append('/').Append(trace.Ended).Append('/').Append(trace.Cancelled)
					.Append(" other=").Append(trace.Other);
				if (trace.Moved != 0)
					message.Append(" lastMoved=").Append(trace.LastMovedTimestamp).Append('@').Append(trace.LastMovedLocation);
				if (trace.Stationary != 0)
					message.Append(" lastStationary=").Append(trace.LastStationaryTimestamp).Append('@').Append(trace.LastStationaryLocation);
				message.Append(" evictedRecords=").Append(_evictedRecords);
				message.Append(" restartedRecords=").Append(_restartedRecords);
				message.Append(" firstTarget=[").Append(trace.FirstTarget);
				message.Append("] terminalTarget=[").Append(DescribeTarget(touch.View)).Append(']');
				message.AppendLine();
				System.IO.File.AppendAllText(path, message.ToString());
			}
		}

		int FindSlot(NativeHandle handle)
		{
			int vacant = -1;
			int oldest = 0;
			for (int i = 0; i < _pending.Length; i++)
			{
				var trace = _pending[i];
				if (trace is null)
				{
					if (vacant < 0)
						vacant = i;
					continue;
				}
				if (trace.Handle == handle)
					return i;
				if (_pending[oldest] is not { } previous || trace.LastSequence < previous.LastSequence)
					oldest = i;
			}
			if (vacant >= 0)
				return vacant;

			_evictedRecords++;
			return oldest;
		}

		static string DescribeTarget(UIView? view)
		{
			if (view is null)
				return "<null>";

			var description = new StringBuilder(MaxTargetCharacters);
			for (int depth = 0; view is not null && depth < 12; depth++, view = view.Superview)
			{
				var typeName = view.GetType().Name;
				var identifier = view.AccessibilityIdentifier;
				description.Append(" | ").Append(typeName.AsSpan(0, Math.Min(typeName.Length, 96)));
				if (typeName.Length > 96)
					description.Append("[type truncated]");
				description.Append(" nativeHandle=").Append(view.Handle);
				description.Append(" id=");
				if (identifier is not null)
				{
					description.Append(identifier.AsSpan(0, Math.Min(identifier.Length, 128)));
					if (identifier.Length > 128)
						description.Append("[id truncated]");
				}
				description.Append(" frame=").Append(view.Frame);
				description.Append(" bounds=").Append(view.Bounds);
				description.Append(" interactive=").Append(view.UserInteractionEnabled);
				description.Append(" gestures=").Append(view.GestureRecognizers?.Length ?? 0);
				if (description.Length >= MaxTargetCharacters)
				{
					const string suffix = " [truncated]";
					description.Length = MaxTargetCharacters - suffix.Length;
					description.Append(suffix);
					break;
				}
			}
			return description.ToString();
		}

		sealed class TouchTrace
		{
			public NativeHandle Handle;
			public UITouchPhase FirstPhase;
			public double FirstTimestamp;
			public CoreGraphics.CGPoint FirstLocation;
			public bool? FirstKeyWindow;
			public string FirstTarget = "";
			public long LastSequence;
			public double LastMovedTimestamp, LastStationaryTimestamp;
			public CoreGraphics.CGPoint LastMovedLocation, LastStationaryLocation;
			public int Began, Moved, Stationary, Ended, Cancelled, Other;
		}
	}
}