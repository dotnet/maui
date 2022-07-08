using Android.Views.Accessibility;

namespace Microsoft.Maui.Accessibility
{
	partial class SemanticScreenReaderImplementation : ISemanticScreenReader
	{
		public void Announce(string text)
		{
			var manager = Android.App.Application.Context.GetSystemService(Android.Content.Context.AccessibilityService) as AccessibilityManager;
			var announcement = new AccessibilityEvent();

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
