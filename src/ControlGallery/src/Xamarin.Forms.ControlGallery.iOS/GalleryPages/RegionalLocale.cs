using System;
using Foundation;
using Xamarin.Forms;
using Xamarin.Forms.Controls.GalleryPages.DateTimePickerGalleries;

[assembly: Dependency(typeof(Xamarin.Forms.ControlGallery.iOS.GalleryPages.RegionalLocale))]
namespace Xamarin.Forms.ControlGallery.iOS.GalleryPages
{
	public class RegionalLocale : ILocalize
	{
		public string GetCurrentCultureInfo()
		{
			string iOSLocale = NSLocale.CurrentLocale.CountryCode;
			string iOSLanguage = NSLocale.CurrentLocale.LanguageCode;
			return iOSLanguage + "-" + iOSLocale;
		}
	}
}
