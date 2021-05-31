using System;
using System.ComponentModel;
using CoreAnimation;
using CoreGraphics;
using Microsoft.Maui.Controls.Shapes;
using Shape = Microsoft.Maui.Controls.Shapes.Shape;
using Microsoft.Maui.Graphics;

#if __MOBILE__
using UIKit;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
#else
using AppKit;

namespace Microsoft.Maui.Controls.Compatibility.Platform.MacOS
#endif
{
	public class ShapeRenderer<TShape, TNativeShape> : ViewRenderer<TShape, TNativeShape>
		where TShape : Shape
		where TNativeShape : ShapeView
	{
		double _height;
		double _width;

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

				if (!args.NewElement.Bounds.IsEmpty)
				{
					_height = Element.Height;
					_width = Element.Width;

					UpdateSize();
				}
			}
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			base.OnElementPropertyChanged(sender, args);

			if (args.PropertyName == VisualElement.HeightProperty.PropertyName)
			{
				_height = Element.Height;
				UpdateSize();
			}
			else if (args.PropertyName == VisualElement.WidthProperty.PropertyName)
			{
				_width = Element.Width;
				UpdateSize();
			}
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

		public override SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			if (Control != null)
			{
				return Control.ShapeLayer.GetDesiredSize();
			}

			return base.GetDesiredSize(widthConstraint, heightConstraint);
		}

		void UpdateAspect()
		{
			Control.ShapeLayer.UpdateAspect(Element.Aspect);
		}

		void UpdateSize()
		{
			Control.ShapeLayer.UpdateSize(new CGSize(new nfloat(_width), new nfloat(_height)));
		}

		void UpdateFill()
		{
			Control.ShapeLayer.UpdateFill(Element.Fill);
		}

		void UpdateStroke()
		{
			Control.ShapeLayer.UpdateStroke(Element.Stroke);
		}

		void UpdateStrokeThickness()
		{
			Control.ShapeLayer.UpdateStrokeThickness(Element.StrokeThickness);
		}

		void UpdateStrokeDashArray()
		{
			if (Element.StrokeDashArray == null || Element.StrokeDashArray.Count == 0)
				Control.ShapeLayer.UpdateStrokeDash(new nfloat[0]);
			else
			{
				nfloat[] dashArray;
				double[] array;

				if (Element.StrokeDashArray.Count % 2 == 0)
				{
					array = new double[Element.StrokeDashArray.Count];
					dashArray = new nfloat[Element.StrokeDashArray.Count];
					Element.StrokeDashArray.CopyTo(array, 0);
				}
				else
				{
					array = new double[2 * Element.StrokeDashArray.Count];
					dashArray = new nfloat[2 * Element.StrokeDashArray.Count];
					Element.StrokeDashArray.CopyTo(array, 0);
					Element.StrokeDashArray.CopyTo(array, Element.StrokeDashArray.Count);
				}

				double thickness = Element.StrokeThickness;

				for (int i = 0; i < array.Length; i++)
					dashArray[i] = new nfloat(thickness * array[i]);

				Control.ShapeLayer.UpdateStrokeDash(dashArray);
			}
		}

		void UpdateStrokeDashOffset()
		{
			Control.ShapeLayer.UpdateStrokeDashOffset((nfloat)Element.StrokeDashOffset);
		}

		void UpdateStrokeLineCap()
		{
			PenLineCap lineCap = Element.StrokeLineCap;
			CGLineCap iLineCap = CGLineCap.Butt;

			switch (lineCap)
			{
				case PenLineCap.Flat:
					iLineCap = CGLineCap.Butt;
					break;
				case PenLineCap.Square:
					iLineCap = CGLineCap.Square;
					break;
				case PenLineCap.Round:
					iLineCap = CGLineCap.Round;
					break;
			}

			Control.ShapeLayer.UpdateStrokeLineCap(iLineCap);
		}

		void UpdateStrokeLineJoin()
		{
			PenLineJoin lineJoin = Element.StrokeLineJoin;
			CGLineJoin iLineJoin = CGLineJoin.Miter;

			switch (lineJoin)
			{
				case PenLineJoin.Miter:
					iLineJoin = CGLineJoin.Miter;
					break;
				case PenLineJoin.Bevel:
					iLineJoin = CGLineJoin.Bevel;
					break;
				case PenLineJoin.Round:
					iLineJoin = CGLineJoin.Round;
					break;
			}

			Control.ShapeLayer.UpdateStrokeLineJoin(iLineJoin);
		}

		void UpdateStrokeMiterLimit()
		{
			Control.ShapeLayer.UpdateStrokeMiterLimit(new nfloat(Element.StrokeMiterLimit));
		}
	}

	public class ShapeView
#if __MOBILE__
	: UIView
#else
	: NSView
#endif
	{
		public ShapeView()
		{
#if __MOBILE__
			BackgroundColor = UIColor.Clear;
#else
			WantsLayer = true;
#endif
			ShapeLayer = new ShapeLayer();
			Layer.AddSublayer(ShapeLayer);
			Layer.MasksToBounds = false;
		}

		public ShapeLayer ShapeLayer
		{
			private set;
			get;
		}

#if !__MOBILE__
		public override bool IsFlipped => true;
#endif
	}

	public class ShapeLayer : CALayer
	{
		CGPath _path;
		CGRect _pathFillBounds;
		CGRect _pathStrokeBounds;

		CGPath _renderPath;
		CGRect _renderPathFill;
		CGRect _renderPathStroke;

		bool _fillMode;

		Brush _stroke;
		Brush _fill;

		nfloat _strokeWidth;
		nfloat[] _strokeDash;
		nfloat _dashOffset;

		Stretch _stretch;

		CGLineCap _strokeLineCap;
		CGLineJoin _strokeLineJoin;
		nfloat _strokeMiterLimit;

		public ShapeLayer()
		{
#if __MOBILE__
			ContentsScale = UIScreen.MainScreen.Scale;
#else
			ContentsScale = NSScreen.MainScreen.BackingScaleFactor;
#endif
			_fillMode = false;
			_stretch = Stretch.None;
			_strokeLineCap = CGLineCap.Butt;
			_strokeLineJoin = CGLineJoin.Miter;
			_strokeMiterLimit = 10;
		}

		public override void DrawInContext(CGContext ctx)
		{
			base.DrawInContext(ctx);
			RenderShape(ctx);
		}

		public void UpdateShape(CGPath path)
		{
			_path = path;

			if (_path != null)
				_pathFillBounds = _path.PathBoundingBox;
			else
				_pathFillBounds = new CGRect();

			UpdatePathStrokeBounds();
		}

		public void UpdateFillMode(bool fillMode)
		{
			_fillMode = fillMode;
			SetNeedsDisplay();
		}

		public SizeRequest GetDesiredSize()
		{
			return new SizeRequest(new Size(
				Math.Max(0, nfloat.IsNaN(_pathStrokeBounds.Right) ? 0 : _pathStrokeBounds.Right),
				Math.Max(0, nfloat.IsNaN(_pathStrokeBounds.Bottom) ? 0 : _pathStrokeBounds.Bottom)));
		}

		public void UpdateSize(CGSize size)
		{
			Bounds = new CGRect(new CGPoint(), size);
			BuildRenderPath();
		}

		public void UpdateAspect(Stretch stretch)
		{
			_stretch = stretch;
			BuildRenderPath();
		}

		public void UpdateFill(Brush fill)
		{
			_fill = fill;
			SetNeedsDisplay();
		}

		public void UpdateStroke(Brush stroke)
		{
			_stroke = stroke;
			SetNeedsDisplay();
		}

		public void UpdateStrokeThickness(double strokeWidth)
		{
			_strokeWidth = new nfloat(strokeWidth);
			BuildRenderPath();
		}

		public void UpdateStrokeDash(nfloat[] dash)
		{
			_strokeDash = dash;
			SetNeedsDisplay();
		}

		public void UpdateStrokeDashOffset(nfloat dashOffset)
		{
			_dashOffset = dashOffset;
			SetNeedsDisplay();
		}

		public void UpdateStrokeLineCap(CGLineCap strokeLineCap)
		{
			_strokeLineCap = strokeLineCap;
			UpdatePathStrokeBounds();
			SetNeedsDisplay();
		}

		public void UpdateStrokeLineJoin(CGLineJoin strokeLineJoin)
		{
			_strokeLineJoin = strokeLineJoin;
			UpdatePathStrokeBounds();
			SetNeedsDisplay();
		}

		public void UpdateStrokeMiterLimit(nfloat strokeMiterLimit)
		{
			_strokeMiterLimit = strokeMiterLimit;
			UpdatePathStrokeBounds();
			SetNeedsDisplay();
		}

		void BuildRenderPath()
		{
			if (_path == null)
			{
				_renderPath = null;
				_renderPathFill = new CGRect();
				_renderPathStroke = new CGRect();
				return;
			}

			CATransaction.Begin();
			CATransaction.DisableActions = true;

			if (_stretch != Stretch.None)
			{
				CGRect viewBounds = Bounds;
				viewBounds.X += _strokeWidth / 2;
				viewBounds.Y += _strokeWidth / 2;
				viewBounds.Width -= _strokeWidth;
				viewBounds.Height -= _strokeWidth;

				nfloat widthScale = viewBounds.Width / _pathFillBounds.Width;
				nfloat heightScale = viewBounds.Height / _pathFillBounds.Height;
				var stretchTransform = CGAffineTransform.MakeIdentity();

				switch (_stretch)
				{
					case Stretch.None:
						break;

					case Stretch.Fill:
						stretchTransform.Scale(widthScale, heightScale);

						stretchTransform.Translate(
							viewBounds.Left - widthScale * _pathFillBounds.Left,
							viewBounds.Top - heightScale * _pathFillBounds.Top);
						break;

					case Stretch.Uniform:
						nfloat minScale = NMath.Min(widthScale, heightScale);

						stretchTransform.Scale(minScale, minScale);

						stretchTransform.Translate(
							viewBounds.Left - minScale * _pathFillBounds.Left +
							(viewBounds.Width - minScale * _pathFillBounds.Width) / 2,
							viewBounds.Top - minScale * _pathFillBounds.Top +
							(viewBounds.Height - minScale * _pathFillBounds.Height) / 2);
						break;

					case Stretch.UniformToFill:
						nfloat maxScale = NMath.Max(widthScale, heightScale);

						stretchTransform.Scale(maxScale, maxScale);

						stretchTransform.Translate(
							viewBounds.Left - maxScale * _pathFillBounds.Left,
							viewBounds.Top - maxScale * _pathFillBounds.Top);
						break;
				}

				Frame = Bounds;
				_renderPath = _path.CopyByTransformingPath(stretchTransform);
			}
			else
			{
				nfloat adjustX = NMath.Min(0, _pathStrokeBounds.X);
				nfloat adjustY = NMath.Min(0, _pathStrokeBounds.Y);

				if (adjustX < 0 || adjustY < 0)
				{
					nfloat width = Bounds.Width;
					nfloat height = Bounds.Height;

					if (_pathStrokeBounds.Width > Bounds.Width)
						width = Bounds.Width - adjustX;
					if (_pathStrokeBounds.Height > Bounds.Height)
						height = Bounds.Height - adjustY;

					Frame = new CGRect(adjustX, adjustY, width, height);
					var transform = new CGAffineTransform(Bounds.Width / width, 0, 0, Bounds.Height / height, -adjustX, -adjustY);
					_renderPath = _path.CopyByTransformingPath(transform);
				}
				else
				{
					Frame = Bounds;
					_renderPath = _path.CopyByTransformingPath(CGAffineTransform.MakeIdentity());
				}
			}

			_renderPathFill = _renderPath.PathBoundingBox;
			_renderPathStroke = _renderPath.CopyByStrokingPath(_strokeWidth, _strokeLineCap, _strokeLineJoin, _strokeMiterLimit).PathBoundingBox;

			CATransaction.Commit();

			SetNeedsDisplay();
		}

		void RenderShape(CGContext graphics)
		{
			if (_path == null)
				return;

			if (_stroke == null && _fill == null)
				return;

			CATransaction.Begin();
			CATransaction.DisableActions = true;

			graphics.SetLineWidth(_strokeWidth);
			graphics.SetLineDash(_dashOffset * _strokeWidth, _strokeDash);
			graphics.SetLineCap(_strokeLineCap);
			graphics.SetLineJoin(_strokeLineJoin);
			graphics.SetMiterLimit(_strokeMiterLimit * _strokeWidth / 4);

			if (_fill is GradientBrush fillGradientBrush)
			{
				graphics.AddPath(_renderPath);

				if (_fillMode)
					graphics.Clip();
				else
					graphics.EOClip();

				RenderBrush(graphics, _renderPathFill, fillGradientBrush);
			}
			else
			{
				CGColor fillColor =
#if __MOBILE__
					UIColor.Clear.CGColor;
#else
					NSColor.Clear.CGColor;
#endif
				if (_fill is SolidColorBrush solidColorBrush && solidColorBrush.Color != null)
					fillColor = solidColorBrush.Color.ToCGColor();

				graphics.AddPath(_renderPath);
				graphics.SetFillColor(fillColor);
				graphics.DrawPath(_fillMode ? CGPathDrawingMode.FillStroke : CGPathDrawingMode.EOFillStroke);
			}

			if (_stroke is GradientBrush strokeGradientBrush)
			{
				graphics.AddPath(_renderPath);
				graphics.ReplacePathWithStrokedPath();
				graphics.Clip();
				RenderBrush(graphics, _renderPathStroke, strokeGradientBrush);
			}
			else
			{
				CGColor strokeColor =
#if __MOBILE__
					UIColor.Clear.CGColor;
#else
					NSColor.Clear.CGColor;
#endif
				if (_stroke is SolidColorBrush solidColorBrush && solidColorBrush.Color != null)
					strokeColor = solidColorBrush.Color.ToCGColor();

				graphics.AddPath(_renderPath);
				graphics.SetStrokeColor(strokeColor);
				graphics.DrawPath(CGPathDrawingMode.Stroke);
			}

			CATransaction.Commit();
		}

		void RenderBrush(CGContext graphics, CGRect pathBounds, GradientBrush brush)
		{
			if (brush == null)
				return;

			using (CGColorSpace rgb = CGColorSpace.CreateDeviceRGB())
			{
				CGColor[] colors = new CGColor[brush.GradientStops.Count];
				nfloat[] locations = new nfloat[brush.GradientStops.Count];

				for (int index = 0; index < brush.GradientStops.Count; index++)
				{
					Color color = brush.GradientStops[index].Color;
					colors[index] = new CGColor(new nfloat(color.Red), new nfloat(color.Green), new nfloat(color.Blue), new nfloat(color.Alpha));
					locations[index] = new nfloat(brush.GradientStops[index].Offset);
				}

				CGGradient gradient = new CGGradient(rgb, colors, locations);

				if (brush is LinearGradientBrush linearGradientBrush)
				{
					graphics.DrawLinearGradient(
						gradient,
						new CGPoint(pathBounds.Left + linearGradientBrush.StartPoint.X * pathBounds.Width, pathBounds.Top + linearGradientBrush.StartPoint.Y * pathBounds.Height),
						new CGPoint(pathBounds.Left + linearGradientBrush.EndPoint.X * pathBounds.Width, pathBounds.Top + linearGradientBrush.EndPoint.Y * pathBounds.Height),
						CGGradientDrawingOptions.DrawsBeforeStartLocation | CGGradientDrawingOptions.DrawsAfterEndLocation);
				}

				if (brush is RadialGradientBrush radialGradientBrush)
				{
					graphics.DrawRadialGradient(
						gradient,
						new CGPoint(radialGradientBrush.Center.X * pathBounds.Width + pathBounds.Left, radialGradientBrush.Center.Y * pathBounds.Height + pathBounds.Top),
						0.0f,
						new CGPoint(radialGradientBrush.Center.X * pathBounds.Width + pathBounds.Left, radialGradientBrush.Center.Y * pathBounds.Height + pathBounds.Top),
						(nfloat)(radialGradientBrush.Radius * Math.Max(pathBounds.Height, pathBounds.Width)),
						CGGradientDrawingOptions.DrawsBeforeStartLocation | CGGradientDrawingOptions.DrawsAfterEndLocation);
				}
			}
		}

		void UpdatePathStrokeBounds()
		{
			if (_path != null)
				_pathStrokeBounds = _path.CopyByStrokingPath(_strokeWidth, _strokeLineCap, _strokeLineJoin, _strokeMiterLimit).PathBoundingBox;
			else
				_pathStrokeBounds = new CGRect();

			BuildRenderPath();
		}
	}
}