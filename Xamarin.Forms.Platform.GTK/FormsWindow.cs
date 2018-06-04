using Gtk;
using System;
using System.ComponentModel;
using System.Threading;

namespace Xamarin.Forms.Platform.GTK
{
	public class FormsWindow : Window
	{
		private Application _application;
		private Gdk.Size _lastSize;

		public FormsWindow ()
			: base (WindowType.Toplevel)
		{
			SetDefaultSize (800, 600);
			SetSizeRequest (400, 400);

			MainThreadID = Thread.CurrentThread.ManagedThreadId;
			MainWindow = this;

			if (SynchronizationContext.Current == null)
				SynchronizationContext.SetSynchronizationContext (new GtkSynchronizationContext ());

			WindowStateEvent += OnWindowStateEvent;
		}

		public static int MainThreadID { get; set; }
		public static Window MainWindow { get; set; }

		public void LoadApplication (Application application)
		{
			if (application == null)
				throw new ArgumentNullException (nameof (application));

			Application.SetCurrentApplication (application);
			_application = application;

			application.PropertyChanged += ApplicationOnPropertyChanged;
			UpdateMainPage ();

			_application.SendStart ();
		}

		public void SetApplicationTitle (string title)
		{
			if (string.IsNullOrEmpty (title))
				return;

			Title = title;
		}

		public void SetApplicationIcon (string icon)
		{
			if (string.IsNullOrEmpty (icon))
				return;

			var appliccationIconPixbuf = new Gdk.Pixbuf(icon);
			Icon = appliccationIconPixbuf;
		}

		public sealed override void Dispose ()
		{
			base.Dispose ();

			Dispose (true);
		}

		protected override bool OnDeleteEvent (Gdk.Event evnt)
		{
			base.OnDeleteEvent (evnt);

			Gtk.Application.Quit ();

			return true;
		}

		private void ApplicationOnPropertyChanged (object sender, PropertyChangedEventArgs args)
		{
			if (args.PropertyName == nameof (Application.MainPage)) {
				UpdateMainPage ();
			}
		}

		protected override bool OnConfigureEvent (Gdk.EventConfigure evnt)
		{
			Gdk.Size newSize = new Gdk.Size(evnt.Width, evnt.Height);

			if (_lastSize != newSize) {
				_lastSize = newSize;
				var pageRenderer = Platform.GetRenderer(_application.MainPage);
				pageRenderer?.SetElementSize (new Size (newSize.Width, newSize.Height));
			}

			return base.OnConfigureEvent (evnt);
		}

		private void UpdateMainPage ()
		{
			if (_application.MainPage == null)
				return;

			var platformRenderer = Child as PlatformRenderer;

			if (platformRenderer != null) {
				RemoveChildIfExists ();
				((IDisposable)platformRenderer.Platform).Dispose ();
			}

			var platform = new Platform();
			platform.PlatformRenderer.SetSizeRequest (WidthRequest, HeightRequest);
			Add (platform.PlatformRenderer);
			platform.SetPage (_application.MainPage);

			Child.ShowAll ();
		}

		private void RemoveChildIfExists ()
		{
			foreach (var child in Children) {
				var widget = child as Widget;

				if (widget != null) {
					Remove (widget);
				}
			}
		}

		private void OnWindowStateEvent (object o, WindowStateEventArgs args)
		{
			if (args.Event.ChangedMask == Gdk.WindowState.Iconified) {
				var windowState = args.Event.NewWindowState;

				if (windowState == Gdk.WindowState.Iconified)
					_application.SendSleep();
				else
					_application.SendResume ();
			}
		}

		private void Dispose (bool disposing)
		{
			if (disposing && _application != null) {
				WindowStateEvent -= OnWindowStateEvent;
				_application.PropertyChanged -= ApplicationOnPropertyChanged;
			}
		}
	}
}