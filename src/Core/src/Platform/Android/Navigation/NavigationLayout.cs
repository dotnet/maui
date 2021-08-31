using System;
using Android.Content;
using Android.Runtime;
using Android.Util;
using AndroidX.AppCompat.Widget;
using AndroidX.CoordinatorLayout.Widget;
using Google.Android.Material.AppBar;

namespace Microsoft.Maui
{
	public class NavigationLayout : CoordinatorLayout
	{
		// I'd prefer to not need this here but I'm not sure how else
		// to get this class to the NavigationViewFragment
		NavigationManager? _navigationManager;
		public NavigationManager NavigationManager
		{
			get => _navigationManager ?? throw new InvalidOperationException($"NavigationManager cannot be null");
			internal set => _navigationManager = value;
		}

#pragma warning disable CS0618 //FIXME: [Preserve] is obsolete
		[Preserve(Conditional = true)]
		public NavigationLayout(Context context) : base(context)
		{
		}

		[Preserve(Conditional = true)]
		public NavigationLayout(Context context, IAttributeSet attrs) : base(context, attrs)
		{
		}

		[Preserve(Conditional = true)]
		public NavigationLayout(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
		{
		}

		[Preserve(Conditional = true)]
		protected NavigationLayout(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
		{
		}
#pragma warning restore CS0618 //FIXME: [Preserve] is obsolete
	}
}
