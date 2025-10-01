using System;
using Android.Content;
using Android.Runtime;
using Android.Util;
using AndroidX.CoordinatorLayout.Widget;
using AndroidX.Core.View;

namespace Microsoft.Maui.Platform
{
    /// <summary>
    /// A CoordinatorLayout variant that owns its own <see cref="GlobalWindowInsetListener"/> instance.
    /// Children inside this hierarchy will preferentially attach to this local listener rather than the
    /// activity-wide global listener. First iteration only used for NavigationPage root layout.
    /// </summary>
    public class MauiCoordinatorLayout : CoordinatorLayout
    {
        internal GlobalWindowInsetListener WindowInsetListener { get; }

        void Initialize()
        {
            // Only run once even if multiple constructors chain
            if (_initialized)
                return;
            _initialized = true;
            ViewCompat.SetOnApplyWindowInsetsListener(this, WindowInsetListener);
            ViewCompat.SetWindowInsetsAnimationCallback(this, WindowInsetListener);
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
