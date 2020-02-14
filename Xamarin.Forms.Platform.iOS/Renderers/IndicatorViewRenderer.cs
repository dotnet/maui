using System.ComponentModel;
using UIKit;
using static Xamarin.Forms.IndicatorView;

namespace Xamarin.Forms.Platform.iOS
{
	public class IndicatorViewRenderer : ViewRenderer<IndicatorView, UIView>
	{
		UIColor _defaultPagesIndicatorTintColor;
		UIColor _defaultCurrentPagesIndicatorTintColor;
		UIPageControl UIPager => Control as UIPageControl;
		bool _disposed;
		bool _updatingPosition;

		public UIView View => this;


		[Internals.Preserve(Conditional = true)]
		public IndicatorViewRenderer()
		{

		}

		protected override void OnElementChanged(ElementChangedEventArgs<IndicatorView> e)
		{
			base.OnElementChanged(e);

			if (e.NewElement != null)
			{
				if (Control == null)
				{
					UpdateControl();
				}
			}

			if (UIPager != null)
			{
				UpdatePagesIndicatorTintColor();
				UpdateCurrentPagesIndicatorTintColor();
				UpdatePages();
				UpdateHidesForSinglePage();
				UpdateCurrentPage();
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			_disposed = true;

			if (disposing)
			{
				if (UIPager != null)
				{
					UIPager.ValueChanged -= UIPagerValueChanged;
				}
			}

			base.Dispose(disposing);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == IndicatorsShapeProperty.PropertyName ||
				e.PropertyName == ItemsSourceProperty.PropertyName)
				UpdateIndicator();
			else if (e.PropertyName == IndicatorTemplateProperty.PropertyName)
				UpdateIndicatorTemplate();
			if (e.PropertyName == IndicatorColorProperty.PropertyName)
				UpdatePagesIndicatorTintColor();
			else if (e.PropertyName == SelectedIndicatorColorProperty.PropertyName)
				UpdateCurrentPagesIndicatorTintColor();
			else if (e.PropertyName == CountProperty.PropertyName)
				UpdatePages();
			else if (e.PropertyName == HideSingleProperty.PropertyName)
				UpdateHidesForSinglePage();
			else if (e.PropertyName == PositionProperty.PropertyName)
				UpdateCurrentPage();
		}

		protected override UIView CreateNativeControl()
		{
			if (UIPager != null)
			{
				UIPager.ValueChanged -= UIPagerValueChanged;
			}

			var uiPager = new UIPageControl();
			_defaultPagesIndicatorTintColor = uiPager.PageIndicatorTintColor;
			_defaultCurrentPagesIndicatorTintColor = uiPager.CurrentPageIndicatorTintColor;
			uiPager.ValueChanged += UIPagerValueChanged;

			return uiPager;
		}
	
		void UpdateControl()
		{
			ClearIndicators();

			var control = (Element.IndicatorTemplate != null)
				? (UIView)Element.IndicatorLayout.GetRenderer()
				: CreateNativeControl();

			SetNativeControl(control);
		}

		void ClearIndicators()
		{
			foreach (var child in View.Subviews)
				child.RemoveFromSuperview();
		}

		void UpdateIndicator()
		{
			if (Element.IndicatorsShape == IndicatorShape.Circle && Element.IndicatorTemplate == null)
				UpdateIndicatorShape();
			else
				UpdateIndicatorTemplate();
		}

		void UpdateIndicatorShape()
		{
			ClearIndicators();
			AddSubview(UIPager);
		}

		void UpdateIndicatorTemplate()
		{
			if (Element.IndicatorLayout == null)
				return;

			ClearIndicators();
			var control = (UIView)Element.IndicatorLayout.GetRenderer();
			AddSubview(control);

			var indicatorLayoutSizeRequest = Element.IndicatorLayout.Measure(double.PositiveInfinity, double.PositiveInfinity, MeasureFlags.IncludeMargins);
			Element.IndicatorLayout.Layout(new Rectangle(0, 0, indicatorLayoutSizeRequest.Request.Width, indicatorLayoutSizeRequest.Request.Height));
		}

		void UIPagerValueChanged(object sender, System.EventArgs e)
		{
			if (_updatingPosition || UIPager == null)
				return;

			Element.Position = (int)UIPager.CurrentPage;
		}

		void UpdateCurrentPage()
		{
			if (UIPager == null)
				return;

			_updatingPosition = true;
			UIPager.CurrentPage = Element.Position;
			_updatingPosition = false;
		}

		void UpdatePages()
		{
			if (UIPager == null)
				return;

			UIPager.Pages = Element.Count;
		}

		void UpdateHidesForSinglePage()
		{
			if (UIPager == null)
				return;

			UIPager.HidesForSinglePage = Element.HideSingle;
		}

		void UpdatePagesIndicatorTintColor()
		{
			if (UIPager == null)
				return;

			var color = Element.IndicatorColor;
			UIPager.PageIndicatorTintColor = color.IsDefault ? _defaultPagesIndicatorTintColor : color.ToUIColor();
		}

		void UpdateCurrentPagesIndicatorTintColor()
		{
			if (UIPager == null)
				return;

			var color = Element.SelectedIndicatorColor;
			UIPager.CurrentPageIndicatorTintColor = color.IsDefault ? _defaultCurrentPagesIndicatorTintColor : color.ToUIColor();
		}
	}
}