using System;
using Foundation;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.GalleryPages.DateTimePickerGalleries;

[assembly: Dependency(typeof(Microsoft.Maui.Controls.Compatibility.ControlGallery.iOS.GalleryPages.RegionalLocale))]
namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.iOS.GalleryPages
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
