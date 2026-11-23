#nullable enable
using Microsoft.UI.Dispatching;
using Windows.UI.ViewManagement;

namespace Microsoft.Maui.Controls.Animations
{
	internal sealed partial class PlatformMotionPolicy
	{
		UISettings? _settings;
		DispatcherQueue? _queue;

		UISettings Settings => _settings ??= new UISettings();

		partial void ReadReducedMotionCore(ref bool reduced) => reduced = !Settings.AnimationsEnabled;

		partial void StartObserving()
		{
			_queue = DispatcherQueue.GetForCurrentThread();
			Settings.AnimationsEnabledChanged += OnAnimationsEnabledChanged;
		}

		partial void StopObserving() => Settings.AnimationsEnabledChanged -= OnAnimationsEnabledChanged;

		// Raised on a background thread.
		void OnAnimationsEnabledChanged(UISettings sender, UISettingsAnimationsEnabledChangedEventArgs args)
			=> _queue?.TryEnqueue(RaiseChanged);
	}
}
