using System;
using System.ComponentModel;
using System.Diagnostics;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Platform;

namespace Maui.Controls.Sample.Controls
{
	public class FocusPlatformEffect : PlatformEffect
	{
#if __ANDROID__
		Android.Graphics.Color originalBackgroundColor = new Android.Graphics.Color(0, 0, 0, 0);
		Android.Graphics.Color backgroundColor = default!;
#elif __IOS__
		UIKit.UIColor backgroundColor = default!;
#elif TIZEN
		Tizen.NUI.Color backgroundColor = default!;
#endif

		public FocusPlatformEffect()
			: base()
		{
		}

		protected override void OnAttached()
		{
			try
			{
#if WINDOWS
				(Control as Microsoft.UI.Xaml.Controls.Control)!.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Cyan);
#elif __ANDROID__
				backgroundColor = Android.Graphics.Color.LightGreen;
				Control.SetBackgroundColor(backgroundColor);
#elif __IOS__
				Control.BackgroundColor = backgroundColor = UIKit.UIColor.FromRGB(204, 153, 255);
#elif TIZEN
				(Control as Tizen.NUI.BaseComponents.View).BackgroundColor = backgroundColor = Tizen.NUI.Color.LightGreen;
#endif
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Cannot set property on attached control. Error: ", ex.Message);
			}
		}

		protected override void OnDetached()
		{
		}

		protected override void OnElementPropertyChanged(PropertyChangedEventArgs args)
		{
			base.OnElementPropertyChanged(args);

			try
			{
#if __ANDROID__
				if (args.PropertyName == "IsFocused")
				{
					if (((Android.Graphics.Drawables.ColorDrawable)Control.Background!).Color == backgroundColor)
					{
						Control.SetBackgroundColor(originalBackgroundColor);
					}
					else
					{
						Control.SetBackgroundColor(backgroundColor);
					}
				}
#elif __IOS__
				if (args.PropertyName == "IsFocused")
				{
					if (Control.BackgroundColor == backgroundColor)
					{
						Control.BackgroundColor = UIKit.UIColor.White;
					}
					else
					{
						Control.BackgroundColor = backgroundColor;
					}
				}
#elif TIZEN
				if (args.PropertyName == "IsFocused")
				{
					if (Control.BackgroundColor == backgroundColor)
					{
						Control.BackgroundColor = Tizen.NUI.Color.White;
					}
					else
					{
						Control.BackgroundColor = backgroundColor;
					}
				}
#endif
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Cannot set property on attached control. Error: ", ex.Message);
			}
		}
	}
}