using System;
using Android.App;
using Android.Views;
using AndroidX.Core.Graphics;
using AndroidX.Core.View;
using AndroidX.Fragment.App;
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
			int? _originalHeight;

			public WindowInsetsCompat? OnApplyWindowInsets(AView? rootView, WindowInsetsCompat? insets)
			{
				if (insets == null || rootView == null)
				{
					return insets;
				}

				var appbarInsets = insets.GetInsets(WindowInsetsCompat.Type.SystemBars() | WindowInsetsCompat.Type.DisplayCutout());

				if (appbarInsets is null)
				{
					return insets;
				}

				var appBarLayout = rootView.FindViewById<AppBarLayout>(Resource.Id.navigationlayout_appbar);
				var navigationLayout = rootView.FindViewById<FragmentContainerView>(Resource.Id.navigationlayout_content);

				var toolbar = appBarLayout?.GetChildAt(0) as MaterialToolbar;
				var systemBars = insets.GetInsets(WindowInsetsCompat.Type.SystemBars());
				var displayCutout = insets.GetInsets(WindowInsetsCompat.Type.DisplayCutout());

				// For toolbar content padding, we need to avoid both system bars and display cutouts
				var contentPaddingLeft = Math.Max(systemBars?.Left ?? 0, displayCutout?.Left ?? 0);
				var contentPaddingRight = Math.Max(systemBars?.Right ?? 0, displayCutout?.Right ?? 0);

				if (appBarLayout is not null && toolbar is not null)
				{
					// Store the original height on first call, then always calculate from that base
					var currentHeight = toolbar.LayoutParameters?.Height ?? 0;
					if (!_originalHeight.HasValue)
					{
						_originalHeight = currentHeight;
					}

					// Always calculate new height from the original height to prevent accumulation
					var newHeight = _originalHeight.Value + appbarInsets.Top;

					// Update the layout parameters to extend into status bar area
					if (toolbar.LayoutParameters != null)
					{
						toolbar.LayoutParameters.Height = newHeight;
						toolbar.RequestLayout();
					}

					// Apply left/right margins to AppBarLayout only for system bars, not display cutouts
					// This allows the background to extend behind cutouts while avoiding system bars
					if (appBarLayout.LayoutParameters is ViewGroup.MarginLayoutParams appBarLayoutParams && systemBars != null)
					{
						appBarLayoutParams.LeftMargin = systemBars.Left;
						appBarLayoutParams.RightMargin = systemBars.Right;
						appBarLayout.LayoutParameters = appBarLayoutParams;
					}



					var contentPaddingTop = appbarInsets.Top;
					// Apply padding to toolbar content to avoid both system bars and display cutouts
					toolbar.SetPadding(contentPaddingLeft, contentPaddingTop, contentPaddingRight, 0);

					// Clear AppBarLayout padding to allow toolbar to extend fully
					appBarLayout.SetPadding(0, 0, 0, 0);
				}

				// REMOVE MARGINS FROM NAVIGATION LAYOUT - Let it extend edge-to-edge
				if (navigationLayout?.LayoutParameters is ViewGroup.MarginLayoutParams navigationLayoutParams)
				{
					navigationLayoutParams.LeftMargin = 0;
					navigationLayoutParams.RightMargin = 0;
					navigationLayout.LayoutParameters = navigationLayoutParams;
				}

				// Apply only bottom insets to root view
				rootView.SetPadding(0, 0, 0, appbarInsets.Bottom);

				// Create new insets that preserve side insets for the navigation content
				// Only consume top insets since we handled them in the toolbar
				var newSystemBars = Insets.Of(
					systemBars?.Left ?? 0,
					0, // Top consumed by toolbar
					systemBars?.Right ?? 0,
					systemBars?.Bottom ?? 0
				) ?? Insets.None;

				var newDisplayCutout = Insets.Of(
					displayCutout?.Left ?? 0,
					0, // Top consumed by toolbar
					displayCutout?.Right ?? 0,
					displayCutout?.Bottom ?? 0
				) ?? Insets.None;

				// Return insets that the navigation content can use to respect side/bottom insets
				return new WindowInsetsCompat.Builder(insets)
					?.SetInsets(WindowInsetsCompat.Type.SystemBars(), newSystemBars)
					?.SetInsets(WindowInsetsCompat.Type.DisplayCutout(), newDisplayCutout)
					?.Build() ?? insets;
			}
		}
	}
}