#nullable enable
using Foundation;
using UIKit;

namespace Microsoft.Maui.Controls.Animations
{
	internal sealed partial class PlatformMotionPolicy
	{
		NSObject? _observer;

		partial void ReadReducedMotionCore(ref bool reduced) => reduced = UIAccessibility.IsReduceMotionEnabled;

		partial void StartObserving()
			=> _observer ??= NSNotificationCenter.DefaultCenter.AddObserver(UIApplication.ReduceMotionStatusDidChangeNotification, _ => RaiseChanged());

		partial void StopObserving()
		{
			_observer?.Dispose();
			_observer = null;
		}
	}
}
