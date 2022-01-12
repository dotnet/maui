using System;
using Tizen.NUI;
using Tizen.UIExtensions.Common;
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
			_isPressed = false;
			return false;
		}
	}
}