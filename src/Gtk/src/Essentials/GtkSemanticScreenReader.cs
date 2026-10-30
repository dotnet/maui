using System;
using Microsoft.Maui.Accessibility;

namespace Microsoft.Maui.Essentials.Gtk;

/// <summary>
/// <see cref="ISemanticScreenReader"/> using <c>Gtk.Accessible.Announce</c> (GTK 4.14+) on the active window.
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

			if (GtkEssentialsContext.GetParentWindow() is global::Gtk.IAccessible accessible)
				accessible.Announce(text, global::Gtk.AccessibleAnnouncementPriority.Medium);
		});
	}
}
