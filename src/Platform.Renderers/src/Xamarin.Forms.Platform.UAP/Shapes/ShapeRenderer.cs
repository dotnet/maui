using System;
using System.ComponentModel;
using Xamarin.Forms.Shapes;
using Shape = Xamarin.Forms.Shapes.Shape;

#if WINDOWS_UWP
using Windows.UI.Xaml;
using WDoubleCollection = Windows.UI.Xaml.Media.DoubleCollection;
using WPenLineCap = Windows.UI.Xaml.Media.PenLineCap;
using WPenLineJoin = Windows.UI.Xaml.Media.PenLineJoin;
using WShape = Windows.UI.Xaml.Shapes.Shape;
using WStretch = Windows.UI.Xaml.Media.Stretch;

namespace Xamarin.Forms.Platform.UWP
#else
using System.Windows;
using Xamarin.Forms.Platform.WPF.Extensions;
using WDoubleCollection = System.Windows.Media.DoubleCollection;
using WPenLineCap = System.Windows.Media.PenLineCap;
using WPenLineJoin = System.Windows.Media.PenLineJoin;
using WShape = System.Windows.Shapes.Shape;
using WStretch = System.Windows.Media.Stretch;

namespace Xamarin.Forms.Platform.WPF
#endif
{
	public class ShapeRenderer<TShape, TNativeShape> : ViewRenderer<TShape, TNativeShape>
		  where TShape : Shape
		  where TNativeShape : WShape
	{
		protected override void OnElementChanged(ElementChangedEventArgs<TShape> args)
		{
			base.OnElementChanged(args);

			if (args.NewElement != null)
			{
				UpdateAspect();
				UpdateFill();
				UpdateStroke();
				UpdateStrokeThickness();
				UpdateStrokeDashArray();
				UpdateStrokeDashOffset();
				UpdateStrokeLineCap();
				UpdateStrokeLineJoin();
				UpdateStrokeMiterLimit();
			}
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			base.OnElementPropertyChanged(sender, args);

			if (args.PropertyName == VisualElement.HeightProperty.PropertyName)
				UpdateHeight();
			else if (args.PropertyName == VisualElement.WidthProperty.PropertyName)
				UpdateWidth();
			else if (args.PropertyName == Shape.AspectProperty.PropertyName)
				UpdateAspect();
			else if (args.PropertyName == Shape.FillProperty.PropertyName)
				UpdateFill();
			else if (args.PropertyName == Shape.StrokeProperty.PropertyName)
				UpdateStroke();
			else if (args.PropertyName == Shape.StrokeThicknessProperty.PropertyName)
				UpdateStrokeThickness();
			else if (args.PropertyName == Shape.StrokeDashArrayProperty.PropertyName)
				UpdateStrokeDashArray();
			else if (args.PropertyName == Shape.StrokeDashOffsetProperty.PropertyName)
				UpdateStrokeDashOffset();
			else if (args.PropertyName == Shape.StrokeLineCapProperty.PropertyName)
				UpdateStrokeLineCap();
			else if (args.PropertyName == Shape.StrokeLineJoinProperty.PropertyName)
				UpdateStrokeLineJoin();
			else if (args.PropertyName == Shape.StrokeMiterLimitProperty.PropertyName)
				UpdateStrokeMiterLimit();
		}

#if !WINDOWS_UWP
		new
#endif
		void UpdateHeight()
		{
			Control.Height = Math.Max(Element.Height, 0);
		}

#if !WINDOWS_UWP
		new
#endif
		void UpdateWidth()
		{
			Control.Width = Math.Max(Element.Width, 0);
		}

		void UpdateAspect()
		{
			Stretch aspect = Element.Aspect;
			WStretch stretch = WStretch.None;

			switch (aspect)
			{
				case Stretch.None:
					stretch = WStretch.None;
					break;
				case Stretch.Fill:
					stretch = WStretch.Fill;
					break;
				case Stretch.Uniform:
					stretch = WStretch.Uniform;
					break;
				case Stretch.UniformToFill:
					stretch = WStretch.UniformToFill;
					break;
			}

			Control.Stretch = stretch;

			if (aspect == Stretch.Uniform)
			{
				Control.HorizontalAlignment = HorizontalAlignment.Center;
				Control.VerticalAlignment = VerticalAlignment.Center;
			}
			else
			{
				Control.HorizontalAlignment = HorizontalAlignment.Left;
				Control.VerticalAlignment = VerticalAlignment.Top;
			}
		}

		void UpdateFill()
		{
			Control.Fill = Element.Fill.ToBrush();
		}

		void UpdateStroke()
		{
			Control.Stroke = Element.Stroke.ToBrush();
		}

		void UpdateStrokeThickness()
		{
			Control.StrokeThickness = Element.StrokeThickness;
		}

		void UpdateStrokeDashArray()
		{
			if (Control.StrokeDashArray != null)
				Control.StrokeDashArray.Clear();

			if (Element.StrokeDashArray != null && Element.StrokeDashArray.Count > 0)
			{
				if (Control.StrokeDashArray == null)
					Control.StrokeDashArray = new WDoubleCollection();

				double[] array = new double[Element.StrokeDashArray.Count];
				Element.StrokeDashArray.CopyTo(array, 0);

				foreach (double value in array)
				{
					Control.StrokeDashArray.Add(value);
				}
			}
		}

		void UpdateStrokeDashOffset()
		{
			Control.StrokeDashOffset = Element.StrokeDashOffset;
		}

		void UpdateStrokeLineCap()
		{
			PenLineCap lineCap = Element.StrokeLineCap;
			WPenLineCap wLineCap = WPenLineCap.Flat;

			switch (lineCap)
			{
				case PenLineCap.Flat:
					wLineCap = WPenLineCap.Flat;
					break;
				case PenLineCap.Square:
					wLineCap = WPenLineCap.Square;
					break;
				case PenLineCap.Round:
					wLineCap = WPenLineCap.Round;
					break;
			}

			Control.StrokeStartLineCap = wLineCap;
			Control.StrokeEndLineCap = wLineCap;
		}

		void UpdateStrokeLineJoin()
		{
			PenLineJoin lineJoin = Element.StrokeLineJoin;
			WPenLineJoin wLineJoin = WPenLineJoin.Miter;

			switch (lineJoin)
			{
				case PenLineJoin.Miter:
					wLineJoin = WPenLineJoin.Miter;
					break;
				case PenLineJoin.Bevel:
					wLineJoin = WPenLineJoin.Bevel;
					break;
				case PenLineJoin.Round:
					wLineJoin = WPenLineJoin.Round;
					break;
			}

			Control.StrokeLineJoin = wLineJoin;
		}

		void UpdateStrokeMiterLimit()
		{
			Control.StrokeMiterLimit = Element.StrokeMiterLimit;
		}
	}
}