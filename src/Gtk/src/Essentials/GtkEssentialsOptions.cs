using System;

namespace Microsoft.Maui.Hosting;

/// <summary>
/// Options for the GTK implementations of the .NET MAUI Essentials APIs.
/// </summary>
/// <remarks>
/// Pass a configuration callback to <see cref="GtkEssentialsExtensions.UseMauiGtkEssentials"/>.
/// Every property is optional: unset values are derived from the entry assembly and the running GTK application.
/// </remarks>
public sealed class GtkEssentialsOptions
{
	/// <summary>
	/// Gets or sets the reverse-DNS application id (for example <c>com.contoso.notes</c>).
	/// It is the <see cref="Microsoft.Maui.ApplicationModel.IAppInfo.PackageName"/> and names the per-app XDG directories.
	/// When unset the <c>ApplicationId</c> assembly metadata is used, then the entry assembly name.
	/// </summary>
	public string? ApplicationId { get; set; }

	/// <summary>
	/// Gets or sets the display name returned by <see cref="Microsoft.Maui.ApplicationModel.IAppInfo.Name"/>.
	/// When unset the <c>AssemblyTitle</c> (then <c>AssemblyProduct</c>) of the entry assembly is used, then its name.
	/// </summary>
	public string? ApplicationName { get; set; }

	/// <summary>
	/// Gets or sets the callback that returns the window used as the parent of dialogs and launchers
	/// and as the target of window screenshots and screen reader announcements.
	/// Called on the GTK main thread. When unset the active window of the default <c>Gtk.Application</c> is used.
	/// </summary>
	public Func<global::Gtk.Window?>? ParentWindowProvider { get; set; }
}
