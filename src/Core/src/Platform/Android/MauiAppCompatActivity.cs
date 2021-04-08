using System;
using Android.OS;
using Android.Views;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Widget;
using AndroidX.CoordinatorLayout.Widget;
using Google.Android.Material.AppBar;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Maui
{
	public partial class MauiAppCompatActivity : AppCompatActivity
	{
		public IWindow? CurrentWindow { get; private set; }
		protected override void OnCreate(Bundle? savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			var mauiApp = MauiApplication.Current.Application;
			if (mauiApp == null)
				throw new InvalidOperationException($"The {nameof(IApplication)} instance was not found.");

			var services = MauiApplication.Current.Services;
			if (mauiApp == null)
				throw new InvalidOperationException($"The {nameof(IServiceProvider)} instance was not found.");

			var mauiContext = new MauiContext(services, this);

			var state = new ActivationState(mauiContext, savedInstanceState);
			CurrentWindow = mauiApp.CreateWindow(state);
			CurrentWindow.MauiContext = mauiContext;

			var content = (CurrentWindow.Page as IView) ?? CurrentWindow.Page.View;

			CoordinatorLayout parent = new CoordinatorLayout(this);

			SetContentView(parent, new ViewGroup.LayoutParams(CoordinatorLayout.LayoutParams.MatchParent, CoordinatorLayout.LayoutParams.MatchParent));

			//AddToolbar(parent);

			parent.AddView(content.ToNative(CurrentWindow.MauiContext), new CoordinatorLayout.LayoutParams(CoordinatorLayout.LayoutParams.MatchParent, CoordinatorLayout.LayoutParams.MatchParent));
		}
		public override bool DispatchTouchEvent(MotionEvent? ev)
		{
			if (ev != null && ev.Action == MotionEventActions.Down && CurrentWindow != null)
				SendPointToAdornerService(ev.GetX(), ev.GetY());
			return base.DispatchTouchEvent(ev);
		}

		void AddToolbar(ViewGroup parent)
		{
			Toolbar toolbar = new Toolbar(this);
			var appbarLayout = new AppBarLayout(this);

			appbarLayout.AddView(toolbar, new ViewGroup.LayoutParams(AppBarLayout.LayoutParams.MatchParent, global::Android.Resource.Attribute.ActionBarSize));
			SetSupportActionBar(toolbar);
			parent.AddView(appbarLayout, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));
		}

		void SendPointToAdornerService(float x, float y)
		{
			var statusBarHeight = GetStatusBarHeight();
			int GetStatusBarHeight()
			{
				int result = 0;
				if (Resources == null)
					return result;
				int resourceId = Resources.GetIdentifier("status_bar_height", "dimen", "android");
				if (resourceId > 0)
				{
					result = Resources.GetDimensionPixelSize(resourceId);
				}
				return result;
			}
			var adornerService = MauiApplication.Current.Services.GetService<IAdornerService>();
			if (adornerService != null)
			{
				var point = new Point(this.FromPixels(x), this.FromPixels(y - statusBarHeight));
				System.Diagnostics.Debug.WriteLine($"touch at point {point.X} {point.Y}");
				(adornerService as AdornerService)?.ExecuteTouchEventDelegate(point);
			}
		}
	}
}