using System;
using System.Diagnostics;
using Gtk;
using Microsoft.Maui.LifecycleEvents;
using Application = GLib.Application;

namespace Microsoft.Maui
{

	public class MauiGtkMainWindow : Gtk.Window
	{

		public MauiGtkMainWindow() : base(WindowType.Toplevel)
		{
			WindowStateEvent += OnWindowStateEvent;
			Shown += OnShown;
			Hidden += OnHidden;
			//GtkWidget::visibility-notify-event has been deprecated since version 3.12
			// VisibilityNotifyEvent += OnVisibilityNotifyEvent;
			DeleteEvent += OnDeleteEvent;

		}

		void OnDeleteEvent(object o, DeleteEventArgs args)
		{
			MauiGtkApplication.Current.Services?.InvokeLifecycleEvents<GtkLifecycle.OnDelete>(del => del(this, args));

			if (MauiGtkApplication.Current.MainWindow == o)
			{

				((Application)MauiGtkApplication.CurrentGtkApplication).Quit();

				args.Event.SendEvent = true;
			}
		}

		//GtkWidget::visibility-notify-event has been deprecated since version 3.12
		// void OnVisibilityNotifyEvent(object o, VisibilityNotifyEventArgs args)
		// {
		// 	MauiGtkApplication.Current.Services?.InvokeLifecycleEvents<GtkLifecycle.OnVisibilityChanged>(del => del(this, args));
		// }

		void OnHidden(object? sender, EventArgs args)
		{
			MauiGtkApplication.Current.Services?.InvokeLifecycleEvents<GtkLifecycle.OnHidden>(del => del(this, args));
		}

		void OnShown(object? sender, EventArgs args)
		{
			MauiGtkApplication.Current.Services?.InvokeLifecycleEvents<GtkLifecycle.OnShown>(del => del(this, args));
		}

		void OnWindowStateEvent(object o, WindowStateEventArgs args)
		{
			MauiGtkApplication.Current.Services?.InvokeLifecycleEvents<GtkLifecycle.OnStateChanged>(del => del(this, args));
		}

	}

}