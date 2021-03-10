using CoreAnimation;
using CoreGraphics;
using UIKit;

namespace Microsoft.Maui
{
	public class RadioButtonCALayer : CALayer
	{
		const float CheckLineWidth = 2f;
		const float ContainerLineWidth = 2f;
		const float CheckSize = 25f;
		const float ContainerInset = 5f;
		const float CheckInset = 9f;

		const UIColor CheckBorderStrokeColor = null;
		const UIColor CheckBorderFillColor = null;
		const UIColor CheckMarkStrokeColor = null;
		const UIColor CheckMarkFillColor = null;

		readonly IRadioButton? _radioButton;
		readonly UIButton? _nativeControl;

		readonly CAShapeLayer _checkLayer = new CAShapeLayer();
		readonly CAShapeLayer _containerLayer = new CAShapeLayer();

		public RadioButtonCALayer(IRadioButton? radioButton, UIButton? nativeControl)
		{
			NeedsDisplayOnBoundsChange = true;

			_radioButton = radioButton;
			_nativeControl = nativeControl;

			InitializeLayers();
			Frame = GetCheckFrame(_nativeControl?.Bounds ?? CGRect.Empty);
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
			if (_nativeControl == null)
				return;

			_nativeControl.Layer.AddSublayer(_containerLayer);
			_nativeControl.Layer.AddSublayer(_checkLayer);
		}

		void ColorLayers()
		{
			if (_nativeControl == null || _radioButton == null)
				return;

			if (_nativeControl.Enabled)
			{
				if (_radioButton.IsChecked)
				{
					_containerLayer.StrokeColor = CheckBorderStrokeColor?.CGColor ?? _nativeControl.CurrentTitleColor.CGColor;
					_containerLayer.FillColor = CheckMarkFillColor?.CGColor ?? UIColor.White.CGColor;
					_checkLayer.FillColor = CheckBorderFillColor?.CGColor ?? _nativeControl.CurrentTitleColor.CGColor;
					_checkLayer.StrokeColor = CheckMarkStrokeColor?.CGColor ?? _nativeControl.CurrentTitleColor.CGColor;
				}
				else
				{
					_containerLayer.StrokeColor = CheckBorderStrokeColor?.CGColor ?? _nativeControl.CurrentTitleColor.CGColor;
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
			if (_nativeControl == null)
				return;

			CGRect checkFrame = GetCheckFrame(_nativeControl.Bounds);
			_containerLayer.Frame = _nativeControl.Bounds;
			_containerLayer.LineWidth = ContainerLineWidth;
			_containerLayer.Path = GetContainerPath(checkFrame);

			_checkLayer.Frame = _nativeControl.Bounds;
			_checkLayer.LineWidth = CheckLineWidth;
			_checkLayer.Path = GetCheckPath(checkFrame);
		}

		protected virtual CGRect GetFrame(CGRect bounds)
		{
			return new CGRect(0, bounds.Height / 2 - CheckSize / 2, CheckSize, CheckSize);
		}

		protected virtual CGRect GetCheckFrame(CGRect bounds)
		{
			return new CGRect(0, bounds.Height / 2 - CheckSize / 2, CheckSize, CheckSize);
		}

		protected virtual CGPath? GetContainerPath(CGRect frame)
		{
			frame = CGRect.Inflate(frame, -ContainerInset, -ContainerInset);
			frame.X -= ContainerInset - 1;
			return UIBezierPath.FromOval(frame).CGPath;
		}

		protected virtual CGPath? GetCheckPath(CGRect frame)
		{
			frame = CGRect.Inflate(frame, -CheckInset, -CheckInset);
			frame.X -= ContainerInset - 1;
			return UIBezierPath.FromOval(frame).CGPath;
		}
	}
}