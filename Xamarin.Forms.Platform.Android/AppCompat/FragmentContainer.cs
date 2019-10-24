using System;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific.AppCompat;
using AView = Android.Views.View;
using Fragment = Android.Support.V4.App.Fragment;

namespace Xamarin.Forms.Platform.Android.AppCompat
{
	internal class FragmentContainer : Fragment
	{
		readonly WeakReference _pageReference;

		Action<PageContainer> _onCreateCallback;
		bool? _isVisible;
		PageContainer _pageContainer;
		IVisualElementRenderer _visualElementRenderer;

		public FragmentContainer()
		{
		}

		public FragmentContainer(Page page) : this()
		{
			_pageReference = new WeakReference(page);
		}

		protected FragmentContainer(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
		{
		}

		public Page Page => (Page)_pageReference?.Target;

		IPageController PageController => Page as IPageController;

		public override bool UserVisibleHint
		{
			get { return base.UserVisibleHint; }
			set
			{
				base.UserVisibleHint = value;
				if (_isVisible == value)
					return;
				_isVisible = value;
				if (_isVisible.Value)
					PageController?.SendAppearing();
				else
					PageController?.SendDisappearing();
			}
		}

		public static Fragment CreateInstance(Page page)
		{
			return new FragmentContainer(page) { Arguments = new Bundle() };
		}

		public void SetOnCreateCallback(Action<PageContainer> callback)
		{
			_onCreateCallback = callback;
		}

		public override AView OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			if (Page != null)
			{
				_visualElementRenderer = Android.Platform.CreateRenderer(Page, ChildFragmentManager, inflater.Context);
				Android.Platform.SetRenderer(Page, _visualElementRenderer);

				_pageContainer = new PageContainer(inflater.Context, _visualElementRenderer, true);

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
			bool shouldSendEvent = Application.Current.OnThisPlatform().GetSendDisappearingEventOnPause();
			if (shouldSendEvent)
				SendLifecycleEvent(false);

			base.OnPause();
		}

		public override void OnResume()
		{
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

			if (isAppearing && UserVisibleHint)
				PageController?.SendAppearing();
			else if(!isAppearing)
				PageController?.SendDisappearing();
		}
	}
}