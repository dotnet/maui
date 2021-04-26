#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Handlers;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers
{
	public partial class NavigationPageHandler :
		ViewHandler<NavigationPage, UIView>, INativeViewHandler
	{
		ControlsNavigationController _controlsNavigationController;
		UIViewController? INativeViewHandler.ViewController => _controlsNavigationController;

		protected override UIView CreateNativeView()
		{
			_controlsNavigationController = new ControlsNavigationController(this);

			if (_controlsNavigationController.View == null)
				throw new NullReferenceException("ControlsNavigationController.View is null");

			return _controlsNavigationController.View;
		}

		public static void MapPadding(NavigationPageHandler handler, NavigationPage view) { }

		public static void MapTitleColor(NavigationPageHandler handler, NavigationPage view) { }

		public static void MapNavigationBarBackground(NavigationPageHandler handler, NavigationPage view) { }

		public static void MapTitleIcon(NavigationPageHandler handler, NavigationPage view) { }

		public static void MapTitleView(NavigationPageHandler handler, NavigationPage view) { }


		protected override void ConnectHandler(UIView nativeView)
		{
			base.ConnectHandler(nativeView);

			if (VirtualView == null)
				return;

			VirtualView.PushRequested += OnPushRequested;
			VirtualView.PopRequested += OnPopRequested;
			_controlsNavigationController.LoadPages(this.MauiContext);

			//VirtualView.PopToRootRequested += OnPopToRootRequested;
			//VirtualView.RemovePageRequested += OnRemovedPageRequested;
			//VirtualView.InsertPageBeforeRequested += OnInsertPageBeforeRequested;
		}

		protected override void DisconnectHandler(UIView nativeView)
		{
			base.DisconnectHandler(nativeView);

			if (VirtualView == null)
				return;

			VirtualView.PushRequested -= OnPushRequested;
			VirtualView.PopRequested -= OnPopRequested;
			//VirtualView.PopToRootRequested -= OnPopToRootRequested;
			//VirtualView.RemovePageRequested -= OnRemovedPageRequested;
			//VirtualView.InsertPageBeforeRequested -= OnInsertPageBeforeRequested;
		}

		void OnPopRequested(object sender, NavigationRequestedEventArgs e)
		{
			_controlsNavigationController?
				.OnPopRequestedAsync(e)
				.FireAndForget((exc) => Log.Warning(nameof(NavigationPage), $"{exc}"));
		}

		void OnPushRequested(object sender, NavigationRequestedEventArgs e)
		{
			_controlsNavigationController?
				.OnPushRequested(e, this.MauiContext);
		}

		internal void SendPopping(Task popTask)
		{
			if (VirtualView == null)
				return;

			(VirtualView as INavigationPageController)?.PopAsyncInner(false, true)
				.FireAndForget((exc) => Log.Warning(nameof(NavigationPage), $"{exc}"));
		}
	}
}
