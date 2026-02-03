using System;
using System.Threading.Tasks;
using Android.App.Roles;
using Android.Runtime;
using Android.Views;
using AndroidX.AppCompat.Widget;
using AndroidX.CoordinatorLayout.Widget;
using AndroidX.Core.View;
using AndroidX.DrawerLayout.Widget;
using AndroidX.Fragment.App;
using AndroidX.Lifecycle;
using Microsoft.Maui.Platform;

#pragma warning disable RS0016 // Add public types and members to the declared API
namespace Microsoft.Maui.Handlers
{
	public partial class FlyoutViewHandler : ViewHandler<IFlyoutView, MauiDrawerLayout>
	{
		View? _flyoutView;
		const uint DefaultScrimColor = 0x99000000;
		View? _navigationRoot;
		ScopedFragment? _detailViewFragment;

		// MauiDrawerLayout provides: _sideBySideView, layout methods, lock modes
		MauiDrawerLayout MauiDrawerLayout => PlatformView;

		protected override MauiDrawerLayout CreatePlatformView()
		{
			var li = MauiContext?.GetLayoutInflater();
			_ = li ?? throw new InvalidOperationException($"LayoutInflater cannot be null");

			// Create MauiDrawerLayout instead of raw DrawerLayout
			var dl = new MauiDrawerLayout(Context);

			// Create navigation root from XML
			_navigationRoot = li.Inflate(Resource.Layout.navigationlayout, null)
				?? throw new InvalidOperationException($"Resource.Layout.navigationlayout missing");

			_navigationRoot.Id = View.GenerateViewId();

			// Set navigation root as content view in MauiDrawerLayout
			dl.SetContentView(_navigationRoot);

			return dl;
		}

		double FlyoutWidth
		{
			get
			{
				var width = VirtualView.FlyoutWidth;
				if (width == -1)
					width = LinearLayoutCompat.LayoutParams.MatchParent;
				else
					width = Context.ToPixels(width);

				return width;
			}
		}

		IDisposable? _pendingFragment;
		void UpdateDetailsFragmentView()
		{
			_pendingFragment?.Dispose();
			_pendingFragment = null;

			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			if (_detailViewFragment is not null &&
				_detailViewFragment?.DetailView == VirtualView.Detail &&
				!_detailViewFragment.IsDestroyed)
			{
				return;
			}

			var context = MauiContext.Context;
			if (context is null)
				return;

			if (VirtualView.Detail?.Handler is IPlatformViewHandler pvh)
				pvh.DisconnectHandler();

			var fragmentManager = MauiContext.GetFragmentManager();

			if (VirtualView.Detail is null)
			{
				if (_detailViewFragment is not null)
				{
					_pendingFragment =
						fragmentManager
							.RunOrWaitForResume(context, (fm) =>
							{
								if (_detailViewFragment is null)
								{
									return;
								}

								fm
									.BeginTransactionEx()
									.RemoveEx(_detailViewFragment)
									.SetReorderingAllowed(true)
									.Commit();
							});
				}
			}
			else
			{
				_pendingFragment =
					fragmentManager
						.RunOrWaitForResume(context, (fm) =>
						{
							_detailViewFragment = new ScopedFragment(VirtualView.Detail, MauiContext);
							fm
								.BeginTransaction()
								.Replace(Resource.Id.navigationlayout_content, _detailViewFragment)
								.SetReorderingAllowed(true)
								.Commit();
						});
			}
		}

		void UpdateDetail()
		{
			LayoutViews();
		}

		void UpdateFlyout()
		{
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");
			_ = VirtualView.Flyout.ToPlatform(MauiContext);

			var newFlyoutView = VirtualView.Flyout.ToPlatform();
			if (_flyoutView == newFlyoutView)
				return;

			_flyoutView?.RemoveFromParent();

			_flyoutView = newFlyoutView;
			if (_flyoutView == null)
				return;

			if (VirtualView.Flyout.Background == null && Context?.Theme != null)
			{
				var colors = Context.Theme.ObtainStyledAttributes(new[] { global::Android.Resource.Attribute.ColorBackground });
				_flyoutView.SetBackgroundColor(new global::Android.Graphics.Color(colors.GetColor(0, 0)));
			}

			// Use MauiDrawerLayout to set the flyout view
			MauiDrawerLayout.FlyoutWidth = FlyoutWidth;
			MauiDrawerLayout.SetFlyoutView(_flyoutView);

			// Set layout mode based on behavior
			UpdateFlyoutBehavior();
		}

		void LayoutViews()
		{
			// Layout is now handled by MauiDrawerLayout internally
			// Just ensure behavior is set correctly
			if (_flyoutView == null)
				return;

			MauiDrawerLayout.FlyoutLayoutModeValue = VirtualView.FlyoutBehavior == FlyoutBehavior.Locked
				? MauiDrawerLayout.FlyoutLayoutMode.SideBySide
				: MauiDrawerLayout.FlyoutLayoutMode.Flyout;

			// Update fragment view after layout
			UpdateDetailsFragmentView();

			// Update toolbar integration
			if (VirtualView is IToolbarElement te && te.Toolbar?.Handler is ToolbarHandler th)
			{
				th.SetupWithDrawerLayout(VirtualView.FlyoutBehavior == FlyoutBehavior.Locked ? null : MauiDrawerLayout);
			}
		}

		// LayoutSideBySide and LayoutAsFlyout are now handled by MauiDrawerLayout

		void UpdateIsPresented()
		{
			// Use MauiDrawerLayout's open/close methods
			if (VirtualView.IsPresented)
				MauiDrawerLayout.OpenFlyout();
			else
				MauiDrawerLayout.CloseFlyout();
		}

		void UpdateFlyoutBehavior()
		{
			var behavior = VirtualView.FlyoutBehavior;
			if (_detailViewFragment?.DetailView?.Handler?.PlatformView == null)
				return;

			// Important to create the layout views before setting the lock mode
			LayoutViews();

			// Use MauiDrawerLayout's SetBehavior method for consistent behavior handling
			MauiDrawerLayout.SetBehavior(behavior);

			// Also set gesture enabled state
			if (behavior == FlyoutBehavior.Flyout)
			{
				MauiDrawerLayout.SetGestureEnabled(VirtualView.IsGestureEnabled);
			}
		}

		protected override void ConnectHandler(MauiDrawerLayout platformView)
		{
			MauiWindowInsetListener.RegisterParentForChildViews(platformView);

			if (_navigationRoot is CoordinatorLayout cl)
			{
				MauiWindowInsetListener.SetupViewWithLocalListener(cl);
			}

			// Subscribe to MauiDrawerLayout events
			platformView.OnPresentedChanged += HandlePresentedChanged;
			platformView.ViewAttachedToWindow += DrawerLayoutAttached;
		}

		protected override void DisconnectHandler(MauiDrawerLayout platformView)
		{
			MauiWindowInsetListener.UnregisterView(platformView);
			if (_navigationRoot is CoordinatorLayout cl)
			{
				MauiWindowInsetListener.UnregisterView(cl);
				_navigationRoot = null;
			}

			// Unsubscribe from MauiDrawerLayout events
			platformView.OnPresentedChanged -= HandlePresentedChanged;
			platformView.ViewAttachedToWindow -= DrawerLayoutAttached;

			// Use MauiDrawerLayout's Disconnect method for cleanup
			platformView.Disconnect();

			if (VirtualView is IToolbarElement te)
			{
				te.Toolbar?.Handler?.DisconnectHandler();
			}
		}

		void DrawerLayoutAttached(object? sender, View.ViewAttachedToWindowEventArgs e)
		{
			UpdateDetailsFragmentView();
		}

		void HandlePresentedChanged(bool isPresented)
		{
			// Sync the virtual view's IsPresented property with the actual drawer state
			if (VirtualView.FlyoutBehavior == FlyoutBehavior.Flyout)
				VirtualView.IsPresented = isPresented;
		}

		public static void MapDetail(IFlyoutViewHandler handler, IFlyoutView flyoutView)
		{
			if (handler is FlyoutViewHandler platformHandler)
				platformHandler.UpdateDetail();
		}

		public static void MapFlyout(IFlyoutViewHandler handler, IFlyoutView flyoutView)
		{
			if (handler is FlyoutViewHandler platformHandler)
				platformHandler.UpdateFlyout();
		}

		public static void MapFlyoutBehavior(IFlyoutViewHandler handler, IFlyoutView flyoutView)
		{
			if (handler is FlyoutViewHandler platformHandler)
				platformHandler.UpdateFlyoutBehavior();
		}

		public static void MapIsPresented(IFlyoutViewHandler handler, IFlyoutView flyoutView)
		{
			if (handler is FlyoutViewHandler platformHandler)
				platformHandler.UpdateIsPresented();
		}

		public static void MapFlyoutWidth(IFlyoutViewHandler handler, IFlyoutView flyoutView)
		{
			if (handler is FlyoutViewHandler platformHandler)
			{
				var nativeFlyoutView = platformHandler._flyoutView;
				if (nativeFlyoutView?.LayoutParameters == null)
					return;

				nativeFlyoutView.LayoutParameters.Width = (int)platformHandler.FlyoutWidth;
			}
		}

		public static void MapToolbar(IFlyoutViewHandler handler, IFlyoutView view)
		{
			ViewHandler.MapToolbar(handler, view);

			if (handler is FlyoutViewHandler platformHandler &&
				handler.VirtualView.FlyoutBehavior == FlyoutBehavior.Flyout &&
				handler.VirtualView is IToolbarElement te &&
				te.Toolbar?.Handler is ToolbarHandler th)
			{
				th.SetupWithDrawerLayout(platformHandler.MauiDrawerLayout);
			}
		}

		public static void MapIsGestureEnabled(IFlyoutViewHandler handler, IFlyoutView view)
		{
			if (handler is FlyoutViewHandler platformHandler)
				platformHandler.UpdateFlyoutBehavior();
		}
	}
}
