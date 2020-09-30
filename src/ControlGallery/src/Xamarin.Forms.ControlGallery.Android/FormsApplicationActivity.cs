#if FORMS_APPLICATION_ACTIVITY

using System.Diagnostics;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Java.Interop;
using Xamarin.Forms.Controls;
using Xamarin.Forms.Controls.Issues;
using Xamarin.Forms.Platform.Android;

namespace Xamarin.Forms.ControlGallery.Android
{
	// This is the Pre-AppCompat version of Activity1

[Activity(Label = "Control Gallery", Icon = "@drawable/icon", MainLauncher = true,
		HardwareAccelerated = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
	public partial class Activity1 : FormsApplicationActivity
	{
		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);

			Forms.SetFlags("Fake_Flag"); // So we can test for flag initialization issues

			Forms.Init(this, bundle);
			FormsMaps.Init(this, bundle);
			Forms.ViewInitialized += (sender, e) => {
				if (!string.IsNullOrWhiteSpace(e.View.StyleId))
				{
					e.NativeView.ContentDescription = e.View.StyleId;
				}
			};

			// uncomment to verify turning off title bar works. This is not intended to be dynamic really.
			//Forms.SetTitleBarVisibility (AndroidTitleBarVisibility.Never);

			var app = _app = new App();

			// When the native control gallery loads up, it'll let us know so we can add the nested native controls
			MessagingCenter.Subscribe<NestedNativeControlGalleryPage>(this, NestedNativeControlGalleryPage.ReadyForNativeControlsMessage, AddNativeControls);

			// When the native binding gallery loads up, it'll let us know so we can set up the native bindings
			MessagingCenter.Subscribe<NativeBindingGalleryPage>(this, NativeBindingGalleryPage.ReadyForNativeBindingsMessage, AddNativeBindings);

			SetUpForceRestartTest();

			LoadApplication(app);
		}

		[Export("IsPreAppCompat")]
		public bool IsPreAppCompat()
		{
			return true;
		}
	}
}

#endif