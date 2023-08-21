//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Android.App;
//using Android.Content;
//using Android.Content.PM;
//using Android.OS;
//using Android.Renderscripts;
//using Android.Runtime;
//using Android.Views;
//using Android.Widget;
//using Microsoft.Maui.Controls.ControlGallery.Issues;

using System;
using Foundation;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.ControlGallery.GalleryPages.DateTimePickerGalleries;

[assembly: Dependency(typeof(Microsoft.Maui.Controls.ControlGallery.iOS.GalleryPages.RegionalLocale))]
namespace Microsoft.Maui.Controls.ControlGallery.iOS.GalleryPages
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
