using System;

namespace Microsoft.Maui.Gtk.Platform
{
	/// <summary>
	/// Marks "the virtual view is being written to the widget", so the widget's change signals raised by that write are not
	/// echoed back to the virtual view.
	/// </summary>
	internal sealed class GtkUpdateGuard
	{
		int _depth;

		/// <summary>Whether a virtual-view-to-widget update is running.</summary>
		public bool IsActive => _depth > 0;

		/// <summary>Runs <paramref name="action"/> with the guard raised.</summary>
		public void Run(Action action)
		{
			_depth++;
			try
			{
				action();
			}
			finally
			{
				_depth--;
			}
		}
	}
}
