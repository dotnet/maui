using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using WRectangle = Windows.UI.Xaml.Shapes.Rectangle;

namespace Xamarin.Forms.Platform.UWP
{
	public class ShellSplitView : SplitView
	{
		Color _flyoutBackdropColor;
		Brush _defaultBrush;
		LightDismissOverlayMode? _defaultLightDismissOverlayMode;
		public ShellSplitView()
		{
		}


		internal void UpdateFlyoutBackdropColor()
		{
			var dismissLayer = ((WRectangle)GetTemplateChild("LightDismissLayer"));

			if (dismissLayer == null)
				return;

			if (_defaultBrush == null)
				_defaultBrush = dismissLayer.Fill;

			if (_flyoutBackdropColor == Color.Default)
			{
				dismissLayer.Fill = _defaultBrush;
			}
			else
			{
				dismissLayer.Fill = _flyoutBackdropColor.ToBrush();
			}
		}

		internal Color FlyoutBackdropColor
		{
			set
			{
				if (_flyoutBackdropColor == value)
					return;

				_flyoutBackdropColor = value;

				if (_defaultLightDismissOverlayMode == null)
					_defaultLightDismissOverlayMode = LightDismissOverlayMode;

				if (value == Color.Default)
				{
					LightDismissOverlayMode = _defaultLightDismissOverlayMode ?? LightDismissOverlayMode.Auto;
				}
				else
				{
					LightDismissOverlayMode = LightDismissOverlayMode.On;
				}
			}
		}
	}
}
