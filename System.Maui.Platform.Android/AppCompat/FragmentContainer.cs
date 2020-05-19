using System;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific.AppCompat;
using AView = Android.Views.View;

#if __ANDROID_29__
using AndroidX.Fragment.App;
#else
using Android.Support.V4.App;
#endif

namespace Xamarin.Forms.Platform.Android.AppCompat
{
	internal class FragmentContainer : Fragment
	{
		readonly WeakReference _pageRenderer;

		Action<PageContainer> _onCreateCallback;
		PageContainer _pageContainer;
		IVisualElementRenderer _visualElementRenderer;

#if __ANDROID_29__
		bool _isVisible = false;
#endif
		public FragmentContainer()
		{
		}

		public FragmentContainer(Page page) : this()
		{
			_pageRenderer = new WeakReference(page);
		}

		protected FragmentContainer(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
		{
		}

		public virtual Page Page => (Page)_pageRenderer?.Target;

		IPageController PageController => Page as IPageController;

		public static Fragment CreateInstance(Page page)
		{
			return new FragmentContainer(page) { Arguments = new Bundle() };
		}

		public void SetOnCreateCallback(Action<PageContainer> callback)
		{
			_onCreateCallback = callback;
		}

		protected virtual PageContainer CreatePageContainer (Context context, IVisualElementRenderer child, bool inFragment)
		{
			return new PageContainer(context, child, inFragment);
		}

		public override AView OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			if (Page != null)
			{
				_visualElementRenderer = Android.Platform.CreateRenderer(Page, ChildFragmentManager, inflater.Context);
				Android.Platform.SetRenderer(Page, _visualElementRenderer);

				_pageContainer = CreatePageContainer(inflater.Context, _visualElementRenderer, true);

				_onCreateCallback?.Invoke(_pageContainer);

				return _pageContainer;
			}

			return null;
		}

		public override void OnDestroyView()
		{
			if (Page != null)
			{
				if (_visualElementRenderer != null)
				{
					if (_visualElementRenderer.View.Handle != IntPtr.Zero)
					{
						_visualElementRenderer.View.RemoveFromParent();
					}

					_visualElementRenderer.Dispose();
				}

				// We do *not* eagerly dispose of the _pageContainer here; doing so  causes a memory leak 
				// if animated fragment transitions are enabled (it removes some info that the animation's 
				// onAnimationEnd handler requires to properly clean things up)
				// Instead, we let the garbage collector pick it up later, when we can be sure it's safe

				Page?.ClearValue(Android.Platform.RendererProperty);
			}

			_onCreateCallback = null;
			_visualElementRenderer = null;

			base.OnDestroyView();
		}

		public override void OnHiddenChanged(bool hidden)
		{
			base.OnHiddenChanged(hidden);

			if (Page == null)
				return;

			if (hidden)
				PageController?.SendDisappearing();
			else
				PageController?.SendAppearing();
		}

		public override void OnPause()
		{
#if __ANDROID_29__
			_isVisible = false;
#endif

			bool shouldSendEvent = Application.Current.OnThisPlatform().GetSendDisappearingEventOnPause();
			if (shouldSendEvent)
				SendLifecycleEvent(false);

			base.OnPause();
		}

		public override void OnResume()
		{

#if __ANDROID_29__
			_isVisible = true;
#endif

			bool shouldSendEvent = Application.Current.OnThisPlatform().GetSendAppearingEventOnResume();
			if (shouldSendEvent)
				SendLifecycleEvent(true);

			base.OnResume();
		}

		void SendLifecycleEvent(bool isAppearing)
		{
			var masterDetailPage = Application.Current.MainPage as MasterDetailPage;
			var pageContainer = (masterDetailPage != null ? masterDetailPage.Detail : Application.Current.MainPage) as IPageContainer<Page>;
			Page currentPage = pageContainer?.CurrentPage;

			if(!(currentPage == null || currentPage == PageController))
				return;

#if __ANDROID_29__
			if (isAppearing && _isVisible)
#else
			if (isAppearing && UserVisibleHint)
#endif
				PageController?.SendAppearing();
			else if(!isAppearing)
				PageController?.SendDisappearing();
		}
	}
}