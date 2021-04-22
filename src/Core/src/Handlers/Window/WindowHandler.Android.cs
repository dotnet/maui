using System;
using Android.Views;
using AndroidX.AppCompat.Widget;
using AndroidX.CoordinatorLayout.Widget;

namespace Microsoft.Maui.Handlers
{
	// Should this be a Activity ?
	public partial class WindowHandler : ViewHandler<IWindow, LinearLayoutCompat>
	{
		protected override LinearLayoutCompat CreateNativeView()
		{
			if (VirtualView == null)
				throw new InvalidOperationException($"{nameof(VirtualView)} must be set to create a CoordinatorLayout");
			
			return new LinearLayoutCompat(Context!);
		}

		public override void SetVirtualView(IView view)
		{
			base.SetVirtualView(view);

			_ = NativeView ?? throw new InvalidOperationException($"{nameof(NativeView)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");


			// This currently relies on IPage : IView, which may not exactly be right
			// we may have to add another handler extension that works for Page
			// Also, AbstractViewHandler is set to work for IView (obviously); if IPage is not IView,
			// then we'll need to change it to AbstractFrameworkElementHandler or create a separate
			// abstract handler for IPage
			// TODO ezhart Think about all this stuff ^^

			var wrap = ViewGroup.LayoutParams.WrapContent;
			// Add the IPage to the root layout; use match parent so the page automatically has the dimensions of the activity
			// nativeRootLayout.AddView(nativePage, new CoordinatorLayout.LayoutParams(wrap, wrap));
			var nativePage = VirtualView.Page.ToContainerView(VirtualView.MauiContext);
			NativeView.AddView(nativePage, new LinearLayoutCompat.LayoutParams(wrap, wrap));
		}
	}
}
