using System;
using System.ComponentModel;
using ElmSharp;
using Tizen.NET.MaterialComponents;
using Xamarin.Forms;
using Xamarin.Forms.Material.Tizen;
using Xamarin.Forms.Platform.Tizen;
using Xamarin.Forms.Platform.Tizen.Native;
using EBox = ElmSharp.Box;
using EColor = ElmSharp.Color;

[assembly: ExportRenderer(typeof(Stepper), typeof(MaterialStepperRenderer), new[] { typeof(VisualMarker.MaterialVisual) })]
namespace Xamarin.Forms.Material.Tizen
{
	public class MaterialStepperRenderer : ViewRenderer<Stepper, EBox>
	{
		readonly int horizontalPadding = 10;
		readonly int minimumWidth = 100;
		readonly int minimumHeight = 50;
		readonly int radius = 5;
		readonly int borderWidth = 1;
		readonly EColor defaultColor = EColor.Black;

		BorderRectangle _borderL;
		BorderRectangle _borderR;
		MButton _buttonL;
		MButton _buttonR;

		double _mininum = Double.MinValue;
		double _maximum = Double.MaxValue;
		double _value = 0;
		double _increment = 1;

		protected override void OnElementChanged(ElementChangedEventArgs<Stepper> e)
		{
			if (Control == null)
			{
				var outter = new EBox(Forms.NativeParent)
				{
					AlignmentX = -1,
					AlignmentY = -1,
					WeightX = 1,
					WeightY = 1,
					IsHorizontal = true,
				};
				outter.SetPadding(horizontalPadding, 0);
				outter.Show();
				outter.SetLayoutCallback(OnLayout);

				_borderL = new BorderRectangle(Forms.NativeParent);
				_borderL.Show();
				_borderL.SetRadius(radius);
				_borderL.BorderWidth = borderWidth;
				_borderL.Color = defaultColor;

				_buttonL = new MButton(Forms.NativeParent)
				{
					BackgroundColor = EColor.Transparent,
					Text = "<span color='#000000'>-</span>",
				};
				_buttonL.Show();

				_buttonL.Clicked += OnDecrementButtonClicked;

				outter.PackEnd(_borderL);
				outter.PackEnd(_buttonL);

				_borderR = new BorderRectangle(Forms.NativeParent);
				_borderR.Show();
				_borderR.SetRadius(radius);
				_borderR.BorderWidth = borderWidth;
				_borderR.Color = defaultColor;

				_buttonR = new MButton(Forms.NativeParent)
				{
					BackgroundColor = EColor.Transparent,
					Text = "<span color='#000000'>+</span>",
				};
				_buttonR.Show();

				_buttonR.Clicked += OnIncrementButtonClicked;

				outter.PackEnd(_borderR);
				outter.PackEnd(_buttonR);

				SetNativeView(outter);
			}

			UpdateValue();
			UpdateIncrement();
			UpdateMinimum();
			UpdateMaximum();

			base.OnElementChanged(e);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == Stepper.ValueProperty.PropertyName)
			{
				UpdateValue();
			}
			else if (e.PropertyName == Stepper.IncrementProperty.PropertyName)
			{
				UpdateIncrement();
			}
			else if (e.PropertyName == Stepper.MaximumProperty.PropertyName)
			{
				UpdateMaximum();
			}
			else if (e.PropertyName == Stepper.MinimumProperty.PropertyName)
			{
				UpdateMinimum();
			}
		}

		protected override void Dispose(bool disposing)
		{
			if(disposing)
			{
				if(Control != null)
				{
					_buttonL.Clicked -= OnDecrementButtonClicked;
					_buttonR.Clicked -= OnIncrementButtonClicked;
				}
			}
			base.Dispose(disposing);
		}


		protected override ElmSharp.Size Measure(int availableWidth, int availableHeight)
		{
			var size = base.Measure(availableWidth, availableHeight);

			if (size.Width < minimumWidth)
			{
				size.Width = Forms.ConvertToScaledPixel(minimumWidth + horizontalPadding);
			}

			if (size.Height < minimumHeight)
			{
				size.Height = Forms.ConvertToScaledPixel(minimumHeight);
			}

			return size;
		}

		void OnLayout()
		{
			var x = Control.Geometry.X + horizontalPadding;
			var y = Control.Geometry.Y;
			var w = (int)(Control.Geometry.Width - (horizontalPadding * 3)) / 2;
			var h = Control.Geometry.Height;
			var rectL = new Rect(x, y, w, h);
			_borderL.Draw(rectL);
			_buttonL.Geometry = rectL;

			var x2 = Control.Geometry.X + w + (horizontalPadding * 2);
			var rectR = new Rect(x2, y, w, h);
			_borderR.Draw(rectR);
			_buttonR.Geometry = rectR;
		}

		void OnIncrementButtonClicked(object sender, EventArgs e)
		{
			if ( _value < _maximum)
			{
				_value = Math.Min((_value + _increment), _maximum);
				((IElementController)Element).SetValueFromRenderer(Stepper.ValueProperty, _value);
			}
		}

		void OnDecrementButtonClicked(object sender, EventArgs e)
		{
			if (_value > _mininum)
			{
				_value = Math.Max((_value - _increment), _mininum);
				((IElementController)Element).SetValueFromRenderer(Stepper.ValueProperty, _value);
			}
		}

		void UpdateValue()
		{
			_value = Element.Value;
		}

		void UpdateMinimum()
		{
			_mininum = Element.Minimum;
		}

		void UpdateMaximum()
		{
			_maximum = Element.Maximum;
		}

		void UpdateIncrement()
		{
			_increment = Element.Increment;
		}
	}
}
