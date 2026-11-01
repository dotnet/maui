using System;
using Microsoft.Maui.Accessibility;

namespace Microsoft.Maui.Essentials.Gtk;

/// <summary>
/// <see cref="ISemanticScreenReader"/> using <c>Gtk.Accessible.Announce</c> (GTK 4.14+) on the content of the active window (the Windows
/// AccessKit backend of GTK 4.22 crashes the process when a toplevel window itself is announced on).
/// Silently does nothing when there is no window or the loaded GTK is older than 4.14.
/// </summary>
sealed class GtkSemanticScreenReader : ISemanticScreenReader
{
	public void Announce(string text)
	{
		if (string.IsNullOrEmpty(text))
			return;

		GtkDispatch.Invoke(() =>
		{
			if (!GtkEssentialsContext.IsGtkAtLeast(4, 14))
				return;

			var window = GtkEssentialsContext.GetParentWindow();
			if ((window?.Child ?? window) is global::Gtk.IAccessible accessible)
				accessible.Announce(text, global::Gtk.AccessibleAnnouncementPriority.Medium);
		});
	}
}
