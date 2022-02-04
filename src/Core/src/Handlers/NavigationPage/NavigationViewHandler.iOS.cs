#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Handlers;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class NavigationViewHandler :
		ViewHandler<IStackNavigationView, UIView>, IPlatformViewHandler
	{
		ControlsNavigationController? _controlsNavigationController;
		UIViewController? IPlatformViewHandler.ViewController => _controlsNavigationController;

		public IStackNavigationView NavigationView => ((IStackNavigationView)VirtualView);

		public IReadOnlyList<IView> NavigationStack { get; private set; } = new List<IView>();

		protected override UIView CreatePlatformView()
		{
			_controlsNavigationController = new ControlsNavigationController(this);

			if (_controlsNavigationController.View == null)
				throw new NullReferenceException("ControlsNavigationController.View is null");

			return _controlsNavigationController.View;
		}

		public static void RequestNavigation(NavigationViewHandler arg1, IStackNavigation arg2, object? arg3)
		{
			arg1.NavigationStack = (arg3 as NavigationRequest)!.NavigationStack;
			//if (arg3 is NavigationRequest args)
			//	arg1.OnPushRequested(args);
		}

		//void OnPushRequested(NavigationRequest e)
		//{
		//	_controlsNavigationController?
		//		.OnPushRequested(e, this.MauiContext!);
		//}

		//void OnPopRequested(NavigationRequest e)
		//{
		//	_controlsNavigationController?
		//		.OnPopRequestedAsync(e)
		//		.FireAndForget((exc) => { });
		//}

		internal void SendPopping(Task popTask)
		{
			//if (VirtualView is not INavigationView nvi)
			//	return;

			//// TODO MAUI
			//nvi
			//	.PopAsync()
			//	.FireAndForget((e) =>
			//	{
			//		//Log.Warning(nameof(NavigationViewHandler), $"{e}");
			//	});
		}

		//protected override void ConnectHandler(UIView nativeView)
		//{
		//	base.ConnectHandler(nativeView);

		//	if (VirtualView == null || MauiContext == null || _controlsNavigationController == null)
		//		return;

		//	_controlsNavigationController.LoadPages(this.MauiContext);
		//}

		//public static void MapPadding(NavigationViewHandler handler, INavigationView view) { }

		//public static void MapTitleIcon(NavigationViewHandler handler, INavigationView view) { }

		//public static void MapTitleView(NavigationViewHandler handler, INavigationView view) { }

		////public static void MapBarBackground(NavigationViewHandler handler, INavigationView view)
		////{
		////	var NavPage = handler.VirtualView;
		////	var barBackgroundBrush = NavPage.BarBackground;

		////	if (Brush.IsNullOrEmpty(barBackgroundBrush) &&
		////		NavPage.BarBackgroundColor != null)
		////		barBackgroundBrush = new SolidColorBrush(NavPage.BarBackgroundColor);

		////	if (barBackgroundBrush == null)
		////		return;

		////	var navController = handler._controlsNavigationController;
		////	var NavigationBar = navController.NavigationBar;

		////	if (PlatformVersion.IsAtLeast(13))
		////	{
		////		var navigationBarAppearance = NavigationBar.StandardAppearance;

		////		navigationBarAppearance.ConfigureWithOpaqueBackground();

		////		//if (barBackgroundColor == null)
		////		//{
		////		//	navigationBarAppearance.BackgroundColor = ColorExtensions.BackgroundColor;

		////		//	var parentingViewController = GetParentingViewController();
		////		//	parentingViewController?.SetupDefaultNavigationBarAppearance();
		////		//}
		////		//else
		////		//	navigationBarAppearance.BackgroundColor = barBackgroundColor.ToUIColor();

		////		var backgroundImage = NavigationBar.GetBackgroundImage(barBackgroundBrush);
		////		navigationBarAppearance.BackgroundImage = backgroundImage;

		////		NavigationBar.CompactAppearance = navigationBarAppearance;
		////		NavigationBar.StandardAppearance = navigationBarAppearance;
		////		NavigationBar.ScrollEdgeAppearance = navigationBarAppearance;
		////	}
		////	else
		////	{
		////		var backgroundImage = NavigationBar.GetBackgroundImage(barBackgroundBrush);
		////		NavigationBar.SetBackgroundImage(backgroundImage, UIBarMetrics.Default);
		////	}
		////}

		////public static void MapBarTextColor(NavigationViewHandler handler, NavigationView view)
		////{
		////	var NavPage = handler.VirtualView;

		////	var navController = handler._controlsNavigationController;
		////	var NavigationBar = navController.NavigationBar;

		////	var barTextColor = NavPage.BarTextColor;
		////	if (NavigationBar == null)
		////		return;

		////	// Determine new title text attributes via global static data
		////	var globalTitleTextAttributes = UINavigationBar.Appearance.TitleTextAttributes;
		////	var titleTextAttributes = new UIStringAttributes
		////	{
		////		ForegroundColor = barTextColor == null ? globalTitleTextAttributes?.ForegroundColor : barTextColor.ToPlatform(),
		////		Font = globalTitleTextAttributes?.Font
		////	};

		////	// Determine new large title text attributes via global static data
		////	var largeTitleTextAttributes = titleTextAttributes;
		////	if (PlatformVersion.IsAtLeast(11))
		////	{
		////		var globalLargeTitleTextAttributes = UINavigationBar.Appearance.LargeTitleTextAttributes;

		////		largeTitleTextAttributes = new UIStringAttributes
		////		{
		////			ForegroundColor = barTextColor == null ? globalLargeTitleTextAttributes?.ForegroundColor : barTextColor.ToPlatform(),
		////			Font = globalLargeTitleTextAttributes?.Font
		////		};
		////	}

		////	if (PlatformVersion.IsAtLeast(13))
		////	{
		////		if (NavigationBar.CompactAppearance != null)
		////		{
		////			NavigationBar.CompactAppearance.TitleTextAttributes = titleTextAttributes;
		////			NavigationBar.CompactAppearance.LargeTitleTextAttributes = largeTitleTextAttributes;
		////		}

		////		NavigationBar.StandardAppearance.TitleTextAttributes = titleTextAttributes;
		////		NavigationBar.StandardAppearance.LargeTitleTextAttributes = largeTitleTextAttributes;

		////		if (NavigationBar.ScrollEdgeAppearance != null)
		////		{
		////			NavigationBar.ScrollEdgeAppearance.TitleTextAttributes = titleTextAttributes;
		////			NavigationBar.ScrollEdgeAppearance.LargeTitleTextAttributes = largeTitleTextAttributes;
		////		}
		////	}
		////	else
		////	{
		////		NavigationBar.TitleTextAttributes = titleTextAttributes;

		////		if (PlatformVersion.IsAtLeast(11))
		////			NavigationBar.LargeTitleTextAttributes = largeTitleTextAttributes;
		////	}

		////	//// set Tint color (i. e. Back Button arrow and Text)
		////	//var iconColor = Current != null ? NavigationView.GetIconColor(Current) : null;
		////	//if (iconColor == null)
		////	//	iconColor = barTextColor;

		////	//NavigationBar.TintColor = iconColor == null || NavPage.OnThisPlatform().GetStatusBarTextColorMode() == StatusBarTextColorMode.DoNotAdjust
		////	//	? UINavigationBar.Appearance.TintColor
		////	//	: iconColor.ToUIColor();
		////}


		//protected override void ConnectHandler(UIView nativeView)
		//{
		//	base.ConnectHandler(nativeView);

		//	if (VirtualView == null)
		//		return;

		//	VirtualView.PushRequested += OnPushRequested;
		//	VirtualView.PopRequested += OnPopRequested;
		//	_controlsNavigationController.LoadPages(this.MauiContext);

		//	//VirtualView.PopToRootRequested += OnPopToRootRequested;
		//	//VirtualView.RemovePageRequested += OnRemovedPageRequested;
		//	//VirtualView.InsertPageBeforeRequested += OnInsertPageBeforeRequested;
		//}

		//protected override void DisconnectHandler(UIView nativeView)
		//{
		//	base.DisconnectHandler(nativeView);

		//	if (VirtualView == null)
		//		return;

		//	VirtualView.PushRequested -= OnPushRequested;
		//	VirtualView.PopRequested -= OnPopRequested;
		//	//VirtualView.PopToRootRequested -= OnPopToRootRequested;
		//	//VirtualView.RemovePageRequested -= OnRemovedPageRequested;
		//	//VirtualView.InsertPageBeforeRequested -= OnInsertPageBeforeRequested;
		//}

		//void OnPushRequested(object? sender, NavigationRequestedEventArgs e)
		//{
		//	_controlsNavigationController?
		//		.OnPushRequested(e, this.MauiContext);
		//}

		//void OnPopRequested(object? sender, NavigationRequestedEventArgs e)
		//{
		//	_controlsNavigationController?
		//		.OnPopRequestedAsync(e)
		//		.FireAndForget((exc) => Log.Warning(nameof(NavigationView), $"{exc}"));
		//}

		//internal void SendPopping(Task popTask)
		//{
		//	if (VirtualView == null)
		//		return;

		//	VirtualView.PopAsyncInner(false, true, true)
		//		.FireAndForget((exc) => Log.Warning(nameof(NavigationView), $"{exc}"));
		//}
	}
}
