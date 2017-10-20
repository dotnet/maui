using System;
using System.ComponentModel;
using Android.Content;
using Android.Support.V4.View;
using Android.Views;

namespace Xamarin.Forms.Platform.Android
{
	public class CarouselPageRenderer : VisualElementRenderer<CarouselPage>
	{
		ViewPager _viewPager;

		public CarouselPageRenderer(Context context) : base(context)
		{
			AutoPackage = false;
		}

		[Obsolete("This constructor is obsolete as of version 2.5. Please use CarouselPageRenderer(Context) instead.")]
		public CarouselPageRenderer()
		{
			AutoPackage = false;
		}

		IPageController PageController => Element as IPageController;

		protected override void Dispose(bool disposing)
		{
			if (disposing && _viewPager != null)
			{
				if (_viewPager.Adapter != null)
					_viewPager.Adapter.Dispose();
				_viewPager.Dispose();
				_viewPager = null;
			}
			base.Dispose(disposing);
		}

		protected override void OnAttachedToWindow()
		{
			base.OnAttachedToWindow();
			var adapter = new CarouselPageAdapter(_viewPager, Element, Context);
			_viewPager.Adapter = adapter;
			_viewPager.AddOnPageChangeListener(adapter);

			adapter.UpdateCurrentItem();

			PageController.SendAppearing();
		}

		protected override void OnDetachedFromWindow()
		{
			base.OnDetachedFromWindow();
			PageController.SendDisappearing();
		}

		protected override void OnElementChanged(ElementChangedEventArgs<CarouselPage> e)
		{
			base.OnElementChanged(e);

			if (_viewPager != null)
			{
				RemoveView(_viewPager);
				_viewPager.ClearOnPageChangeListeners();
				_viewPager.Dispose();
			}

			_viewPager = new ViewPager(Context);

			AddView(_viewPager);

			_viewPager.OffscreenPageLimit = int.MaxValue;
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == "CurrentPage" && Element.CurrentPage != null)
			{
				if (!Element.Batched)
					UpdateCurrentItem();
			}
		}

		protected override void OnLayout(bool changed, int l, int t, int r, int b)
		{
			base.OnLayout(changed, l, t, r, b);
			if (_viewPager != null)
			{
				_viewPager.Measure(MeasureSpecFactory.MakeMeasureSpec(r - l, MeasureSpecMode.Exactly), MeasureSpecFactory.MakeMeasureSpec(b - t, MeasureSpecMode.Exactly));
				_viewPager.Layout(0, 0, r - l, b - t);
			}
		}

		protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
		{
			_viewPager.Measure(widthMeasureSpec, heightMeasureSpec);
			SetMeasuredDimension(_viewPager.MeasuredWidth, _viewPager.MeasuredHeight);
		}

		void UpdateCurrentItem()
		{
			int index = CarouselPage.GetIndex(Element.CurrentPage);
			if (index < 0 || index >= ((IElementController)Element).LogicalChildren.Count)
				return;

			_viewPager.CurrentItem = index;
		}
	}
}