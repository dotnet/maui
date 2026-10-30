using Microsoft.Maui.Gtk.Handlers;

namespace Microsoft.Maui.Gtk.Hosting
{
	/// <summary>
	/// Patches the stock static property/command mappers with the GTK implementations, once per process.
	/// </summary>
	/// <remarks>
	/// The mappers are process-wide statics, so the patches are installed once and every GTK branch only runs for GTK widgets.
	/// Calling <see cref="Apply"/> again is cheap and re-installs the patches that a later re-mapping (for example Controls'
	/// <c>UseMauiApp</c>) has shadowed. Tests that construct handlers directly call <see cref="Apply"/> first.
	/// </remarks>
	public static class GtkMapperRegistration
	{
		static readonly object s_gate = new();
		static bool s_applied;

		/// <summary>Whether the mappers have been patched.</summary>
		public static bool IsApplied => s_applied;

		/// <summary>Patches the core mappers, or re-installs the patches that were shadowed since. Safe to call repeatedly.</summary>
		public static void Apply()
		{
			lock (s_gate)
			{
				if (s_applied)
				{
					// Controls re-maps the static mappers whenever an app is built: put the GTK branches back on top.
					GtkMapperExtensions.ReapplyShadowedPatches();
					return;
				}

				GtkViewMappers.Apply();
				GtkApplicationHandler.ApplyMappers();
				GtkWindowHandler.ApplyMappers();
				GtkContentViewHandler.ApplyMappers();
				GtkPageHandler.ApplyMappers();
				GtkLayoutHandler.ApplyMappers();
				GtkLabelHandler.ApplyMappers();
				GtkButtonHandler.ApplyMappers();
				GtkImageHandler.ApplyMappers();
				GtkEntryHandler.ApplyMappers();
				GtkEditorHandler.ApplyMappers();
				GtkScrollViewHandler.ApplyMappers();
				GtkBorderHandler.ApplyMappers();
				GtkSwitchHandler.ApplyMappers();
				GtkCheckBoxHandler.ApplyMappers();

				s_applied = true;
			}
		}
	}
}
