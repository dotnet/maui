using System;
using Android.App;
using Android.Views;
using AndroidX.Core.Graphics;
using AndroidX.Core.View;
using AndroidX.Window.Layout;
using Google.Android.Material.AppBar;
using AView = Android.Views.View;

namespace Microsoft.Maui.Handlers
{
	public partial class WindowHandler : ElementHandler<IWindow, Activity>
	{
		NavigationRootManager? _rootManager;

		protected override void ConnectHandler(Activity platformView)
		{
			base.ConnectHandler(platformView);

			UpdateVirtualViewFrame(platformView);
		}

		public static void MapTitle(IWindowHandler handler, IWindow window) =>
			handler.PlatformView.UpdateTitle(window);

		public static void MapContent(IWindowHandler handler, IWindow window)
		{
			_ = handler.MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			var rootView = CreateRootViewFromContent(handler, window);
			ViewCompat.SetOnApplyWindowInsetsListener(rootView, new WindowsListener());
			handler.PlatformView.SetContentView(rootView);
		}

		public static void MapX(IWindowHandler handler, IWindow view) =>
			handler.PlatformView?.UpdateX(view);

		public static void MapY(IWindowHandler handler, IWindow view) =>
			handler.PlatformView?.UpdateY(view);

		public static void MapWidth(IWindowHandler handler, IWindow view) =>
			handler.PlatformView?.UpdateWidth(view);

		public static void MapHeight(IWindowHandler handler, IWindow view) =>
			handler.PlatformView?.UpdateHeight(view);

		public static void MapToolbar(IWindowHandler handler, IWindow view)
		{
			if (view is IToolbarElement tb)
				ViewHandler.MapToolbar(handler, tb);
		}

		public static void MapRequestDisplayDensity(IWindowHandler handler, IWindow window, object? args)
		{
			if (args is DisplayDensityRequest request)
				request.SetResult(handler.PlatformView.GetDisplayDensity());
		}

		private protected override void OnConnectHandler(object platformView)
		{
			base.OnConnectHandler(platformView);

			_rootManager = MauiContext!.GetNavigationRootManager();
			_rootManager.RootViewChanged += OnRootViewChanged;
		}

		private protected override void OnDisconnectHandler(object platformView)
		{
			base.OnDisconnectHandler(platformView);

			DisconnectHandler(_rootManager);

			if (_rootManager != null)
				_rootManager.RootViewChanged -= OnRootViewChanged;
		}

		void OnRootViewChanged(object? sender, EventArgs e)
		{
			if (VirtualView.VisualDiagnosticsOverlay != null && _rootManager?.RootView is ViewGroup)
			{
				if (VirtualView.VisualDiagnosticsOverlay.IsPlatformViewInitialized)
					VirtualView.VisualDiagnosticsOverlay.Deinitialize();

				VirtualView.VisualDiagnosticsOverlay.Initialize();
			}
		}

		// This is here to try and ensure symmetry with disconnect code between test handler
		// and the real handler
		internal static void DisconnectHandler(NavigationRootManager? navigationRootManager)
		{
			navigationRootManager?.Disconnect();
		}

		internal static View? CreateRootViewFromContent(IWindowHandler handler, IWindow window)
		{
			_ = handler.MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			var rootManager = handler.MauiContext.GetNavigationRootManager();
			rootManager.Connect(window.Content);
			return rootManager.RootView;
		}

		void UpdateVirtualViewFrame(Activity activity)
		{
			var frame = activity.GetWindowFrame();
			VirtualView.FrameChanged(frame);
		}

		// Temporary workaround:
		// Android 15 / API 36 removed the prior opt‑out path for edge‑to‑edge
		// (legacy "edge to edge ignore" + decor fitting). This placeholder exists
		// so we can keep apps from regressing (content accidentally covered by
		// system bars) until a proper, unified edge‑to‑edge + system bar inset
		// configuration API is implemented in MAUI.
		//
		// NOTE:
		// - Keep this minimal.
		// - Will be replaced by the planned comprehensive window insets solution.
		// - Do not extend; add new logic to the forthcoming implementation instead.
		internal class WindowsListener : Java.Lang.Object, IOnApplyWindowInsetsListener
		{
			public WindowInsetsCompat? OnApplyWindowInsets(AView? v, WindowInsetsCompat? insets)
			{
				if (insets == null || v == null)
					return insets;

				var appBarLayout = v.FindViewById<AppBarLayout>(Resource.Id.navigationlayout_appbar);
				var systemBars = insets.GetInsets(WindowInsetsCompat.Type.SystemBars());
				var displayCutout = insets.GetInsets(WindowInsetsCompat.Type.DisplayCutout());

				var leftInset = Math.Max(systemBars?.Left ?? 0, displayCutout?.Left ?? 0);
				var topInset = Math.Max(systemBars?.Top ?? 0, displayCutout?.Top ?? 0);
				var rightInset = Math.Max(systemBars?.Right ?? 0, displayCutout?.Right ?? 0);
				var bottomInset = Math.Max(systemBars?.Bottom ?? 0, displayCutout?.Bottom ?? 0);

				// Apply top inset only to AppBarLayout to allow content behind it
				appBarLayout?.SetPadding(0, topInset, 0, 0);

				// Apply side and bottom insets to root view, but not top
				v.SetPadding(leftInset, 0, rightInset, bottomInset);

				// Create new insets with only top consumed
				var newSystemBars = Insets.Of(
					systemBars?.Left ?? 0,
					systemBars?.Top ?? 0,
					systemBars?.Right ?? 0,
					0
				) ?? Insets.None;

				// Create new insets with only top consumed
				var newDisplayCutout = Insets.Of(
					displayCutout?.Left ?? 0,
					displayCutout?.Top ?? 0,
					displayCutout?.Right ?? 0,
					0
				) ?? Insets.None;

				return new WindowInsetsCompat.Builder(insets)
					?.SetInsets(WindowInsetsCompat.Type.SystemBars(), newSystemBars)
					?.SetInsets(WindowInsetsCompat.Type.DisplayCutout(), newDisplayCutout)
					?.Build() ?? insets;
			}
		}
	}
}