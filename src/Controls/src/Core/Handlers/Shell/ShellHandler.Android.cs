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
    /// <summary>
    /// Shell handler that uses MauiDrawerLayout (same infrastructure as FlyoutViewHandler).
    /// This replaces the old ShellFlyoutRenderer-based approach.
    /// </summary>
    public partial class ShellHandler : ViewHandler<Shell, MauiDrawerLayout>, IShellContext, IAppearanceObserver
    {
        // MauiDrawerLayout is now the PlatformView (same as FlyoutViewHandler)
        MauiDrawerLayout MauiDrawerLayout => PlatformView;

        // Content frame that hosts ShellItem views
        FrameLayout _contentFrame;

        // Flyout content view (Shell's flyout menu)
        AView _flyoutContentView;
        IShellFlyoutContentRenderer _flyoutContentRenderer;

        // Current shell item renderer
        IShellItemRenderer _currentShellItemRenderer;

        // Track flyout behavior for IShellContext
        FlyoutBehavior _currentBehavior = FlyoutBehavior.Flyout;

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

        protected override MauiDrawerLayout CreatePlatformView()
        {
            // Create MauiDrawerLayout (same as FlyoutViewHandler)
            var drawerLayout = new MauiDrawerLayout(Context);

            // Create the content frame that will host shell items
            _contentFrame = new FrameLayout(Context)
            {
                LayoutParameters = new LP(LP.MatchParent, LP.MatchParent),
                Id = AView.GenerateViewId(),
            };

            // Set the content frame as the main content
            drawerLayout.SetContentView(_contentFrame);

            return drawerLayout;
        }

        protected override void ConnectHandler(MauiDrawerLayout platformView)
        {
            base.ConnectHandler(platformView);

            VirtualView.PropertyChanged += OnShellPropertyChanged;

            // Add appearance observer similar to ShellRenderer
            ((IShellController)VirtualView).AddAppearanceObserver(this, VirtualView);

            // Subscribe to drawer state changes
            platformView.OnPresentedChanged += OnFlyoutPresentedChanged;

            // Initialize flyout behavior
            var behavior = (VirtualView as IFlyoutView)?.FlyoutBehavior ?? FlyoutBehavior.Flyout;
            _currentBehavior = behavior;
            UpdateFlyoutBehaviorInternal(behavior);

            // Initialize with current item if it exists
            if (VirtualView.CurrentItem is not null)
            {
                SwitchToItem(VirtualView.CurrentItem, animate: false);
            }
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

            if (_flyoutContentRenderer is IDisposable disposable)
            {
                disposable.Dispose();
            }
            _flyoutContentRenderer = null;
            _flyoutContentView = null;

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

            var fragmentManager = VirtualView.FindMauiContext()?.GetFragmentManager();
            if (fragmentManager is null || _contentFrame is null)
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

            transaction.ReplaceEx(_contentFrame.Id, fragment);
            transaction.SetReorderingAllowedEx(true);
            transaction.CommitAllowingStateLossEx();

            // Force the fragment transaction to complete synchronously
            fragmentManager.ExecutePendingTransactions();

            if (previousRenderer is not null)
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
            if (behavior == FlyoutBehavior.Locked && _contentFrame is not null)
            {
                var padding = MauiDrawerLayout?.GetLockedContentPadding() ?? 0;
                _contentFrame.SetPadding(padding, _contentFrame.PaddingTop, _contentFrame.PaddingRight, _contentFrame.PaddingBottom);
            }
            else
            {
                _contentFrame?.SetPadding(0, _contentFrame.PaddingTop, _contentFrame.PaddingRight, _contentFrame.PaddingBottom);
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

        public static void MapFlowDirection(ShellHandler handler, Shell shell)
        {
            // Update the flow direction on the MauiDrawerLayout
            if (handler.MauiDrawerLayout is null)
            {
                return;
            }

            handler.MauiDrawerLayout.LayoutDirection = shell.FlowDirection.ToLayoutDirection();
        }

        public static void MapFlyoutBackdrop(ShellHandler handler, Shell shell)
        {
            // TODO: Implement scrim/backdrop for MauiDrawerLayout
            // For now, use default DrawerLayout scrim
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

        public static void MapFlyoutContent(ShellHandler handler, Shell shell)
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
            // Appearance changes handled by appearance trackers
        }

        #endregion
    }
}
#endif
