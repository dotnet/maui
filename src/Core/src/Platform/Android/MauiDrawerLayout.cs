using System;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Views;
using AndroidX.AppCompat.Widget;
using AndroidX.DrawerLayout.Widget;
using AView = Android.Views.View;

namespace Microsoft.Maui.Platform
{
    /// <summary>
    /// Shared DrawerLayout wrapper for FlyoutViewHandler and ShellHandler.
    /// Provides common flyout functionality following Google Material Design guidelines.
    /// </summary>
#pragma warning disable RS0016 // Add public types and members to the declared API
    public class MauiDrawerLayout : DrawerLayout
    {
        AView? _flyoutView;
        AView? _contentView;
        LinearLayoutCompat? _sideBySideView;
        FlyoutBehavior _currentBehavior = FlyoutBehavior.Flyout;
        bool _gestureEnabled = true;
        double _flyoutWidth = -1;
        double _defaultFlyoutWidth;
        FlyoutLayoutMode _layoutMode = FlyoutLayoutMode.Flyout;
        bool _isListening;

        /// <summary>
        /// Defines how the flyout is laid out when in Locked behavior.
        /// </summary>
        public enum FlyoutLayoutMode
        {
            /// <summary>
            /// Standard flyout mode - drawer slides over content.
            /// </summary>
            Flyout,

            /// <summary>
            /// Side-by-side mode - flyout and content are placed horizontally.
            /// Used by FlyoutViewHandler for tablet locked mode.
            /// </summary>
            SideBySide,

            /// <summary>
            /// Padding mode - content is padded to make room for flyout.
            /// Used by ShellFlyoutRenderer for locked mode.
            /// </summary>
            Padding
        }

        #region Constructors

        public MauiDrawerLayout(Context context) : base(context)
        {
            Initialize(context);
        }

        public MauiDrawerLayout(Context context, IAttributeSet? attrs) : base(context, attrs)
        {
            Initialize(context);
        }

        public MauiDrawerLayout(Context context, IAttributeSet? attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
            Initialize(context);
        }

        protected MauiDrawerLayout(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        void Initialize(Context context)
        {
            _defaultFlyoutWidth = CalculateDefaultFlyoutWidth(context);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the current flyout view.
        /// </summary>
        public AView? FlyoutView => _flyoutView;

        /// <summary>
        /// Gets the current content view.
        /// </summary>
        public AView? ContentView => _contentView;

        /// <summary>
        /// Gets whether the flyout is currently open.
        /// </summary>
        public bool IsFlyoutOpen => _flyoutView != null && IsDrawerOpen(_flyoutView);

        /// <summary>
        /// Gets or sets the flyout width. -1 for default width.
        /// </summary>
        public double FlyoutWidth
        {
            get => _flyoutWidth == -1 ? _defaultFlyoutWidth : _flyoutWidth;
            set
            {
                _flyoutWidth = value;
                UpdateFlyoutViewWidth();
            }
        }

        /// <summary>
        /// Gets the default flyout width calculated from Google design guidelines.
        /// </summary>
        public double DefaultFlyoutWidth => _defaultFlyoutWidth;

        /// <summary>
        /// Gets or sets the flyout layout mode for locked behavior.
        /// </summary>
        public FlyoutLayoutMode FlyoutLayoutModeValue
        {
            get => _layoutMode;
            set
            {
                if (_layoutMode != value)
                {
                    _layoutMode = value;
                    if (_currentBehavior == FlyoutBehavior.Locked)
                    {
                        UpdateLayout();
                    }
                }
            }
        }

        /// <summary>
        /// Gets the current flyout behavior.
        /// </summary>
        public FlyoutBehavior CurrentBehavior => _currentBehavior;

        /// <summary>
        /// Gets whether gesture-based opening is enabled.
        /// </summary>
        public bool IsGestureEnabled => _gestureEnabled;

        #endregion

        #region Events

        /// <summary>
        /// Raised when the flyout presented state changes.
        /// </summary>
        public event Action<bool>? OnPresentedChanged;

        /// <summary>
        /// Raised when the drawer slides.
        /// </summary>
        public event Action<float>? OnSlide;

        #endregion

        #region Public Methods

        /// <summary>
        /// Sets the content view (the main content behind the flyout).
        /// </summary>
        public void SetContentView(AView contentView)
        {
            if (_contentView == contentView)
            {
                return;
            }

            _contentView?.RemoveFromParent();
            _contentView = contentView;

            UpdateLayout();
        }

        /// <summary>
        /// Sets the flyout view (the drawer content).
        /// </summary>
        public void SetFlyoutView(AView flyoutView)
        {
            if (_flyoutView == flyoutView)
            {
                return;
            }

            _flyoutView?.RemoveFromParent();
            _flyoutView = flyoutView;

            UpdateLayout();
            EnsureListening();
        }

        /// <summary>
        /// Opens the flyout drawer.
        /// </summary>
        public void OpenFlyout(bool animate = true)
        {
            if (_flyoutView is null || _flyoutView.Parent != this)
            {
                return;
            }

            if (animate)
            {
                OpenDrawer(_flyoutView);
            }
            else
            {
                OpenDrawer(_flyoutView, false);
            }
        }

        /// <summary>
        /// Closes the flyout drawer.
        /// </summary>
        public void CloseFlyout(bool animate = true)
        {
            if (_flyoutView is null)
            {
                CloseDrawers();
                return;
            }

            if (_flyoutView.Parent != this)
            {
                return;
            }

            if (animate)
            {
                CloseDrawer(_flyoutView);
            }
            else
            {
                CloseDrawer(_flyoutView, false);
            }
        }

        /// <summary>
        /// Sets the flyout behavior (Disabled/Flyout/Locked).
        /// </summary>
        public void SetBehavior(FlyoutBehavior behavior)
        {
            bool closeAfterUpdate = (behavior == FlyoutBehavior.Flyout && _currentBehavior == FlyoutBehavior.Locked);
            _currentBehavior = behavior;

            UpdateLayout();
            UpdateLockMode();

            if (closeAfterUpdate && _flyoutView != null && _flyoutView.Parent == this)
            {
                CloseDrawer(_flyoutView, false);
            }
        }

        /// <summary>
        /// Sets whether gesture-based flyout opening is enabled.
        /// </summary>
        public void SetGestureEnabled(bool enabled)
        {
            _gestureEnabled = enabled;

            if (_currentBehavior == FlyoutBehavior.Flyout)
            {
                UpdateLockMode();
            }
        }

        /// <summary>
        /// Gets the left padding needed for content when flyout is locked open (padding mode).
        /// </summary>
        public int GetLockedContentPadding()
        {
            return _currentBehavior == FlyoutBehavior.Locked && _layoutMode == FlyoutLayoutMode.Padding
                ? (int)FlyoutWidth
                : 0;
        }

        /// <summary>
        /// Disconnects event listeners and cleans up.
        /// </summary>
        public virtual void Disconnect()
        {
            if (_isListening)
            {
                DrawerStateChanged -= OnDrawerStateChanged;
                DrawerOpened -= OnDrawerOpened;
                DrawerClosed -= OnDrawerClosed;
                DrawerSlide -= OnDrawerSlide;
                _isListening = false;
            }

            OnPresentedChanged = null;
            OnSlide = null;
        }

        #endregion

        #region Layout Methods

        void UpdateLayout()
        {
            if (_flyoutView is null || _contentView is null)
            {
                return;
            }

            if (_currentBehavior == FlyoutBehavior.Locked && _layoutMode == FlyoutLayoutMode.SideBySide)
            {
                LayoutSideBySide();
            }
            else
            {
                LayoutAsFlyout();
            }
        }

        /// <summary>
        /// Layouts the flyout and content side by side (for tablets in locked mode).
        /// </summary>
        protected virtual void LayoutSideBySide()
        {
            if (_flyoutView is null || _contentView is null)
            {
                return;
            }

            // Create side-by-side container if needed
            if (_sideBySideView is null)
            {
                _sideBySideView = new LinearLayoutCompat(Context!)
                {
                    Orientation = LinearLayoutCompat.Horizontal,
                    LayoutParameters = new LayoutParams(
                        LayoutParams.MatchParent,
                        LayoutParams.MatchParent)
                };
            }

            // Add content to side-by-side view
            if (_contentView.Parent != _sideBySideView)
            {
                _contentView.RemoveFromParent();

                var contentParams = new LinearLayoutCompat.LayoutParams(
                    LinearLayoutCompat.LayoutParams.MatchParent,
                    LinearLayoutCompat.LayoutParams.MatchParent,
                    1);  // weight 1 to fill remaining space

                _sideBySideView.AddView(_contentView, contentParams);
            }

            // Add flyout to side-by-side view
            if (_flyoutView.Parent != _sideBySideView)
            {
                _flyoutView.Visibility = ViewStates.Visible;
                _flyoutView.RemoveFromParent();

                var flyoutParams = new LinearLayoutCompat.LayoutParams(
                    (int)FlyoutWidth,
                    LinearLayoutCompat.LayoutParams.MatchParent,
                    0);  // weight 0 for fixed width

                _sideBySideView.AddView(_flyoutView, 0, flyoutParams);  // Add at index 0 (left)
            }

            // Add side-by-side view to drawer
            if (_sideBySideView.Parent != this)
            {
                AddView(_sideBySideView);
            }
        }

        /// <summary>
        /// Layouts as a standard flyout (drawer over content).
        /// </summary>
        protected virtual void LayoutAsFlyout()
        {
            if (_flyoutView is null || _contentView is null)
            {
                return;
            }

            // Remove side-by-side view
            _sideBySideView?.RemoveAllViews();
            _sideBySideView?.RemoveFromParent();

            // Add content to drawer
            if (_contentView.Parent != this)
            {
                _contentView.RemoveFromParent();

                var contentParams = new LayoutParams(
                    LayoutParams.MatchParent,
                    LayoutParams.MatchParent);

                AddView(_contentView, 0, contentParams);
            }

            // Add flyout to drawer (must be after content for proper gesture handling)
            if (_flyoutView.Parent != this)
            {
                _flyoutView.RemoveFromParent();

                var flyoutParams = new LayoutParams(
                    (int)FlyoutWidth,
                    LayoutParams.MatchParent,
                    (int)GravityFlags.Start);

                AddView(_flyoutView, flyoutParams);
            }
        }

        #endregion

        #region Lock Mode

        void UpdateLockMode()
        {
            int lockMode = _currentBehavior switch
            {
                FlyoutBehavior.Disabled => LockModeLockedClosed,
                FlyoutBehavior.Locked => _layoutMode == FlyoutLayoutMode.SideBySide
                    ? LockModeLockedClosed  // In side-by-side, drawer is not used
                    : LockModeLockedOpen,
                FlyoutBehavior.Flyout => _gestureEnabled
                    ? LockModeUnlocked
                    : LockModeLockedClosed,
                _ => LockModeUnlocked
            };

            SetDrawerLockMode(lockMode);

            if (_currentBehavior == FlyoutBehavior.Disabled)
            {
                CloseDrawers();
            }
        }

        #endregion

        #region Event Handling

        void EnsureListening()
        {
            if (_isListening)
            {
                return;
            }

            DrawerStateChanged += OnDrawerStateChanged;
            DrawerOpened += OnDrawerOpened;
            DrawerClosed += OnDrawerClosed;
            DrawerSlide += OnDrawerSlide;
            _isListening = true;
        }

        void OnDrawerStateChanged(object? sender, DrawerStateChangedEventArgs e)
        {
            if (_flyoutView is null)
            {
                return;
            }

            if (StateIdle == e.NewState)
            {
                var isOpen = IsDrawerOpen(_flyoutView);
                OnPresentedChanged?.Invoke(isOpen);
            }
        }

        void OnDrawerOpened(object? sender, DrawerOpenedEventArgs e)
        {
            OnPresentedChanged?.Invoke(true);
        }

        void OnDrawerClosed(object? sender, DrawerClosedEventArgs e)
        {
            OnPresentedChanged?.Invoke(false);
        }

        void OnDrawerSlide(object? sender, DrawerSlideEventArgs e)
        {
            OnSlide?.Invoke(e.SlideOffset);
        }

        #endregion

        #region Width Calculation

        void UpdateFlyoutViewWidth()
        {
            if (_flyoutView?.LayoutParameters is null)
            {
                return;
            }

            _flyoutView.LayoutParameters.Width = (int)FlyoutWidth;
            _flyoutView.RequestLayout();
        }

        /// <summary>
        /// Calculates the default flyout width based on Google Material Design guidelines.
        /// </summary>
        /// <remarks>
        /// The right edge of the drawer should be Max(56dp, actionBarSize) from the right edge.
        /// Maximum width is 6 * actionBarSize.
        /// </remarks>
        static double CalculateDefaultFlyoutWidth(Context context)
        {
            var metrics = context.Resources?.DisplayMetrics;
            if (metrics is null)
            {
                return 300; // fallback
            }

            var width = Math.Min(metrics.WidthPixels, metrics.HeightPixels);
            var actionBarHeight = (int)context.GetActionBarHeight();

            width -= actionBarHeight;

            var maxWidth = actionBarHeight * 6;
            width = Math.Min(width, maxWidth);

            return width;
        }

        #endregion

        #region Dispose

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Disconnect();
            }

            base.Dispose(disposing);
        }

        #endregion
    }
}
