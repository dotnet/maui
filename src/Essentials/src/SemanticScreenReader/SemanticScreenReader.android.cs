using Android.Views.Accessibility;

namespace Microsoft.Maui.Essentials
{
	public static partial class SemanticScreenReader
	{
		static void PlatformAnnounce(string text)
		{
			AccessibilityManager manager = Android.App.Application.Context.GetSystemService(Android.Content.Context.AccessibilityService) as AccessibilityManager;
			AccessibilityEvent announcement = AccessibilityEvent.Obtain();

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
