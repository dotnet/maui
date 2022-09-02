using System;
using System.ComponentModel;
using System.Linq;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using NScrollView = Tizen.UIExtensions.NUI.ScrollView;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen
{
	[System.Obsolete(Compatibility.Hosting.MauiAppBuilderExtensions.UseMapperInstead)]
	public class ScrollViewRenderer : ViewRenderer<ScrollView, NScrollView>
	{
		public ScrollViewRenderer()
		{
			RegisterPropertyHandler("Content", FillContent);
			RegisterPropertyHandler(ScrollView.OrientationProperty, UpdateOrientation);
			RegisterPropertyHandler(ScrollView.VerticalScrollBarVisibilityProperty, UpdateVerticalScrollBarVisibility);
			RegisterPropertyHandler(ScrollView.HorizontalScrollBarVisibilityProperty, UpdateHorizontalScrollBarVisibility);
			RegisterPropertyHandler(ScrollView.ContentSizeProperty, UpdateContentSize);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<ScrollView> e)
		{
			if (Control == null)
			{
				SetNativeControl(CreateNativeControl());
				Control.Scrolling += OnScrolled;
			}

			if (e.OldElement != null)
			{
				(e.OldElement as IScrollViewController).ScrollToRequested -= OnScrollRequested;
			}

			if (e.NewElement != null)
			{
				(e.NewElement as IScrollViewController).ScrollToRequested += OnScrollRequested;
			}

			base.OnElementChanged(e);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (ScrollView.ContentSizeProperty.PropertyName == e.PropertyName)
			{
				UpdateContentSize();
			}
			else
			{
				base.OnElementPropertyChanged(sender, e);
			}
		}

		protected virtual NScrollView CreateNativeControl()
		{
			return new NScrollView();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (null != Element)
				{
					(Element as IScrollViewController).ScrollToRequested -= OnScrollRequested;
				}

				if (Control != null)
				{
					Control.Scrolling -= OnScrolled;
				}
			}
			base.Dispose(disposing);
		}

		void FillContent()
		{
			foreach (var child in Control.ContentContainer.Children.ToList())
			{
				Control.ContentContainer.Remove(child);
			}
			Control.ContentContainer.Add(Platform.GetOrCreateRenderer(Element.Content).NativeView);
			UpdateContentSize();
		}

		void UpdateOrientation()
		{
			Control.ScrollOrientation = (global::Tizen.UIExtensions.Common.ScrollOrientation)Element.Orientation;

		}

		void UpdateContentSize()
		{
			if (Control.ContentContainer.Children.Count > 0)
			{
				Control.ContentContainer.Children[0].SizeWidth = Forms.ConvertToScaledPixel(Element.ContentSize.Width + Element.Padding.HorizontalThickness);
				Control.ContentContainer.Children[0].SizeHeight = Forms.ConvertToScaledPixel(Element.ContentSize.Height + Element.Padding.VerticalThickness);
			}
		}

		protected void OnScrolled(object sender, EventArgs e)
		{
			var region = Control.ScrollBound.ToDP();
			((IScrollViewController)Element).SetScrolledPosition(region.X, region.Y);
		}

		void OnScrollRequested(object sender, ScrollToRequestedEventArgs e)
		{
			var x = e.ScrollX;
			var y = e.ScrollY;
			if (e.Mode == ScrollToMode.Element)
			{
				Point itemPosition = (Element as IScrollViewController).GetScrollPositionForElement(e.Element as VisualElement, e.Position);
				x = itemPosition.X;
				y = itemPosition.Y;
			}

			var region = new Rect(x, y, Element.Width, Element.Height).ToPixel();
			Control.ScrollTo((float)(Element.Orientation == ScrollOrientation.Horizontal ? region.X : region.Y), e.ShouldAnimate);
			Element.SendScrollFinished();
		}

		void UpdateVerticalScrollBarVisibility()
		{
			Control.VerticalScrollBarVisibility = (global::Tizen.UIExtensions.Common.ScrollBarVisibility)Element.VerticalScrollBarVisibility;
		}

		void UpdateHorizontalScrollBarVisibility()
		{
			var orientation = Element.Orientation;
			if (orientation == ScrollOrientation.Horizontal || orientation == ScrollOrientation.Both)
				Control.HorizontalScrollBarVisibility = (global::Tizen.UIExtensions.Common.ScrollBarVisibility)Element.HorizontalScrollBarVisibility;
		}
	}
}
