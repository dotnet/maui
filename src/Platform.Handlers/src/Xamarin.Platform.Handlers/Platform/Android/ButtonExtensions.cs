using System;
using System.Collections.Generic;
using System.Text;
using AndroidX.AppCompat.Widget;

namespace Xamarin.Platform
{
	public static class ButtonExtensions
	{
		public static void UpdateColor(this AppCompatButton appCompatButton, IButton button) => 
			appCompatButton.SetTextColor(button.Color.ToNative());		

		public static void UpdateText(this AppCompatButton appCompatButton, IButton button) =>
			appCompatButton.Text = button.Text;
	}
}
