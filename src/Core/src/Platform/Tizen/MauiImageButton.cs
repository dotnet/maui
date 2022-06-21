using System;
using Tizen.NUI;
using NColor = Tizen.NUI.Color;
using TImage = Tizen.UIExtensions.NUI.Image;

namespace Microsoft.Maui.Platform
{
	public class MauiImageButton : TImage
	{
		bool _isPressed;

		public event EventHandler? Pressed;
		public event EventHandler? Released;
		public event EventHandler? Clicked;

		public MauiImageButton()
		{
			TouchEvent += OnTouched;
			Border = Border = new Rectangle(0, 0, 0, 0);
		}

		public void UpdateStrokeColor(IButtonStroke button)
		{
			BorderlineColor = button.StrokeColor.ToNUIColor() ?? NColor.Transparent;
		}

		public void UpdateStrokeThickness(IButtonStroke button)
		{
			BorderlineWidth = button.StrokeThickness.ToScaledPixel();
		}

		public void UpdateCornerRadius(IButtonStroke button)
		{
			if (button.CornerRadius != -1)
				CornerRadius = ((double)button.CornerRadius).ToScaledPixel();
		}

		bool OnTouched(object source, TouchEventArgs e)
		{
			var state = e.Touch.GetState(0);

			if (state == PointStateType.Down)
			{
				_isPressed = true;
				Pressed?.Invoke(this, EventArgs.Empty);
				return true;
			}
			else if (state == PointStateType.Up)
			{
				Released?.Invoke(this, EventArgs.Empty);
				if (_isPressed)
				{
					Clicked?.Invoke(this, EventArgs.Empty);
				}
				_isPressed = false;
				return true;
			}
			return false;
		}
	}
}