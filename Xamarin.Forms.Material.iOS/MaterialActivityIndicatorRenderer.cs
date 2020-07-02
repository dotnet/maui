using System;
using System.ComponentModel;
using CoreAnimation;
using CoreGraphics;
using MaterialComponents;
using UIKit;
using Xamarin.Forms.Platform.iOS;
using MActivityIndicator = MaterialComponents.ActivityIndicator;


namespace Xamarin.Forms.Material.iOS
{
	public class MaterialActivityIndicatorRenderer : ViewRenderer<ActivityIndicator, MActivityIndicator>
	{
		// by Material spec the stroke width is 1/12 of the diameter, 
		// but Android's native progress indicator is 1/10 of the diameter.
		const float _strokeRatio = 10;
		const float _defaultRadius = 22;
		const float _defaultStrokeWidth = 4;
		const float _defaultSize = 2 * _defaultRadius + _defaultStrokeWidth;
		SemanticColorScheme _defaultColorScheme;
		SemanticColorScheme _colorScheme;
		CAShapeLayer _backgroundLayer;

		protected override void OnElementChanged(ElementChangedEventArgs<ActivityIndicator> e)
		{
			_colorScheme?.Dispose();
			_colorScheme = CreateColorScheme();

			base.OnElementChanged(e);

			if (e.NewElement != null)
			{
				if (Control == null)
				{
					_defaultColorScheme = CreateColorScheme();

					SetNativeControl(CreateNativeControl());

					_backgroundLayer = new CAShapeLayer
					{
						LineWidth = Control.StrokeWidth,
						FillColor = UIColor.Clear.CGColor,
						Hidden = true
					};
					Control.Layer.InsertSublayer(_backgroundLayer, 0);
				}

				UpdateColor();
				UpdateIsRunning();
				SetBackgroundColor(Element.BackgroundColor);

				ApplyTheme();
			}
		}

		protected virtual SemanticColorScheme CreateColorScheme()
		{
			return MaterialColors.Light.CreateColorScheme();
		}

		protected virtual void ApplyTheme()
		{
			ActivityIndicatorColorThemer.ApplySemanticColorScheme(_colorScheme, Control);
		}

		protected override MActivityIndicator CreateNativeControl()
		{
			return new MActivityIndicator
			{
				IndicatorMode = ActivityIndicatorMode.Indeterminate,
				StrokeWidth = _defaultStrokeWidth,
				Radius = _defaultRadius
			};
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();

			if (Control == null) return;
			// try get the radius for this size
			var min = NMath.Min(Control.Bounds.Width, Control.Bounds.Height);
			var stroke = min / _strokeRatio;
			var radius = min / 2;

			// but, in the end use the limit set by the control
			Control.Radius = radius;
			Control.StrokeWidth = Control.Radius / (_strokeRatio / 2);

			_backgroundLayer.LineWidth = Control.StrokeWidth;
			_backgroundLayer.Path = UIBezierPath.FromArc(Control.Center, Control.Radius - Control.StrokeWidth / 2, 0, 360, true).CGPath;
		}

		public override CGSize SizeThatFits(CGSize size)
		{
			if (nfloat.IsInfinity(size.Width))
				size.Width = _defaultSize;
			if (nfloat.IsInfinity(size.Height))
				size.Height = _defaultSize;
			var min = NMath.Min(size.Width, size.Height);
			size.Width = size.Height = min;
			return size;
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			var updatedTheme = false;
			if (e.PropertyName == ActivityIndicator.ColorProperty.PropertyName)
			{
				UpdateColor();
				updatedTheme = true;
			}
			else if (e.PropertyName == ActivityIndicator.IsRunningProperty.PropertyName)
			{
				UpdateIsRunning();
			}

			if (updatedTheme)
				ApplyTheme();
		}

		protected override void SetBackgroundColor(Color color)
		{
			if (_backgroundLayer == null)
				return;

			_backgroundLayer.Hidden = color.IsDefault;
			_backgroundLayer.StrokeColor = color.ToCGColor();
		}

		void UpdateColor() => _colorScheme.PrimaryColor = Element.Color.IsDefault ? _defaultColorScheme.PrimaryColor : Element.Color.ToUIColor();

		void UpdateIsRunning()
		{
			bool isRunning = Element.IsRunning;

			if (Control.Animating == isRunning)
				return;

			if (isRunning)
				Control.StartAnimating();
			else
				Control.StopAnimating();
		}
	}
}
