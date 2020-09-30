using System;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Views.Animations;
using AndroidX.AppCompat.Widget;
using AndroidX.CoordinatorLayout.Widget;
using AndroidX.Fragment.App;
using Google.Android.Material.AppBar;
using AndroidAnimation = Android.Views.Animations.Animation;
using AnimationSet = Android.Views.Animations.AnimationSet;
using AView = Android.Views.View;

namespace Xamarin.Forms.Platform.Android
{
	public class ShellContentFragment : Fragment, AndroidAnimation.IAnimationListener, IShellObservableFragment, IAppearanceObserver
	{
		#region IAnimationListener

		void AndroidAnimation.IAnimationListener.OnAnimationEnd(AndroidAnimation animation)
		{
			View?.SetLayerType(LayerType.None, null);
			AnimationFinished?.Invoke(this, EventArgs.Empty);
		}

		void AndroidAnimation.IAnimationListener.OnAnimationRepeat(AndroidAnimation animation)
		{
		}

		void AndroidAnimation.IAnimationListener.OnAnimationStart(AndroidAnimation animation)
		{
		}

		#endregion IAnimationListener

		#region IAppearanceObserver

		void IAppearanceObserver.OnAppearanceChanged(ShellAppearance appearance)
		{
			if (appearance == null)
				ResetAppearance();
			else
				SetAppearance(appearance);
		}

		#endregion IAppearanceObserver

		readonly IShellContext _shellContext;
		IShellToolbarAppearanceTracker _appearanceTracker;
		Page _page;
		IVisualElementRenderer _renderer;
		AView _root;
		ShellPageContainer _shellPageContainer;
		ShellContent _shellContent;
		Toolbar _toolbar;
		IShellToolbarTracker _toolbarTracker;
		bool _disposed;

		public ShellContentFragment(IShellContext shellContext, ShellContent shellContent)
		{
			_shellContext = shellContext;
			_shellContent = shellContent;
		}

		public ShellContentFragment(IShellContext shellContext, Page page)
		{
			_shellContext = shellContext;
			_page = page;
		}

		public event EventHandler AnimationFinished;

		public Fragment Fragment => this;

		public override AndroidAnimation OnCreateAnimation(int transit, bool enter, int nextAnim)
		{
			var result = base.OnCreateAnimation(transit, enter, nextAnim);

			if (result == null && nextAnim != 0)
			{
				result = AnimationUtils.LoadAnimation(Context, nextAnim);
			}

			if (result == null)
				return result;

			// we only want to use a hardware layer for the entering view because its quite likely
			// the view exiting is animating a button press of some sort. This means lots of GPU
			// transactions to update the texture.
			if (enter)
				View.SetLayerType(LayerType.Hardware, null);

			// This is very strange what we are about to do. For whatever reason if you take this animation
			// and wrap it into an animation set it will have a 1 frame glitch at the start where the
			// fragment shows at the final position. That sucks. So instead we reach into the returned
			// set and hook up to the first item. This means any animation we use depends on the first item
			// finishing at the end of the animation.

			if (result is AnimationSet set)
			{
				set.Animations[0].SetAnimationListener(this);
			}

			return result;
		}

		public override AView OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			if (_shellContent != null)
			{
				_page = ((IShellContentController)_shellContent).GetOrCreateContent();
			}

			_root = inflater.Inflate(Resource.Layout.ShellContent, null).JavaCast<CoordinatorLayout>();

			_toolbar = _root.FindViewById<Toolbar>(Resource.Id.shellcontent_toolbar);

			_renderer = Platform.CreateRenderer(_page, Context);
			Platform.SetRenderer(_page, _renderer);

			_shellPageContainer = new ShellPageContainer(Context, _renderer);

			if (_root is ViewGroup vg)
				vg.AddView(_shellPageContainer);

			_toolbarTracker = _shellContext.CreateTrackerForToolbar(_toolbar);
			_toolbarTracker.Page = _page;
			// this is probably not the most ideal way to do that
			_toolbarTracker.CanNavigateBack = _shellContent == null;

			_appearanceTracker = _shellContext.CreateToolbarAppearanceTracker();

			((IShellController)_shellContext.Shell).AddAppearanceObserver(this, _page);

			if (_shellPageContainer.LayoutParameters is CoordinatorLayout.LayoutParams layoutParams)
				layoutParams.Behavior = new AppBarLayout.ScrollingViewBehavior();

			return _root;
		}

		void Destroy()
		{
			((IShellController)_shellContext.Shell).RemoveAppearanceObserver(this);

			if (_shellContent != null)
			{
				((IShellContentController)_shellContent).RecyclePage(_page);
				_page.ClearValue(Platform.RendererProperty);
				_page = null;
			}

			if (_shellPageContainer != null)
			{
				_shellPageContainer.RemoveAllViews();

				if (_root is ViewGroup vg)
					vg.RemoveView(_shellPageContainer);
			}

			_renderer?.Dispose();
			_root?.Dispose();
			_toolbarTracker?.Dispose();
			_appearanceTracker?.Dispose();


			_appearanceTracker = null;
			_toolbarTracker = null;
			_toolbar = null;
			_root = null;
			_renderer = null;
			_shellContent = null;
		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			_disposed = true;
			if (disposing)
			{
				Destroy();
				_page = null;
			}

			base.Dispose(disposing);
		}

		// Use OnDestroy instead of OnDestroyView because OnDestroyView will be
		// called before the animation completes. This causes tons of tiny issues.
		public override void OnDestroy()
		{
			base.OnDestroy();
			Destroy();
		}

		protected virtual void ResetAppearance() => _appearanceTracker.ResetAppearance(_toolbar, _toolbarTracker);

		protected virtual void SetAppearance(ShellAppearance appearance) => _appearanceTracker.SetAppearance(_toolbar, _toolbarTracker, appearance);
	}
}