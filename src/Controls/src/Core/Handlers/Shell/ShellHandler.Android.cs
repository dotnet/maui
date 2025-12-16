#nullable disable
#if ANDROID
using System;
using Android.Content;
using Android.Views;
using Android.Widget;
using AndroidX.DrawerLayout.Widget;
using AndroidX.Fragment.App;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.Platform.Compatibility;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using AView = Android.Views.View;
using AToolbar = AndroidX.AppCompat.Widget.Toolbar;
using LP = Android.Views.ViewGroup.LayoutParams;

#pragma warning disable RS0016 // Add public types and members to the declared API

namespace Microsoft.Maui.Controls.Handlers
{
    public partial class ShellHandler : ViewHandler<Shell, AView>, IShellContext, IAppearanceObserver
    {
        DrawerLayout _drawerLayout;
        IShellFlyoutRenderer _flyoutRenderer;
        FrameLayout _frameLayout;
        IShellItemRenderer _currentShellItemRenderer;

        public static PropertyMapper<Shell, ShellHandler> Mapper =
            new PropertyMapper<Shell, ShellHandler>(ElementMapper)
            {
                [nameof(Shell.CurrentItem)] = MapCurrentItem,
                [nameof(Shell.FlyoutIsPresented)] = MapFlyoutIsPresented,
                [nameof(Shell.FlyoutBehavior)] = MapFlyoutBehavior,
                [nameof(Shell.FlyoutWidth)] = MapFlyoutWidth,
                [nameof(Shell.FlowDirection)] = MapFlowDirection,
                [nameof(Shell.FlyoutBackdrop)] = MapFlyoutBackdrop,
                [nameof(Shell.FlyoutBackground)] = MapFlyoutBackground,
                [nameof(Shell.FlyoutBackgroundColor)] = MapFlyoutBackground,
                [nameof(Shell.FlyoutBackgroundImage)] = MapFlyoutBackgroundImage,
                [nameof(Shell.FlyoutBackgroundImageAspect)] = MapFlyoutBackgroundImage,
                [nameof(Shell.FlyoutHeader)] = MapFlyoutHeader,
                [nameof(Shell.FlyoutHeaderTemplate)] = MapFlyoutHeader,
                [nameof(Shell.FlyoutFooter)] = MapFlyoutFooter,
                [nameof(Shell.FlyoutFooterTemplate)] = MapFlyoutFooter,
                [nameof(Shell.FlyoutHeaderBehavior)] = MapFlyoutHeaderBehavior,
                [nameof(Shell.FlyoutVerticalScrollMode)] = MapFlyoutVerticalScrollMode,
                [nameof(Shell.FlyoutContent)] = MapFlyoutContent,
                [nameof(Shell.FlyoutContentTemplate)] = MapFlyoutContent,
            };

        public static CommandMapper<Shell, ShellHandler> CommandMapper =
            new CommandMapper<Shell, ShellHandler>(ElementCommandMapper);

        public ShellHandler() : base(Mapper, CommandMapper)
        {
        }

        protected override AView CreatePlatformView()
        {
            // Create the flyout renderer (which IS the DrawerLayout)
            _flyoutRenderer = CreateShellFlyoutRenderer();
            _drawerLayout = _flyoutRenderer.AndroidView as DrawerLayout;

            // Create the content frame that will host shell items
            _frameLayout = new FrameLayout(Context)
            {
                LayoutParameters = new LP(LP.MatchParent, LP.MatchParent),
                Id = AView.GenerateViewId(),
            };

            // Attach the flyout - this adds the content to the drawer
            _flyoutRenderer.AttachFlyout(this, _frameLayout);

            return _drawerLayout;
        }

        protected override void ConnectHandler(AView platformView)
        {
            base.ConnectHandler(platformView);

            VirtualView.PropertyChanged += OnShellPropertyChanged;

            // Add appearance observer similar to ShellRenderer
            ((IShellController)VirtualView).AddAppearanceObserver(this, VirtualView);

            // Initialize with current item if it exists
            if (VirtualView.CurrentItem != null)
            {
                SwitchToItem(VirtualView.CurrentItem, animate: false);
            }
        }

        protected override void DisconnectHandler(AView platformView)
        {
            if (VirtualView is not null)
            {
                VirtualView.PropertyChanged -= OnShellPropertyChanged;
                ((IShellController)VirtualView).RemoveAppearanceObserver(this);
            }

            _currentShellItemRenderer?.Dispose();
            _currentShellItemRenderer = null;

            if (_flyoutRenderer is IDisposable disposable)
            {
                disposable.Dispose();
            }

            _flyoutRenderer = null;

            base.DisconnectHandler(platformView);
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

            var fragmentManager = VirtualView.FindMauiContext()?.GetFragmentManager();
            if (fragmentManager is null || _frameLayout is null)
            {
                return;
            }

            var previousRenderer = _currentShellItemRenderer;

            _currentShellItemRenderer = CreateShellItemRenderer(newItem);
            _currentShellItemRenderer.ShellItem = newItem;

            var fragment = _currentShellItemRenderer.Fragment;
            var transaction = fragmentManager.BeginTransactionEx();

            if (animate)
            {
                transaction.SetTransitionEx((int)global::Android.App.FragmentTransit.FragmentOpen);
            }

            transaction.ReplaceEx(_frameLayout.Id, fragment);
            transaction.SetReorderingAllowedEx(true);
            transaction.CommitAllowingStateLossEx();

            if (previousRenderer != null)
            {
                void OnDestroyed(object sender, EventArgs args)
                {
                    previousRenderer.Destroyed -= OnDestroyed;
                    previousRenderer = null;
                }
                previousRenderer.Destroyed += OnDestroyed;
            }
        }

        public static void MapCurrentItem(ShellHandler handler, Shell shell)
        {
            handler.SwitchToItem(shell.CurrentItem, animate: true);
        }

        public static void MapFlyoutIsPresented(ShellHandler handler, Shell shell)
        {
            // Update drawer state directly
            if (handler._drawerLayout != null)
            {
                // Check if the drawer actually has a drawer view before manipulating it
                // TabBar shells don't have drawer views, only FlyoutItem shells do
                var drawerView = handler._drawerLayout.GetChildAt(1);
                if (drawerView != null)
                {
                    if (shell.FlyoutIsPresented)
                        handler._drawerLayout.OpenDrawer((int)GravityFlags.Start);
                    else
                        handler._drawerLayout.CloseDrawer((int)GravityFlags.Start);
                }
            }
        }

        public static void MapFlyoutBehavior(ShellHandler handler, Shell shell)
        {
            // Update flyout behavior (drawer lock mode)
            if (handler._drawerLayout != null)
            {
                var behavior = (shell as IFlyoutView).FlyoutBehavior;
                var lockMode = behavior == FlyoutBehavior.Locked
                    ? DrawerLayout.LockModeLockedClosed
                    : DrawerLayout.LockModeUnlocked;
                handler._drawerLayout.SetDrawerLockMode(lockMode);
            }
        }
        public static void MapFlyoutWidth(ShellHandler handler, Shell shell)
        {
            // Update the flyout width when it changes
            if (handler._flyoutRenderer is ShellFlyoutRenderer shellFlyoutRenderer)
            {
                shellFlyoutRenderer.UpdateFlyoutSize(shell.FlyoutWidth, -1); // -1 means keep current height
            }
        }
        public static void MapFlowDirection(ShellHandler handler, Shell shell)
        {
            // Update the flow direction when it changes
            if (handler._flyoutRenderer is ShellFlyoutRenderer shellFlyoutRenderer)
            {
                shellFlyoutRenderer.UpdateFlowDirection();
            }
        }

        public static void MapFlyoutBackdrop(ShellHandler handler, Shell shell)
        {
            // Update the flyout backdrop (scrim) when it changes
            if (handler._flyoutRenderer is ShellFlyoutRenderer shellFlyoutRenderer)
            {
                shellFlyoutRenderer.UpdateFlyoutBackdrop(shell.FlyoutBackdrop);
            }
        }
        public static void MapFlyoutBackground(ShellHandler handler, Shell shell)
        {
            // Update the flyout content renderer when background changes
            if (handler._flyoutRenderer is ShellFlyoutRenderer shellFlyoutRenderer)
            {
                // Access the flyout content renderer directly
                var contentRenderer = shellFlyoutRenderer.FlyoutContentRenderer;
                if (contentRenderer is ShellFlyoutTemplatedContentRenderer templatedRenderer)
                {
                    templatedRenderer.UpdateFlyoutBackground();
                }
            }
        }

        public static void MapFlyoutBackgroundImage(ShellHandler handler, Shell shell)
        {
            // Update the flyout content renderer when background image changes
            if (handler._flyoutRenderer is ShellFlyoutRenderer shellFlyoutRenderer)
            {
                var contentRenderer = shellFlyoutRenderer.FlyoutContentRenderer;
                if (contentRenderer is ShellFlyoutTemplatedContentRenderer templatedRenderer)
                {
                    templatedRenderer.UpdateFlyoutBackground();
                }
            }
        }

        public static void MapFlyoutHeader(ShellHandler handler, Shell shell)
        {
            // Update the flyout header when it changes
            if (handler._flyoutRenderer is ShellFlyoutRenderer shellFlyoutRenderer)
            {
                var contentRenderer = shellFlyoutRenderer.FlyoutContentRenderer;
                if (contentRenderer is ShellFlyoutTemplatedContentRenderer templatedRenderer)
                {
                    templatedRenderer.UpdateFlyoutHeader();
                }
            }
        }

        public static void MapFlyoutFooter(ShellHandler handler, Shell shell)
        {
            // Update the flyout footer when it changes
            if (handler._flyoutRenderer is ShellFlyoutRenderer shellFlyoutRenderer)
            {
                var contentRenderer = shellFlyoutRenderer.FlyoutContentRenderer;
                if (contentRenderer is ShellFlyoutTemplatedContentRenderer templatedRenderer)
                {
                    templatedRenderer.UpdateFlyoutFooter();
                }
            }
        }

        public static void MapFlyoutHeaderBehavior(ShellHandler handler, Shell shell)
        {
            // Update the flyout header behavior when it changes
            if (handler._flyoutRenderer is ShellFlyoutRenderer shellFlyoutRenderer)
            {
                var contentRenderer = shellFlyoutRenderer.FlyoutContentRenderer;
                if (contentRenderer is ShellFlyoutTemplatedContentRenderer templatedRenderer)
                {
                    templatedRenderer.UpdateFlyoutHeaderBehavior();
                }
            }
        }

        public static void MapFlyoutVerticalScrollMode(ShellHandler handler, Shell shell)
        {
            // Update the flyout vertical scroll mode when it changes
            if (handler._flyoutRenderer is ShellFlyoutRenderer shellFlyoutRenderer)
            {
                var contentRenderer = shellFlyoutRenderer.FlyoutContentRenderer;
                if (contentRenderer is ShellFlyoutTemplatedContentRenderer templatedRenderer)
                {
                    templatedRenderer.UpdateVerticalScrollMode();
                }
            }
        }

        public static void MapFlyoutContent(ShellHandler handler, Shell shell)
        {
            // Update the flyout content when it changes
            if (handler._flyoutRenderer is ShellFlyoutRenderer shellFlyoutRenderer)
            {
                var contentRenderer = shellFlyoutRenderer.FlyoutContentRenderer;
                if (contentRenderer is ShellFlyoutTemplatedContentRenderer templatedRenderer)
                {
                    templatedRenderer.UpdateFlyoutContent();
                }
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

        protected virtual IShellFlyoutRenderer CreateShellFlyoutRenderer()
        {
            return new ShellFlyoutRenderer(this, Context);
        }

        #region IShellContext Implementation

        Context IShellContext.AndroidContext => Context;
        DrawerLayout IShellContext.CurrentDrawerLayout => _drawerLayout;
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
            return new ShellToolbarTracker(this, toolbar, _drawerLayout);
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
            // Appearance changes handled by appearance trackers
        }

        #endregion
    }
}
#endif
