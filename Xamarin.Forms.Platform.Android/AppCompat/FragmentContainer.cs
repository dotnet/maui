using System;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Views;
using AView = Android.Views.View;

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
				_visualElementRenderer = Android.Platform.CreateRenderer(Page, ChildFragmentManager);
				Android.Platform.SetRenderer(Page, _visualElementRenderer);

				_pageContainer = new PageContainer(Forms.Context, _visualElementRenderer, true);

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
					if (_visualElementRenderer.ViewGroup.Handle != IntPtr.Zero)
					{
						_visualElementRenderer.ViewGroup.RemoveFromParent();
					}

					_visualElementRenderer.Dispose();
				}

				if (_pageContainer != null && _pageContainer.Handle != IntPtr.Zero)
				{
					_pageContainer.RemoveFromParent();
					_pageContainer.Dispose();
				}

				Page?.ClearValue(Android.Platform.RendererProperty);
			}

			_onCreateCallback = null;
			_visualElementRenderer = null;
			_pageContainer = null;

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
			PageController?.SendDisappearing();
			base.OnPause();
		}
		
		public override void OnResume()
		{
			if (UserVisibleHint)
				PageController?.SendAppearing();
			base.OnResume();
		}
	}
}