using System;
using Microsoft.Maui.Platform;

namespace Microsoft.Maui.Controls
{
	public partial class RadioButton
	{

		public static void MapContent(RadioButtonHandler handler, RadioButton radioButton)
		{

			var children = ((IVisualTreeElement)radioButton).GetVisualChildren();
			var nativeView = handler.NativeView;

			if (nativeView is Microsoft.Maui.Platform.ContentView cv)
			{
				cv.CrossPlatformMeasure = radioButton.CrossPlatformMeasure;
				cv.CrossPlatformArrange = radioButton.CrossPlatformArrange;
			}

			if (children.Count == 0)
			{
				nativeView.ClearSubviews();
				return;
			}


			var nativeChildView = ((IView)children[0]).ToNative(radioButton.MauiContext, true);

			if (nativeView.Subviews.Length == 1)
			{
				if(nativeView.Subviews[0] == nativeChildView)
					return;

				nativeView.ClearSubviews();
			}

			nativeView.AddSubview(nativeChildView);
		}
	}
}
