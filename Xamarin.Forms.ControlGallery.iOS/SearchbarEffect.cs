using System;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.ControlGallery.iOS;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportEffect(typeof(SearchbarEffect), "SearchbarEffect")]
namespace Xamarin.Forms.ControlGallery.iOS
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

			Control.BackgroundColor = Color.Cornsilk.ToUIColor();
			
			if (Control is UISearchBar searchBar)
			{
				if (_defaultTintColor == null)
					_defaultTintColor = searchBar.BarTintColor;

				if (_defaultBackImage == null)
					_defaultBackImage = searchBar.BackgroundImage;

				searchBar.BarTintColor = Color.Goldenrod.ToUIColor();
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
