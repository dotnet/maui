namespace Microsoft.Maui.Gtk.Platform
{
	/// <summary>
	/// Implemented by backend container widgets so a child can be swapped for its wrapper (or vice versa) in place.
	/// </summary>
	public interface IGtkChildHost
	{
		/// <summary>Replaces <paramref name="oldChild"/> with <paramref name="newChild"/> at the same position.</summary>
		void ReplaceChild(global::Gtk.Widget oldChild, global::Gtk.Widget newChild);
	}
}
