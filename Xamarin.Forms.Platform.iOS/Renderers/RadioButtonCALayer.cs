using CoreAnimation;
using CoreGraphics;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	public class RadioButtonCALayer : CALayer
	{
		const float _checkLineWidth = 2f;
		const float _containerLineWidth = 2f;
		const bool _fillsOnChecked = true;
		const float _checkSize = 25f;
		const float _containerInset = 5f;
		const float _checkInset = 9f;
		const UIColor _checkBorderStrokeColor = null;
		const UIColor _checkBorderFillColor = null;
		const UIColor _checkMarkStrokeColor = null;
		const UIColor _checkMarkFillColor = null;

		readonly UIButton _nativeControl;
		readonly RadioButton _radioButton;
		readonly CAShapeLayer _checkLayer = new CAShapeLayer();
		readonly CAShapeLayer _containerLayer = new CAShapeLayer();

		public RadioButtonCALayer(RadioButton radioButton, UIButton nativeControl)
		{
			NeedsDisplayOnBoundsChange = true;
			_radioButton = radioButton;
			_nativeControl = nativeControl;
			InitializeLayers();
			Frame = GetCheckFrame(_nativeControl.Bounds);
		}

		public override void Display()
		{
			base.Display();
			ColorLayers();
		}

		public override void LayoutSublayers()
		{
			base.LayoutSublayers();
			LayoutLayers();
		}

		void InitializeLayers()
		{
			_nativeControl.Layer.AddSublayer(_containerLayer);
			_nativeControl.Layer.AddSublayer(_checkLayer);
		}

		void ColorLayers()
		{
			if (_nativeControl.Enabled)
			{
				if (_radioButton.IsChecked)
				{
					_containerLayer.StrokeColor = _checkBorderStrokeColor?.CGColor ?? _nativeControl.CurrentTitleColor.CGColor;
					_containerLayer.FillColor = _checkMarkFillColor?.CGColor ?? UIColor.White.CGColor;
					_checkLayer.FillColor = _checkBorderFillColor?.CGColor ?? _nativeControl.CurrentTitleColor.CGColor;
					_checkLayer.StrokeColor = _checkMarkStrokeColor?.CGColor ?? _nativeControl.CurrentTitleColor.CGColor;
				}
				else
				{
					_containerLayer.StrokeColor = _checkBorderStrokeColor?.CGColor ?? _nativeControl.CurrentTitleColor.CGColor;
					_containerLayer.FillColor = UIColor.Clear.CGColor;
					_checkLayer.FillColor = UIColor.Clear.CGColor;
					_checkLayer.StrokeColor = UIColor.Clear.CGColor;
				}
			}
			else
			{
				// When disabled, use tint disabled color
				if (_radioButton.IsChecked)
				{
					_containerLayer.StrokeColor = _nativeControl.CurrentTitleColor.CGColor;
					_containerLayer.FillColor = _nativeControl.CurrentTitleColor.CGColor;
					_checkLayer.FillColor = _nativeControl.CurrentTitleColor.CGColor;
					_checkLayer.StrokeColor = _nativeControl.CurrentTitleColor.CGColor;
				}
				else
				{
					_containerLayer.StrokeColor = _nativeControl.CurrentTitleColor.CGColor;
					_containerLayer.FillColor = UIColor.Clear.CGColor;
					_checkLayer.FillColor = UIColor.Clear.CGColor;
					_checkLayer.StrokeColor = UIColor.Clear.CGColor;
				}
			}
		}

		void LayoutLayers()
		{
			CGRect checkFrame = GetCheckFrame(_nativeControl.Bounds);
			_containerLayer.Frame = _nativeControl.Bounds;
			_containerLayer.LineWidth = _containerLineWidth;
			_containerLayer.Path = GetContainerPath(checkFrame);

			_checkLayer.Frame = _nativeControl.Bounds;
			_checkLayer.LineWidth = _checkLineWidth;
			_checkLayer.Path = GetCheckPath(checkFrame);
		}

		protected virtual CGRect GetFrame(CGRect bounds)
		{
			return new CGRect(0, bounds.Height / 2 - _checkSize / 2, _checkSize, _checkSize);
		}

		protected virtual CGRect GetCheckFrame(CGRect bounds)
		{
			return new CGRect(0, bounds.Height / 2 - _checkSize / 2, _checkSize, _checkSize);
		}

		protected virtual CGPath GetContainerPath(CGRect frame)
		{
			frame = CGRect.Inflate(frame, -_containerInset, -_containerInset);
			frame.X -= _containerInset - 1;
			return UIBezierPath.FromOval(frame).CGPath;
		}

		protected virtual CGPath GetCheckPath(CGRect frame)
		{
			frame = CGRect.Inflate(frame, -_checkInset, -_checkInset);
			frame.X -= _containerInset - 1;
			return UIBezierPath.FromOval(frame).CGPath;
		}
	}
}