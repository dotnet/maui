using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Views;
using AndroidX.CoordinatorLayout.Widget;
using AndroidX.Core.View;

namespace Microsoft.Maui.Platform
{
    /// <summary>
    /// Registry entry for tracking CoordinatorLayout instances and their associated listeners
    /// </summary>
    internal record CoordinatorLayoutEntry(WeakReference<MauiCoordinatorLayout> Layout, GlobalWindowInsetListener Listener);

    /// <summary>
    /// A CoordinatorLayout variant that owns its own <see cref="GlobalWindowInsetListener"/> instance.
    /// Children inside this hierarchy will preferentially attach to this local listener rather than the
    /// activity-wide global listener. First iteration only used for NavigationPage root layout.
    /// </summary>
    public class MauiCoordinatorLayout : CoordinatorLayout
    {
        static readonly List<CoordinatorLayoutEntry> _registeredListeners = new();
        static readonly object _lockObject = new();

        internal GlobalWindowInsetListener WindowInsetListener { get; }

        /// <summary>
        /// Finds the appropriate GlobalWindowInsetListener for a given view by checking
        /// if it's contained within any registered MauiCoordinatorLayout
        /// </summary>
        internal static GlobalWindowInsetListener? FindListenerForView(View view)
        {
            lock (_lockObject)
            {
                // Clean up any dead references first
                for (int i = _registeredListeners.Count - 1; i >= 0; i--)
                {
                    if (!_registeredListeners[i].Layout.TryGetTarget(out _))
                    {
                        _registeredListeners.RemoveAt(i);
                    }
                }

                // Find the listener for this view
                foreach (var entry in _registeredListeners)
                {
                    if (entry.Layout.TryGetTarget(out var layout) && IsViewContainedIn(view, layout))
                    {
                        return entry.Listener;
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// Registers this CoordinatorLayout's listener in the static registry
        /// </summary>
        void RegisterListener()
        {
            lock (_lockObject)
            {
                _registeredListeners.Add(new CoordinatorLayoutEntry(new WeakReference<MauiCoordinatorLayout>(this), WindowInsetListener));
            }
        }

        /// <summary>
        /// Unregisters this CoordinatorLayout's listener from the static registry
        /// </summary>
        void UnregisterListener()
        {
            lock (_lockObject)
            {
                for (int i = _registeredListeners.Count - 1; i >= 0; i--)
                {
                    if (_registeredListeners[i].Layout.TryGetTarget(out var layout) && layout == this)
                    {
                        _registeredListeners.RemoveAt(i);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Checks if a view is contained within the specified layout's hierarchy
        /// </summary>
        static bool IsViewContainedIn(View view, ViewGroup layout)
        {
            var parent = view.Parent;
            while (parent != null)
            {
                if (parent == layout)
                    return true;
                parent = parent.Parent;
            }
            return false;
        }

        void Initialize()
        {
            // Only run once even if multiple constructors chain
            if (_initialized)
                return;
            _initialized = true;
            ViewCompat.SetOnApplyWindowInsetsListener(this, WindowInsetListener);
            ViewCompat.SetWindowInsetsAnimationCallback(this, WindowInsetListener);
            RegisterListener();
        }

        public override void OnAttachedToWindow()
        {
            base.OnAttachedToWindow();
            // Ensure we're registered when attached to window
            RegisterListener();
        }

        public override void OnDetachedFromWindow()
        {
            base.OnDetachedFromWindow();
            // Unregister when detached from window
            UnregisterListener();
        }

        bool _initialized;

        public MauiCoordinatorLayout(Context context) : base(context)
        {
            WindowInsetListener = new GlobalWindowInsetListener();
            Initialize();
        }

        public MauiCoordinatorLayout(Context context, IAttributeSet? attrs) : base(context, attrs)
        {
            WindowInsetListener = new GlobalWindowInsetListener();
            Initialize();
        }

        public MauiCoordinatorLayout(Context context, IAttributeSet? attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
            WindowInsetListener = new GlobalWindowInsetListener();
            Initialize();
        }

        protected MauiCoordinatorLayout(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
            WindowInsetListener = new GlobalWindowInsetListener();
            Initialize();
        }
    }
}
