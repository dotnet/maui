using System;
using GLib;

namespace Microsoft.Maui.LifecycleEvents
{

	public class ActivationEventArgs : EventArgs
	{

		public ActivationEventArgs(ActivationState state) : base()
		{
			ActivationState = state;
		}

		public ActivationState ActivationState { get; }

	}

	public static class GtkLifecycle
	{

		public delegate void OnStartup(Gtk.Application application, EventArgs args);

		public delegate void OnLaunching(MauiGtkApplication application, EventArgs args);

		public delegate void OnLaunched(Gtk.Application application, EventArgs args);

		public delegate void OnOpened(Gtk.Application application, OpenedArgs args);

		public delegate void OnApplicationActivated(Gtk.Application application, EventArgs args);

		public delegate void OnShutdown(Gtk.Application application, EventArgs args);

		public delegate void OnCreated(Gtk.Window window, EventArgs args);

		public delegate void OnShown(Gtk.Window window, EventArgs args);

		public delegate void OnHidden(Gtk.Window window, EventArgs args);

		public delegate void OnVisibilityChanged(Gtk.Window window, Gtk.VisibilityNotifyEventArgs args);

		public delegate void OnStateChanged(Gtk.Window window, Gtk.WindowStateEventArgs args);

		public delegate void OnDelete(Gtk.Window window, Gtk.DeleteEventArgs args);
		
		internal delegate void OnMauiContextCreated(IMauiContext mauiContext);


	}

}