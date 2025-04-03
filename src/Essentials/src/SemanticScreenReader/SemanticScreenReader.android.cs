using System;
using Android.App;
using Android.Content;
using Android.Views.Accessibility;

namespace Microsoft.Maui.Accessibility
{
	partial class SemanticScreenReaderImplementation : ISemanticScreenReader
	{
		public void Announce(string text)
		{
			var manager = Application.Context.GetSystemService(Context.AccessibilityService) as AccessibilityManager;
			var announcement = OperatingSystem.IsAndroidVersionAtLeast(33)
				? new AccessibilityEvent()
#pragma warning disable 618
				: AccessibilityEvent.Obtain();
#pragma warning restore 618

			if (manager == null || announcement == null)
				return;

			if (!(manager.IsEnabled || manager.IsTouchExplorationEnabled))
				return;

			announcement.EventType = EventTypes.Announcement;
			announcement.Text?.Add(new Java.Lang.String(text));
			manager.SendAccessibilityEvent(announcement);
		}
	}
}
