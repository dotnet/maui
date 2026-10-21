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
		public InputTraceApplication(NativeHandle handle) : base(handle)
		{
		}

		public override void SendEvent(UIEvent uiEvent)
		{
			base.SendEvent(uiEvent);

			// Observe only after UIKit has processed a completed touch.
			var path = Environment.GetEnvironmentVariable("ISSUE17389_TAP_LOG");
			if (string.IsNullOrEmpty(path) || uiEvent.Type != UIEventType.Touches || uiEvent.AllTouches is not { } touches)
				return;

			foreach (UITouch touch in touches)
			{
				if (touch.Phase != UITouchPhase.Ended)
					continue;

				var message = new StringBuilder();
				message.Append(DateTime.UtcNow.ToString("O"));
				message.Append(" NativeTouchEnded pid=").Append(Environment.ProcessId);
				message.Append(" location=").Append(touch.LocationInView(null));
				message.Append(" keyWindow=").Append(touch.Window?.IsKeyWindow);
				var view = touch.View;
				for (int depth = 0; view is not null && depth < 12; depth++, view = view.Superview)
				{
					message.Append(" | ").Append(view.GetType().Name);
					message.Append(" id=").Append(view.AccessibilityIdentifier);
					message.Append(" frame=").Append(view.Frame);
					message.Append(" bounds=").Append(view.Bounds);
					message.Append(" interactive=").Append(view.UserInteractionEnabled);
					message.Append(" gestures=").Append(view.GestureRecognizers?.Length ?? 0);
				}
				message.AppendLine();
				System.IO.File.AppendAllText(path, message.ToString());
			}
		}
	}
}