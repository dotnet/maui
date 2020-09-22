using System;
using System.Collections.Generic;
using System.Text;
using UIKit;

namespace Xamarin.Platform
{
	public static class ButtonExtensions
	{
		public static void UpdateColor(this UIButton nativeButton, IButton button)
		{
			// appCompatButton.SetTextColor(button.Color.ToNative());
		}

		public static void UpdateText(this UIButton nativeButton, IButton button) => nativeButton.SetText(button.Text);
	}
}
