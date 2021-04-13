using System;
using System.ComponentModel;
using CoreGraphics;
using Microsoft.Maui.Graphics;
using UIKit;
using static Microsoft.Maui.Controls.IndicatorView;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
{
	public class IndicatorViewRenderer : ViewRenderer<IndicatorView, UIView>
	{
		UIColor _defaultPagesIndicatorTintColor;
		UIColor _defaultCurrentPagesIndicatorTintColor;
		FormsPageControl UIPager => Control as FormsPageControl;
		bool _disposed;
		bool _updatingPosition;

		public UIView View => this;


		[Microsoft.Maui.Controls.Internals.Preserve(Conditional = true)]
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
				if (Forms.IsiOS14OrNewer)
				{
					UIPager.AllowsContinuousInteraction = false;
					UIPager.BackgroundStyle = UIPageControlBackgroundStyle.Minimal;
				}

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

			if (e.PropertyName == IndicatorSizeProperty.PropertyName)
				UpdateIndicatorSize();
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
			else if (e.Is(PositionProperty))
				UpdateCurrentPage();
			else if (e.Is(MaximumVisibleProperty))
				UpdateMaximumVisible();
		}

		protected override UIView CreateNativeControl()
		{
			if (UIPager != null)
			{
				UIPager.ValueChanged -= UIPagerValueChanged;
			}

			var uiPager = new FormsPageControl
			{
				IsSquare = Element.IndicatorsShape == IndicatorShape.Square,
				IndicatorSize = Element.IndicatorSize
			};
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
			if (Element.IndicatorTemplate == null)
				UpdateIndicatorShape();
			else
				UpdateIndicatorTemplate();
		}

		void UpdateIndicatorShape()
		{
			ClearIndicators();
			UIPager.IsSquare = Element.IndicatorsShape == IndicatorShape.Square;
			AddSubview(UIPager);
			UIPager.LayoutSubviews();
		}

		void UpdateIndicatorSize()
		{
			UIPager.IndicatorSize = Element.IndicatorSize;
			UIPager.LayoutSubviews();
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
			var maxVisible = GetMaximumVisible();
			var position = Element.Position;
			var index = position >= maxVisible ? maxVisible - 1 : position;
			UIPager.CurrentPage = index;
			UIPager.LayoutSubviews();
			_updatingPosition = false;
		}

		void UpdatePages()
		{
			if (UIPager == null)
				return;

			UIPager.Pages = GetMaximumVisible();

			UpdateCurrentPage();
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
			UIPager.PageIndicatorTintColor = color?.ToUIColor() ?? _defaultPagesIndicatorTintColor;
		}

		void UpdateCurrentPagesIndicatorTintColor()
		{
			if (UIPager == null)
				return;

			var color = Element.SelectedIndicatorColor;
			UIPager.CurrentPageIndicatorTintColor = color?.ToUIColor() ?? _defaultCurrentPagesIndicatorTintColor;
		}

		void UpdateMaximumVisible()
		{
			UpdatePages();
			UpdateCurrentPage();
		}

		int GetMaximumVisible()
		{
			var minValue = Math.Min(Element.MaximumVisible, Element.Count);
			return minValue <= 0 ? 0 : minValue;
		}
	}

	class FormsPageControl : UIPageControl
	{
		const int DefaultIndicatorSize = 7;

		public bool IsSquare { get; set; }

		public double IndicatorSize { get; set; }

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();

			float scale = (float)IndicatorSize / DefaultIndicatorSize;
			var newTransform = CGAffineTransform.MakeScale(scale, scale);

			Transform = newTransform;
			if (Subviews.Length == 0)
				return;

			foreach (var view in Subviews)
			{
				if (IsSquare)
				{
					view.Layer.CornerRadius = 0;
				}
			}
		}
	}
}