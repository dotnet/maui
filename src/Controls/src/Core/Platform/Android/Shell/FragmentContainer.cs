using System;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using AndroidX.Fragment.App;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.AppCompat;
using AView = Android.Views.View;

namespace Microsoft.Maui.Controls.Platform
{
	internal class FragmentContainer : Fragment
	{
		readonly WeakReference _pageRenderer;
		readonly IMauiContext _mauiContext;
		Action<PageContainer> _onCreateCallback;
		PageContainer _pageContainer;
		INativeViewHandler _viewhandler;
		//bool _isVisible = false;
		AView NativeView => _viewhandler?.NativeView as AView;

		public FragmentContainer(IMauiContext mauiContext)
		{
			_mauiContext = mauiContext;
		}

		public FragmentContainer(Page page, IMauiContext mauiContext) : this(mauiContext)
		{
			_pageRenderer = new WeakReference(page);
			_mauiContext = mauiContext;
		}

		public virtual Page Page => (Page)_pageRenderer?.Target;

		IPageController PageController => Page as IPageController;

		public static Fragment CreateInstance(Page page, IMauiContext mauiContext)
		{
			return new FragmentContainer(page, mauiContext) { Arguments = new Bundle() };
		}

		public void SetOnCreateCallback(Action<PageContainer> callback)
		{
			_onCreateCallback = callback;
		}

		protected virtual PageContainer CreatePageContainer(Context context, INativeViewHandler child, bool inFragment)
		{
			return new PageContainer(context, child, inFragment);
		}

		public override AView OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			if (Page != null)
			{
				Page.ToNative(_mauiContext);
				_viewhandler = (INativeViewHandler)Page.Handler;

				_pageContainer = CreatePageContainer(inflater.Context, _viewhandler, true);

				_onCreateCallback?.Invoke(_pageContainer);

				return _pageContainer;
			}

			return null;
		}

		protected virtual void RecyclePage()
		{
			// Page.Handler = null;
		}

		public override void OnDestroyView()
		{
			if (Page != null)
			{
				if (_viewhandler != null)
				{
					if (NativeView.IsAlive())
					{
						NativeView.RemoveFromParent();
					}

					RecyclePage();
				}
			}

			_onCreateCallback = null;
			_viewhandler = null;

			base.OnDestroyView();
		}

		//public override void OnHiddenChanged(bool hidden)
		//{
		//	base.OnHiddenChanged(hidden);

		//	if (Page == null)
		//		return;

		//	if (hidden)
		//		PageController?.SendDisappearing();
		//	else
		//		PageController?.SendAppearing();
		//}

		// TODO MAUI
		//public override void OnPause()
		//{
		//	_isVisible = false;

		//	bool shouldSendEvent = Application.Current.OnThisPlatform().GetSendDisappearingEventOnPause();
		//	if (shouldSendEvent)
		//		SendLifecycleEvent(false);

		//	base.OnPause();
		//}

		//public override void OnResume()
		//{
		//	_isVisible = true;

		//	bool shouldSendEvent = Application.Current.OnThisPlatform().GetSendAppearingEventOnResume();
		//	if (shouldSendEvent)
		//		SendLifecycleEvent(true);

		//	base.OnResume();
		//}

		//void SendLifecycleEvent(bool isAppearing)
		//{
		//	var flyoutPage = Application.Current.MainPage as FlyoutPage;
		//	var pageContainer = (flyoutPage != null ? flyoutPage.Detail : Application.Current.MainPage) as IPageContainer<Page>;
		//	Page currentPage = pageContainer?.CurrentPage;

		//	if (!(currentPage == null || currentPage == PageController))
		//		return;

		//	if (isAppearing && _isVisible)
		//		PageController?.SendAppearing();
		//	else if (!isAppearing)
		//		PageController?.SendDisappearing();
		//}
	}
}