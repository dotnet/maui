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
using Microsoft.Maui.Graphics;

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

        // Track the current scrim brush from appearance observer (matches old ShellFlyoutRenderer pattern).
        // Initialized to Brush.Transparent because the appearance observer fires with null first.
        Brush _scrimBrush = Brush.Transparent;

        // Pending fragment transaction (from RunOrWaitForResume, same as FlyoutViewHandler)
        IDisposable _pendingFragment;

        public static PropertyMapper<Shell, ShellHandler> Mapper =
            new PropertyMapper<Shell, ShellHandler>(ElementMapper)
            {
                [nameof(Shell.CurrentItem)] = MapCurrentItem,
                [nameof(Shell.FlyoutIsPresented)] = MapFlyoutIsPresented,
                [nameof(Shell.FlyoutBehavior)] = MapFlyoutBehavior,
                [nameof(Shell.FlyoutWidth)] = MapFlyoutWidth,
                [nameof(Shell.FlyoutHeight)] = MapFlyoutHeight,
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

            // Use Padding layout mode to match old ShellFlyoutRenderer behavior.
            // In locked mode, the content is padded left by the flyout width
            // (the drawer stays open and content shifts right).
            drawerLayout.FlyoutLayoutModeValue = MauiDrawerLayout.FlyoutLayoutMode.Padding;

            // Create the content frame that will host shell items
            _contentFrame = new FrameLayout(Context)
            {
                LayoutParameters = new LP(LP.MatchParent, LP.MatchParent),
                Id = AView.GenerateViewId(),
            };

            // Add _contentFrame directly as a child so it's in the view hierarchy
            // immediately. MauiDrawerLayout.SetContentView + UpdateLayout requires both
            // content AND flyout views to be set before it parents the content frame.
            // But we need the content frame to be a child now so fragment transactions
            // can find it by ID when the FragmentManager processes them.
            drawerLayout.AddView(_contentFrame, new LP(LP.MatchParent, LP.MatchParent));

            // Also set it as the content view for MauiDrawerLayout's internal tracking
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

            _pendingFragment?.Dispose();
            _pendingFragment = null;

            var context = MauiContext?.Context;
            if (context is null || _contentFrame is null)
            {
                return;
            }

            var fragmentManager = MauiContext.GetFragmentManager();

            var previousRenderer = _currentShellItemRenderer;

            _currentShellItemRenderer = CreateShellItemRenderer(newItem);
            _currentShellItemRenderer.ShellItem = newItem;

            var fragment = _currentShellItemRenderer.Fragment;

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
                    .Replace(_contentFrame.Id, fragment)
                    .SetReorderingAllowed(true)
                    .CommitNow();
            });

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

        void OnPlatformViewAttachedToWindow(object sender, AView.ViewAttachedToWindowEventArgs e)
        {
            PlatformView.ViewAttachedToWindow -= OnPlatformViewAttachedToWindow;

            // Initial load: now that the view is in the Activity's hierarchy,
            // the FragmentManager can find _contentFrame by ID.
            if (VirtualView?.CurrentItem is not null && _contentFrame is not null)
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
