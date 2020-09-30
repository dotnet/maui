#if PRE_APPLICATION_CLASS

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Xamarin.Forms.Controls;
using Xamarin.Forms.Platform.Android;

namespace Xamarin.Forms.ControlGallery.Android
{
	[Activity (Label = "Control Gallery", 
			   Icon = "@drawable/icon",
			   MainLauncher = true, 
			   HardwareAccelerated = true, 
			   ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
	public class Activity1 : AndroidActivity
	{
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			
			Forms.Init (this, bundle);
			FormsMaps.Init (this, bundle);

			SetPage (FormsApp.GetFormsApp ());

			MessagingCenter.Subscribe<RootPagesGallery, Type> (this, Messages.ChangeRoot, (sender, pageType) => {
				var page = ((Page)Activator.CreateInstance (pageType));
				SetPage (page);
			});

			MessagingCenter.Subscribe<RootPagesGallery, Type> (this, Messages.ChangeRoot, (sender, pageType) => {
				var page = ((Page)Activator.CreateInstance (pageType));
				SetPage (page);
			});

			MessagingCenter.Subscribe<HomeButton> (this, Messages.GoHome, (sender) => {
				var screen = FormsApp.GetFormsApp ();
				SetPage (screen);
 			});
		}
	}
}

#endif