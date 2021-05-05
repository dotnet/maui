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

		public static void MapTitleIcon(NavigationPageHandler handler, NavigationPage view) { }

		public static void MapTitleView(NavigationPageHandler handler, NavigationPage view) { }

		public static void MapBarBackground(NavigationPageHandler handler, NavigationPage view)
		{
			var NavPage = handler.VirtualViewWithValidation();
			var barBackgroundBrush = NavPage.BarBackground;

			if (Brush.IsNullOrEmpty(barBackgroundBrush) &&
				NavPage.BarBackgroundColor != null)
				barBackgroundBrush = new SolidColorBrush(NavPage.BarBackgroundColor);

			if (barBackgroundBrush == null)
				return;

			var navController = handler._controlsNavigationController;
			var NavigationBar = navController.NavigationBar;

			if (NativeVersion.IsAtLeast(13))
			{
				var navigationBarAppearance = NavigationBar.StandardAppearance;

				navigationBarAppearance.ConfigureWithOpaqueBackground();

				//if (barBackgroundColor == null)
				//{
				//	navigationBarAppearance.BackgroundColor = ColorExtensions.BackgroundColor;

				//	var parentingViewController = GetParentingViewController();
				//	parentingViewController?.SetupDefaultNavigationBarAppearance();
				//}
				//else
				//	navigationBarAppearance.BackgroundColor = barBackgroundColor.ToUIColor();

				var backgroundImage = NavigationBar.GetBackgroundImage(barBackgroundBrush);
				navigationBarAppearance.BackgroundImage = backgroundImage;

				NavigationBar.CompactAppearance = navigationBarAppearance;
				NavigationBar.StandardAppearance = navigationBarAppearance;
				NavigationBar.ScrollEdgeAppearance = navigationBarAppearance;
			}
			else
			{
				var backgroundImage = NavigationBar.GetBackgroundImage(barBackgroundBrush);
				NavigationBar.SetBackgroundImage(backgroundImage, UIBarMetrics.Default);
			}
		}

		public static void MapBarTextColor(NavigationPageHandler handler, NavigationPage view)
		{
			var NavPage = handler.VirtualViewWithValidation();

			var navController = handler._controlsNavigationController;
			var NavigationBar = navController.NavigationBar;

			var barTextColor = NavPage.BarTextColor;
			if (NavigationBar == null)
				return;

			// Determine new title text attributes via global static data
			var globalTitleTextAttributes = UINavigationBar.Appearance.TitleTextAttributes;
			var titleTextAttributes = new UIStringAttributes
			{
				ForegroundColor = barTextColor == null ? globalTitleTextAttributes?.ForegroundColor : barTextColor.ToNative(),
				Font = globalTitleTextAttributes?.Font
			};

			// Determine new large title text attributes via global static data
			var largeTitleTextAttributes = titleTextAttributes;
			if (NativeVersion.IsAtLeast(11))
			{
				var globalLargeTitleTextAttributes = UINavigationBar.Appearance.LargeTitleTextAttributes;

				largeTitleTextAttributes = new UIStringAttributes
				{
					ForegroundColor = barTextColor == null ? globalLargeTitleTextAttributes?.ForegroundColor : barTextColor.ToNative(),
					Font = globalLargeTitleTextAttributes?.Font
				};
			}

			if (NativeVersion.IsAtLeast(13))
			{
				if (NavigationBar.CompactAppearance != null)
				{
					NavigationBar.CompactAppearance.TitleTextAttributes = titleTextAttributes;
					NavigationBar.CompactAppearance.LargeTitleTextAttributes = largeTitleTextAttributes;
				}

				NavigationBar.StandardAppearance.TitleTextAttributes = titleTextAttributes;
				NavigationBar.StandardAppearance.LargeTitleTextAttributes = largeTitleTextAttributes;

				if (NavigationBar.ScrollEdgeAppearance != null)
				{
					NavigationBar.ScrollEdgeAppearance.TitleTextAttributes = titleTextAttributes;
					NavigationBar.ScrollEdgeAppearance.LargeTitleTextAttributes = largeTitleTextAttributes;
				}
			}
			else
			{
				NavigationBar.TitleTextAttributes = titleTextAttributes;

				if (NativeVersion.IsAtLeast(11))
					NavigationBar.LargeTitleTextAttributes = largeTitleTextAttributes;
			}

			//// set Tint color (i. e. Back Button arrow and Text)
			//var iconColor = Current != null ? NavigationPage.GetIconColor(Current) : null;
			//if (iconColor == null)
			//	iconColor = barTextColor;

			//NavigationBar.TintColor = iconColor == null || NavPage.OnThisPlatform().GetStatusBarTextColorMode() == StatusBarTextColorMode.DoNotAdjust
			//	? UINavigationBar.Appearance.TintColor
			//	: iconColor.ToUIColor();
		}


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

		void OnPushRequested(object? sender, NavigationRequestedEventArgs e)
		{
			_controlsNavigationController?
				.OnPushRequested(e, this.MauiContext);
		}

		void OnPopRequested(object? sender, NavigationRequestedEventArgs e)
		{
			_controlsNavigationController?
				.OnPopRequestedAsync(e)
				.FireAndForget((exc) => Log.Warning(nameof(NavigationPage), $"{exc}"));
		}

		internal void SendPopping(Task popTask)
		{
			if (VirtualView == null)
				return;

			VirtualView.PopAsyncInner(false, true, true)
				.FireAndForget((exc) => Log.Warning(nameof(NavigationPage), $"{exc}"));
		}
	}
}
