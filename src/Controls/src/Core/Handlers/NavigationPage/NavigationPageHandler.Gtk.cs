#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using Gtk;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls.Handlers
{

	public partial class NavigationPageHandler :
		ViewHandler<NavigationPage, GtkNavigationPage>
	{

		protected override GtkNavigationPage CreateNativeView()
		{
			return new();
		}

		protected override void ConnectHandler(GtkNavigationPage nativeView)
		{
			base.ConnectHandler(nativeView);

			var virtualView = VirtualView;
			virtualView.PushRequested += OnPushRequested;
			virtualView.PopRequested += OnPopRequested;
			virtualView.PopToRootRequested += OnPopToRootRequested;
			virtualView.InternalChildren.CollectionChanged += OnChildrenChanged;

			SetPage(virtualView.CurrentPage);
		}

		protected override void DisconnectHandler(GtkNavigationPage nativeView)
		{
			base.DisconnectHandler(nativeView);

			var virtualView = VirtualView;
			virtualView.PushRequested -= OnPushRequested;
			virtualView.PopRequested -= OnPopRequested;
			virtualView.PopToRootRequested -= OnPopToRootRequested;
			virtualView.InternalChildren.CollectionChanged -= OnChildrenChanged;
		}

		void SetPage(Page page)
		{
			if (MauiContext == null)
				return;

			if (page.ToNative(MauiContext) is { } nativePage)
				NativeView.PackStart(nativePage, true, true, 0);
		}

		void OnChildrenChanged(object? sender, NotifyCollectionChangedEventArgs e)
		{
			;
		}

		void OnPopToRootRequested(object? sender, NavigationRequestedEventArgs e)
		{
			;
		}

		void OnPopRequested(object? sender, NavigationRequestedEventArgs e)
		{
			;
		}

		void OnPushRequested(object? sender, NavigationRequestedEventArgs e)
		{
			;
		}

		[MissingMapper]
		public static void MapPadding(NavigationPageHandler handler, NavigationPage view) { }

		[MissingMapper]
		public static void MapBarTextColor(NavigationPageHandler handler, NavigationPage view) { }

		[MissingMapper]
		public static void MapBarBackground(NavigationPageHandler handler, NavigationPage view) { }

		[MissingMapper]
		public static void MapTitleIcon(NavigationPageHandler handler, NavigationPage view) { }

		[MissingMapper]
		public static void MapTitleView(NavigationPageHandler handler, NavigationPage view) { }

	}

	public class GtkNavigationPage : Gtk.Box
	{

		public GtkNavigationPage() : base(Orientation.Horizontal, 0) { }

	}

}