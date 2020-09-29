using System;
using System.Collections.Specialized;
using System.ComponentModel;
using Android.Content;
using Android.Views;
using AndroidX.Fragment.App;
using AndroidX.ViewPager.Widget;

namespace Xamarin.Forms.Platform.Android.AppCompat
{

	public class CarouselPageRenderer : VisualElementRenderer<CarouselPage>, ViewPager.IOnPageChangeListener, IManageFragments
	{
		bool _disposed;
		FormsViewPager _viewPager;
		Page _previousPage;
		FragmentManager _fragmentManager;

		public CarouselPageRenderer(Context context) : base(context)
		{
			AutoPackage = false;
		}

		[Obsolete("This constructor is obsolete as of version 2.5. Please use CarouselPageRenderer(Context) instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public CarouselPageRenderer()
		{
			AutoPackage = false;
		}

		void ViewPager.IOnPageChangeListener.OnPageScrolled(int position, float positionOffset, int positionOffsetPixels)
		{
		}

		void ViewPager.IOnPageChangeListener.OnPageScrollStateChanged(int state)
		{
		}

		IPageController PageController => Element as IPageController;

		FragmentManager FragmentManager => _fragmentManager ?? (_fragmentManager = Context.GetFragmentManager());

		void IManageFragments.SetFragmentManager(FragmentManager childFragmentManager)
		{
			if (_fragmentManager == null)
				_fragmentManager = childFragmentManager;
		}

		void ViewPager.IOnPageChangeListener.OnPageSelected(int position)
		{
			Element.CurrentPage = Element.Children[position];
			if (_previousPage != Element.CurrentPage)
			{
				_previousPage?.SendDisappearing();
				_previousPage = Element.CurrentPage;
			}
			Element.CurrentPage.SendAppearing();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && !_disposed)
			{
				_disposed = true;

				if (Element != null)
					PageController.InternalChildren.CollectionChanged -= OnChildrenCollectionChanged;

				if (_viewPager != null)
				{
					RemoveView(_viewPager);

					_viewPager.ClearOnPageChangeListeners();
					_viewPager.Adapter.Dispose();
					_viewPager.Dispose();
					_viewPager = null;
				}

				RemoveAllViews();

				_previousPage = null;
				_fragmentManager = null;

				if (Element?.Children != null)
				{
					foreach (ContentPage pageToRemove in Element.Children)
					{
						IVisualElementRenderer pageRenderer = Android.Platform.GetRenderer(pageToRemove);
						if (pageRenderer != null)
						{
							pageRenderer.View.RemoveFromParent();
							pageRenderer.Dispose();
						}

						pageToRemove.ClearValue(Android.Platform.RendererProperty);
					}
				}
			}

			base.Dispose(disposing);
		}

		protected override void OnAttachedToWindow()
		{
			base.OnAttachedToWindow();
			if (Parent is PageContainer pageContainer && (pageContainer.IsInFragment || pageContainer.Visibility == ViewStates.Gone))
				return;
			PageController.SendAppearing();
			Element.CurrentPage?.SendAppearing();
		}

		protected override void OnDetachedFromWindow()
		{
			base.OnDetachedFromWindow();
			if (Parent is PageContainer pageContainer && pageContainer.IsInFragment)
				return;
			Element.CurrentPage?.SendDisappearing();
			PageController.SendDisappearing();
		}

		protected override void OnElementChanged(ElementChangedEventArgs<CarouselPage> e)
		{
			base.OnElementChanged(e);

			var activity = (FormsAppCompatActivity)Context.GetActivity();

			if (e.OldElement != null)
				((IPageController)e.OldElement).InternalChildren.CollectionChanged -= OnChildrenCollectionChanged;

			if (e.NewElement != null)
			{
				if (_viewPager != null)
				{
					_viewPager.RemoveOnPageChangeListener(this);

					ViewGroup.RemoveView(_viewPager);

					_viewPager.Dispose();
				}

				FormsViewPager pager = _viewPager = new FormsViewPager(activity)
				{
					OverScrollMode = OverScrollMode.Never,
					EnableGesture = true,
					LayoutParameters = new LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent),
					Adapter = new FormsFragmentPagerAdapter<ContentPage>(e.NewElement, FragmentManager) { CountOverride = e.NewElement.Children.Count }
				};
				pager.Id = Platform.GenerateViewId();
				pager.AddOnPageChangeListener(this);

				ViewGroup.AddView(pager);
				CarouselPage carouselPage = e.NewElement;
				if (carouselPage.CurrentPage != null)
				{
					_previousPage = carouselPage.CurrentPage;
					ScrollToCurrentPage();
				}

				((IPageController)carouselPage).InternalChildren.CollectionChanged += OnChildrenCollectionChanged;
			}
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == nameof(Element.CurrentPage))
				ScrollToCurrentPage();
		}

		protected override void OnLayout(bool changed, int l, int t, int r, int b)
		{
			FormsViewPager pager = _viewPager;
			Context context = Context;
			int width = r - l;
			int height = b - t;

			pager.Measure(MeasureSpecFactory.MakeMeasureSpec(width, MeasureSpecMode.AtMost), MeasureSpecFactory.MakeMeasureSpec(height, MeasureSpecMode.AtMost));

			if (width > 0 && height > 0)
			{
				PageController.ContainerArea = new Rectangle(0, 0, context.FromPixels(width), context.FromPixels(height));
				pager.Layout(0, 0, width, b);
			}

			base.OnLayout(changed, l, t, r, b);
		}

		void OnChildrenCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			FormsViewPager pager = _viewPager;

			((FormsFragmentPagerAdapter<ContentPage>)pager.Adapter).CountOverride = Element.Children.Count;
			pager.Adapter.NotifyDataSetChanged();
		}

		void ScrollToCurrentPage()
		{
			_viewPager.SetCurrentItem(Element.Children.IndexOf(Element.CurrentPage), true);
		}
	}
}