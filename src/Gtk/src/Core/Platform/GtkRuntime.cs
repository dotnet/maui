using System;
using GtkApplication = Gtk.Application;

namespace Microsoft.Maui.Gtk.Platform
{
	/// <summary>
	/// Process-wide GTK state of the backend: one-time initialisation, the identity of the GTK (main) thread and
	/// runtime version checks. GTK may only be used from the thread that called <c>gtk_init</c>.
	/// </summary>
	public static class GtkRuntime
	{
		static readonly object s_gate = new();
		static int s_threadId;
		static bool s_initialized;

		/// <summary>The oldest GTK 4 minor version the backend supports; newer APIs must be guarded with <see cref="AtLeast"/>.</summary>
		public const int MinimumMinorVersion = 14;

		/// <summary>Initialises GTK on the calling thread, which becomes the GTK thread. Idempotent.</summary>
		public static void EnsureInitialized()
		{
			lock (s_gate)
			{
				if (s_initialized)
					return;

				GtkApplication.Init();
				s_threadId = Environment.CurrentManagedThreadId;
				s_initialized = true;
			}
		}

		/// <summary>Whether <see cref="EnsureInitialized"/> has run.</summary>
		public static bool IsInitialized => s_initialized;

		/// <summary>Whether the calling thread is the GTK thread.</summary>
		public static bool IsGtkThread => s_initialized && Environment.CurrentManagedThreadId == s_threadId;

		/// <summary>Whether the loaded GTK library is at least <paramref name="major"/>.<paramref name="minor"/>.</summary>
		/// <remarks>Used to guard APIs newer than the 4.14 floor (see <see cref="MinimumMinorVersion"/>).</remarks>
		public static bool AtLeast(uint major, uint minor)
		{
			EnsureInitialized();
			return global::Gtk.Global.MajorVersion > major
				|| (global::Gtk.Global.MajorVersion == major && global::Gtk.Global.MinorVersion >= minor);
		}

		/// <summary>Runs <paramref name="action"/> on the GTK thread: inline when already there, otherwise queued on the main loop.</summary>
		internal static void Post(Action action)
		{
			if (IsGtkThread)
			{
				action();
				return;
			}

			GLib.Idle.Add(() =>
			{
				action();
				return false;
			});
		}
	}
}
