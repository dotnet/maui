using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Compatibility.Platform.iOS;
using Microsoft.Maui.Controls.ControlGallery.iOS;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;
using ObjCRuntime;
using UIKit;

[assembly: ExportEffect(typeof(SearchbarEffect), "SearchbarEffect")]
namespace Microsoft.Maui.Controls.ControlGallery.iOS
{
	public class SearchbarEffect : PlatformEffect
	{
		UIColor _defaultBackColor;
		UIColor _defaultTintColor;
		UIImage _defaultBackImage;
		protected override void OnAttached()
		{
			if (_defaultBackColor == null)
				_defaultBackColor = Control.BackgroundColor;

			Control.BackgroundColor = Colors.Cornsilk.ToPlatform();

			if (Control is UISearchBar searchBar)
			{
				if (_defaultTintColor == null)
					_defaultTintColor = searchBar.BarTintColor;

				if (_defaultBackImage == null)
					_defaultBackImage = searchBar.BackgroundImage;

				searchBar.BarTintColor = Colors.Goldenrod.ToPlatform();
				searchBar.BackgroundImage = new UIImage();
			}
		}

		protected override void OnDetached()
		{
			Control.BackgroundColor = _defaultBackColor;

			if (Control is UISearchBar searchBar)
			{
				searchBar.BarTintColor = _defaultTintColor;
				searchBar.BackgroundImage = _defaultBackImage;
			}
		}
	}
}
