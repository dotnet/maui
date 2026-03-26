#nullable disable
#if ANDROID
using System;
using Android.Content;
using AndroidX.CoordinatorLayout.Widget;
using AndroidX.DrawerLayout.Widget;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.Platform.Compatibility;
using AView = Android.Views.View;
using AToolbar = AndroidX.AppCompat.Widget.Toolbar;
using LP = Android.Views.ViewGroup.LayoutParams;
using Microsoft.Maui.Graphics;
namespace Microsoft.Maui.Controls.Handlers
{
    /// <summary>
    /// Shell handler that uses MauiDrawerLayout (same infrastructure as FlyoutViewHandler).
    /// This replaces the old ShellFlyoutRenderer-based approach.
    /// </summary>
    public partial class ShellHandler : ViewHandler<Shell, MauiDrawerLayout>, IShellContext, IAppearanceObserver
    {
        // MauiDrawerLayout is now the PlatformView (same as FlyoutViewHandler)
        MauiDrawerLayout MauiDrawerLayout => PlatformView;

        // Navigation root (inflated navigationlayout.axml) — provides named slots
        // for content, toolbar, bottom tabs, and top tabs (same as FlyoutViewHandler).
        AView _navigationRoot;

        // Flyout content view (Shell's flyout menu)
        AView _flyoutContentView;
        IShellFlyoutContentRenderer _flyoutContentRenderer;

        // Current shell item renderer
        IShellItemRenderer _currentShellItemRenderer;

        // ShellItem handler — placed once in navigationlayout_content, reused
        // across ShellItem switches via SwitchToShellItem() (no fragment replacement).
        ShellItemHandler _shellItemHandler;

        // Track flyout behavior for IShellContext
        FlyoutBehavior _currentBehavior = FlyoutBehavior.Flyout;

        // Track the current scrim brush from appearance observer (matches old ShellFlyoutRenderer pattern).
        // Initialized to Brush.Transparent because the appearance observer fires with null first.
        Brush _scrimBrush = Brush.Transparent;

        // Pending fragment transaction (from RunOrWaitForResume, same as FlyoutViewHandler)
        IDisposable _pendingFragment;

        protected override MauiDrawerLayout CreatePlatformView()
        {
            // Create MauiDrawerLayout (same as FlyoutViewHandler)
            var drawerLayout = new MauiDrawerLayout(Context);

            // Use Padding layout mode to match old ShellFlyoutRenderer behavior.
            // In locked mode, the content is padded left by the flyout width
            // (the drawer stays open and content shifts right).
            drawerLayout.FlyoutLayoutModeValue = MauiDrawerLayout.FlyoutLayoutMode.Padding;

            // Inflate navigationlayout.axml — provides named slots for content,
            // toolbar, bottom tabs, and top tabs (same as FlyoutViewHandler pattern).
            // This enables Shell to use the same NRM infrastructure as non-Shell.
            var layoutInflater = MauiContext?.GetLayoutInflater()
                ?? throw new InvalidOperationException("LayoutInflater missing");
            _navigationRoot = layoutInflater.Inflate(Resource.Layout.navigationlayout, null)
                ?? throw new InvalidOperationException("navigationlayout inflation failed");

            // Add navigation root as content inside MauiDrawerLayout.
            // Fragment transactions target Resource.Id.navigationlayout_content
            // (a static ID from the layout) instead of a dynamic GenerateViewId().
            drawerLayout.AddView(_navigationRoot, new LP(LP.MatchParent, LP.MatchParent));

            // Set as content view for MauiDrawerLayout's internal tracking
            drawerLayout.SetContentView(_navigationRoot);

            return drawerLayout;
        }

        protected override void ConnectHandler(MauiDrawerLayout platformView)
        {
            base.ConnectHandler(platformView);

            // Window insets setup for the CoordinatorLayout (same as FlyoutViewHandler)
            if (_navigationRoot is CoordinatorLayout cl)
            {
                MauiWindowInsetListener.SetupViewWithLocalListener(cl);
            }

            VirtualView.PropertyChanged += OnShellPropertyChanged;

            // Add appearance observer similar to ShellRenderer
            ((IShellController)VirtualView).AddAppearanceObserver(this, VirtualView);

            // Subscribe to drawer state changes
            platformView.OnPresentedChanged += OnFlyoutPresentedChanged;

            // Handle initial item switch when view is attached to the window.
            // MapCurrentItem fires during SetVirtualView before the view is in the
            // Activity's hierarchy, so we defer the initial load to ViewAttachedToWindow
            // (same pattern as FlyoutViewHandler.DrawerLayoutAttached).
            platformView.ViewAttachedToWindow += OnPlatformViewAttachedToWindow;

            // Initialize flyout behavior
            var behavior = (VirtualView as IFlyoutView)?.FlyoutBehavior ?? FlyoutBehavior.Flyout;
            _currentBehavior = behavior;
            UpdateFlyoutBehaviorInternal(behavior);
        }

        protected override void DisconnectHandler(MauiDrawerLayout platformView)
        {
            if (VirtualView is not null)
            {
                VirtualView.PropertyChanged -= OnShellPropertyChanged;
                ((IShellController)VirtualView).RemoveAppearanceObserver(this);
            }

            platformView.OnPresentedChanged -= OnFlyoutPresentedChanged;

            _currentShellItemRenderer?.Dispose();
            _currentShellItemRenderer = null;
            _shellItemHandler = null;

            if (_flyoutContentRenderer is IDisposable disposable)
            {
                disposable.Dispose();
            }
            _flyoutContentRenderer = null;
            _flyoutContentView = null;

            // Clean up pending fragment transaction
            _pendingFragment?.Dispose();
            _pendingFragment = null;

            // Clean up ViewAttachedToWindow listener
            platformView.ViewAttachedToWindow -= OnPlatformViewAttachedToWindow;

            // Clean up window insets and navigation root
            if (_navigationRoot is CoordinatorLayout cl)
            {
                MauiWindowInsetListener.RemoveViewWithLocalListener(cl);
            }
            _navigationRoot = null;

            // Disconnect MauiDrawerLayout
            platformView.Disconnect();

            base.DisconnectHandler(platformView);
        }

        void OnFlyoutPresentedChanged(bool isPresented)
        {
            // Sync the Shell's FlyoutIsPresented property with actual drawer state
            if (_currentBehavior == FlyoutBehavior.Flyout)
            {
                VirtualView.SetValueFromRenderer(Shell.FlyoutIsPresentedProperty, isPresented);
            }
        }

        void OnShellPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // Property changes are handled via PropertyMapper
        }

        void SwitchToItem(ShellItem newItem, bool animate)
        {
            if (newItem is null)
            {
                return;
            }

            // Subsequent switches: reuse the permanent fragment, update handler data.
            // No fragment transaction — the wrapper fragment stays in navigationlayout_content.
            if (_shellItemHandler is not null)
            {
                _shellItemHandler.SwitchToShellItem(newItem);
                return;
            }

            // First switch: create handler + fragment, add to layout
            _pendingFragment?.Dispose();
            _pendingFragment = null;

            var context = MauiContext?.Context;

            if (context is null || _navigationRoot is null)
            {
                return;
            }

            var fragmentManager = MauiContext.GetFragmentManager();

            _currentShellItemRenderer = CreateShellItemRenderer(newItem);
            _currentShellItemRenderer.ShellItem = newItem;

            var fragment = _currentShellItemRenderer.Fragment;

            // Store handler reference for future switches.
            // After this fragment transaction, the wrapper fragment stays in
            // navigationlayout_content — subsequent ShellItem switches update
            // the handler's data instead of replacing the fragment.
            if (_currentShellItemRenderer is ShellItemHandlerAdapter adapter)
            {
                _shellItemHandler = adapter.GetHandler();
            }

            // Use RunOrWaitForResume for state-saved safety (same as FlyoutViewHandler),
            // but CommitNow() instead of Commit() because Shell's navigation flow requires
            // the fragment to be immediately in the hierarchy — page Appearing events
            // and NavigationRequested handlers fire synchronously after this returns.
            _pendingFragment = fragmentManager.RunOrWaitForResume(context, (fm) =>
            {
                var transaction = fm.BeginTransaction();

                if (animate)
                {
                    transaction.SetTransition((int)global::Android.App.FragmentTransit.FragmentOpen);
                }

                transaction
                    .Replace(Resource.Id.navigationlayout_content, fragment)
                    .SetReorderingAllowed(true)
                    .CommitNow();
            });
        }

        void OnPlatformViewAttachedToWindow(object sender, AView.ViewAttachedToWindowEventArgs e)
        {
            PlatformView.ViewAttachedToWindow -= OnPlatformViewAttachedToWindow;

            // Initial load: now that the view is in the Activity's hierarchy,
            // the FragmentManager can find navigationlayout_content by ID.
            if (VirtualView?.CurrentItem is not null && _navigationRoot is not null)
            {
                SwitchToItem(VirtualView.CurrentItem, animate: false);
            }
        }

        public static void MapCurrentItem(ShellHandler handler, Shell shell)
        {
            // During initial SetVirtualView, MapCurrentItem fires before the view is
            // in the Activity's hierarchy — skip it. The initial load is handled by
            // OnPlatformViewAttachedToWindow (same pattern as FlyoutViewHandler.DrawerLayoutAttached).
            if (!handler.PlatformView.IsAttachedToWindow)
            {
                return;
            }

            handler.SwitchToItem(shell.CurrentItem, animate: true);
        }

        public static void MapIsPresented(ShellHandler handler, Shell shell)
        {
            // Use MauiDrawerLayout's open/close methods
            if (handler.MauiDrawerLayout is not null)
            {
                if (shell.FlyoutIsPresented)
                {
                    handler.MauiDrawerLayout.OpenFlyout();
                }
                else
                {
                    handler.MauiDrawerLayout.CloseFlyout();
                }
            }
        }

        public static void MapFlyoutBehavior(ShellHandler handler, Shell shell)
        {
            var behavior = (shell as IFlyoutView).FlyoutBehavior;
            handler.UpdateFlyoutBehaviorInternal(behavior);
        }

        void UpdateFlyoutBehaviorInternal(FlyoutBehavior behavior)
        {
            _currentBehavior = behavior;

            // Ensure flyout content is added for non-disabled behaviors
            if (behavior != FlyoutBehavior.Disabled)
            {
                EnsureFlyoutContentCreated();
            }

            // Use MauiDrawerLayout's SetBehavior method
            MauiDrawerLayout?.SetBehavior(behavior);

            // Update content padding for locked mode
            if (behavior == FlyoutBehavior.Locked && _navigationRoot is not null)
            {
                var padding = MauiDrawerLayout?.GetLockedContentPadding() ?? 0;
                _navigationRoot.SetPadding(padding, _navigationRoot.PaddingTop, _navigationRoot.PaddingRight, _navigationRoot.PaddingBottom);
            }
            else
            {
                _navigationRoot?.SetPadding(0, _navigationRoot.PaddingTop, _navigationRoot.PaddingRight, _navigationRoot.PaddingBottom);
            }

            // Sync Shell property
            if (behavior == FlyoutBehavior.Locked)
            {
                VirtualView?.SetValueFromRenderer(Shell.FlyoutIsPresentedProperty, true);
            }
            else if (behavior == FlyoutBehavior.Disabled)
            {
                VirtualView?.SetValueFromRenderer(Shell.FlyoutIsPresentedProperty, false);
            }

            // Re-evaluate scrim for behavior change (locked = no scrim)
            if (VirtualView is not null)
            {
                UpdateScrim(_scrimBrush);
            }
        }

        void EnsureFlyoutContentCreated()
        {
            if (_flyoutContentView is not null)
                return;

            // Create the flyout content renderer
            _flyoutContentRenderer = CreateShellFlyoutContentRenderer();
            _flyoutContentView = _flyoutContentRenderer.AndroidView;

            if (_flyoutContentView is not null)
            {
                // Set flyout width from MauiDrawerLayout's default calculation
                var flyoutWidth = VirtualView.FlyoutWidth;
                if (flyoutWidth == -1)
                {
                    flyoutWidth = MauiDrawerLayout.DefaultFlyoutWidth;
                }
                else
                {
                    flyoutWidth = Context.ToPixels(flyoutWidth);
                }

                MauiDrawerLayout.FlyoutWidth = flyoutWidth;
                MauiDrawerLayout.SetFlyoutView(_flyoutContentView);
            }
        }

        public static void MapFlyoutWidth(ShellHandler handler, Shell shell)
        {
            // Update the flyout width using MauiDrawerLayout
            if (handler.MauiDrawerLayout is not null)
            {
                var width = shell.FlyoutWidth;
                if (width == -1)
                {
                    width = handler.MauiDrawerLayout.DefaultFlyoutWidth;
                }
                else
                {
                    width = handler.Context.ToPixels(width);
                }

                handler.MauiDrawerLayout.FlyoutWidth = width;

                // Update the flyout view's layout params
                if (handler._flyoutContentView?.LayoutParameters is not null)
                {
                    handler._flyoutContentView.LayoutParameters.Width = (int)width;
                    handler._flyoutContentView.RequestLayout();
                }
            }
        }

        public static void MapFlyoutHeight(ShellHandler handler, Shell shell)
        {
            if (handler._flyoutContentView?.LayoutParameters is null)
            {
                return;
            }

            var height = shell.FlyoutHeight;
            int heightPixels;
            if (height == -1)
            {
                heightPixels = LP.MatchParent;
            }
            else
            {
                heightPixels = (int)handler.Context.ToPixels(height);
            }

            handler._flyoutContentView.LayoutParameters.Height = heightPixels;
            handler._flyoutContentView.RequestLayout();
        }

        public static void MapFlowDirection(ShellHandler handler, Shell shell)
        {
            // Update the flow direction on the MauiDrawerLayout
            if (handler.MauiDrawerLayout is null)
            {
                return;
            }

            handler.MauiDrawerLayout.LayoutDirection = shell.FlowDirection.ToLayoutDirection();
        }

        const uint DefaultScrimColor = 0x99000000;

        public static void MapFlyoutBackdrop(ShellHandler handler, Shell shell)
        {
            if (handler.MauiDrawerLayout is null)
            {
                return;
            }

            handler._scrimBrush = shell.FlyoutBackdrop;
            handler.UpdateScrim(handler._scrimBrush);
        }

        void UpdateScrim(Brush backdrop)
        {
            if (MauiDrawerLayout is null)
            {
                return;
            }

            if (_currentBehavior == FlyoutBehavior.Locked)
            {
                MauiDrawerLayout.SetScrimColor(Colors.Transparent.ToPlatform());
                return;
            }

            if (backdrop is SolidColorBrush solidColor)
            {
                var backdropColor = solidColor.Color;
                if (backdropColor is null)
                {
                    unchecked
                    {
                        MauiDrawerLayout.SetScrimColor((int)DefaultScrimColor);
                    }
                }
                else
                {
                    MauiDrawerLayout.SetScrimColor(backdropColor.ToPlatform());
                }
            }
            else
            {
                // Default scrim for null/default/gradient brushes
                unchecked
                {
                    MauiDrawerLayout.SetScrimColor((int)DefaultScrimColor);
                }
            }
        }

        public static void MapFlyoutBackground(ShellHandler handler, Shell shell)
        {
            // Update the flyout content renderer when background changes
            if (handler._flyoutContentRenderer is ShellFlyoutTemplatedContentRenderer templatedRenderer)
            {
                templatedRenderer.UpdateFlyoutBackground();
            }
        }

        public static void MapFlyoutBackgroundImage(ShellHandler handler, Shell shell)
        {
            // Update the flyout content renderer when background image changes
            if (handler._flyoutContentRenderer is ShellFlyoutTemplatedContentRenderer templatedRenderer)
            {
                templatedRenderer.UpdateFlyoutBackground();
            }
        }

        public static void MapFlyoutHeader(ShellHandler handler, Shell shell)
        {
            // Update the flyout header when it changes
            if (handler._flyoutContentRenderer is ShellFlyoutTemplatedContentRenderer templatedRenderer)
            {
                templatedRenderer.UpdateFlyoutHeader();
            }
        }

        public static void MapFlyoutFooter(ShellHandler handler, Shell shell)
        {
            // Update the flyout footer when it changes
            if (handler._flyoutContentRenderer is ShellFlyoutTemplatedContentRenderer templatedRenderer)
            {
                templatedRenderer.UpdateFlyoutFooter();
            }
        }

        public static void MapFlyoutHeaderBehavior(ShellHandler handler, Shell shell)
        {
            // Update the flyout header behavior when it changes
            if (handler._flyoutContentRenderer is ShellFlyoutTemplatedContentRenderer templatedRenderer)
            {
                templatedRenderer.UpdateFlyoutHeaderBehavior();
            }
        }

        public static void MapFlyoutVerticalScrollMode(ShellHandler handler, Shell shell)
        {
            // Update the flyout vertical scroll mode when it changes
            if (handler._flyoutContentRenderer is ShellFlyoutTemplatedContentRenderer templatedRenderer)
            {
                templatedRenderer.UpdateVerticalScrollMode();
            }
        }

        public static void MapFlyout(ShellHandler handler, Shell shell)
        {
            // Update the flyout content when it changes
            if (handler._flyoutContentRenderer is ShellFlyoutTemplatedContentRenderer templatedRenderer)
            {
                templatedRenderer.UpdateFlyoutContent();
            }
        }

        protected virtual IShellItemRenderer CreateShellItemRenderer(ShellItem shellItem)
        {
            // Use the new handler-based architecture
            var handler = new ShellItemHandler();
            handler.SetMauiContext(MauiContext);
            handler.SetVirtualView(shellItem);

            // Wrap it in an adapter to make it compatible with IShellItemRenderer
            return new ShellItemHandlerAdapter(handler, MauiContext);
        }

        protected virtual IShellFlyoutContentRenderer CreateShellFlyoutContentRenderer()
        {
            return new ShellFlyoutTemplatedContentRenderer(this);
        }

        #region IShellContext Implementation

        Context IShellContext.AndroidContext => Context;

        // Return MauiDrawerLayout (which IS a DrawerLayout)
        DrawerLayout IShellContext.CurrentDrawerLayout => MauiDrawerLayout;

        Shell IShellContext.Shell => VirtualView;

        IShellObservableFragment IShellContext.CreateFragmentForPage(Page page)
        {
            return new ShellContentFragment(this, page);
        }

        IShellFlyoutContentRenderer IShellContext.CreateShellFlyoutContentRenderer()
        {
            return new ShellFlyoutTemplatedContentRenderer(this);
        }

        IShellItemRenderer IShellContext.CreateShellItemRenderer(ShellItem shellItem)
        {
            return CreateShellItemRenderer(shellItem);
        }

        IShellSectionRenderer IShellContext.CreateShellSectionRenderer(ShellSection shellSection)
        {
            var handler = new ShellSectionHandler();
            handler.SetMauiContext(MauiContext);
            handler.SetVirtualView(shellSection);

            return new ShellSectionHandlerAdapter(handler, MauiContext);
        }

        IShellToolbarTracker IShellContext.CreateTrackerForToolbar(AToolbar toolbar)
        {
            return new ShellToolbarTracker(this, toolbar, MauiDrawerLayout);
        }

        IShellToolbarAppearanceTracker IShellContext.CreateToolbarAppearanceTracker()
        {
            return new ShellToolbarAppearanceTracker(this);
        }

        IShellTabLayoutAppearanceTracker IShellContext.CreateTabLayoutAppearanceTracker(ShellSection shellSection)
        {
            return new ShellTabLayoutAppearanceTracker(this);
        }

        IShellBottomNavViewAppearanceTracker IShellContext.CreateBottomNavViewAppearanceTracker(ShellItem shellItem)
        {
            return new ShellBottomNavViewAppearanceTracker(this, shellItem);
        }

        #endregion

        #region IAppearanceObserver

        void IAppearanceObserver.OnAppearanceChanged(ShellAppearance appearance)
        {
            // Match old ShellFlyoutRenderer: null appearance → transparent scrim,
            // non-null appearance → use FlyoutBackdrop from appearance.
            if (appearance is null)
            {
                _scrimBrush = Brush.Transparent;
            }
            else
            {
                _scrimBrush = appearance.FlyoutBackdrop;
            }

            UpdateScrim(_scrimBrush);
        }

        #endregion
    }
}
#endif
