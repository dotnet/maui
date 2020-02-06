#if !FORMS_APPLICATION_ACTIVITY && !PRE_APPLICATION_CLASS

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Java.Interop;
using Xamarin.Forms.Controls;
using Xamarin.Forms.Controls.Issues;
using Xamarin.Forms.Platform.Android;
using Xamarin.Forms.Platform.Android.AppLinks;
using System.Linq;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.ControlGallery.Android
{
	// This is the AppCompat version of Activity1

	[Activity(Label = "Control Gallery", Icon = "@drawable/icon", Theme = "@style/MyTheme",
		MainLauncher = true, HardwareAccelerated = true, 
		ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
	[IntentFilter(new[] { Intent.ActionView },
		Categories = new[]
		{
			Intent.ActionView,
			Intent.CategoryDefault,
			Intent.CategoryBrowsable
		},
		DataScheme = "http", DataHost = App.AppName, DataPathPrefix = "/gallery/"
		)
	]
	public partial class Activity1 : FormsAppCompatActivity
	{
		protected override void OnCreate(Bundle bundle)
		{
			Profile.Start();

			ToolbarResource = Resource.Layout.Toolbar;
			TabLayoutResource = Resource.Layout.Tabbar;

			// Uncomment the next line to run this as a full screen app (no status bar)
			//Window.AddFlags(WindowManagerFlags.Fullscreen | WindowManagerFlags.TurnScreenOn);

			base.OnCreate(bundle);

#if TEST_EXPERIMENTAL_RENDERERS
			// Fake_Flag is here so we can test for flag initialization issues
			Forms.SetFlags("Fake_Flag"/*, "CollectionView_Experimental", "Shell_Experimental"*/); 
#else
			Forms.SetFlags("UseLegacyRenderers", "SwipeView_Experimental", "MediaElement_Experimental");
#endif
			Forms.Init(this, bundle);

			FormsMaps.Init(this, bundle);
			DualScreen.DualScreenService.Init(this);
			FormsMaterial.Init(this, bundle);
			AndroidAppLinks.Init(this);
			Forms.ViewInitialized += (sender, e) => {
				//				if (!string.IsNullOrWhiteSpace(e.View.StyleId)) {
				//					e.NativeView.ContentDescription = e.View.StyleId;
				//				}
			};

			// uncomment to verify turning off title bar works. This is not intended to be dynamic really.
			//Forms.SetTitleBarVisibility (AndroidTitleBarVisibility.Never);

			if (RestartAppTest.App != null)
			{
				_app = (App)RestartAppTest.App;
				RestartAppTest.Reinit = true;
			}
			else
			{
				_app = new App();
			}

			// When the native control gallery loads up, it'll let us know so we can add the nested native controls
			MessagingCenter.Subscribe<NestedNativeControlGalleryPage>(this, NestedNativeControlGalleryPage.ReadyForNativeControlsMessage, AddNativeControls);

			// When the native binding gallery loads up, it'll let us know so we can set up the native bindings
			MessagingCenter.Subscribe<NativeBindingGalleryPage>(this, NativeBindingGalleryPage.ReadyForNativeBindingsMessage, AddNativeBindings);

			// Listen for the message from the status bar color toggle test
			MessagingCenter.Subscribe<AndroidStatusBarColor>(this, AndroidStatusBarColor.Message, color => SetStatusBarColor(global::Android.Graphics.Color.Red));

			SetUpForceRestartTest();

			// Make the activity accessible to platform unit tests
			DependencyResolver.ResolveUsing((t) => {
				if (t == typeof(Context))
				{
					return this;
				}

				return null;
			});

			LoadApplication(_app);
			if (Forms.Flags.Contains("FastRenderers_Experimental"))
			{
				var masterPage = ((_app.MainPage as MasterDetailPage)?.Master as ContentPage);
				if (masterPage != null)
					masterPage.Content = new Label { Text = "Fast Renderers" };
			}
		}

		protected override void OnResume()
		{
			base.OnResume();
			Profile.Stop();
		}

		[Export("IsPreAppCompat")]
		public bool IsPreAppCompat()
		{
			return false;
		}
	}
}

#endif
