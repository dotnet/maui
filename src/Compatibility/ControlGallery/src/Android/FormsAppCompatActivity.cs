using System.Net.Http;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Java.Interop;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
using Microsoft.Maui.Controls.ControlGallery;
using Microsoft.Maui.Controls.ControlGallery.Issues;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls.ControlGallery.Android
{
	// This is the AppCompat version of Activity1

	[Activity(Label = "Microsoft Maui Controls Compatibility Gallery", Icon = "@drawable/icon", Theme = "@style/MyTheme",
		MainLauncher = true, HardwareAccelerated = true,
		ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.UiMode)]
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
	[Register("com.microsoft.mauicompatibilitygallery.MainActivity")]
	public partial class Activity1 : MauiAppCompatActivity
	{
		App App => Microsoft.Maui.Controls.Application.Current as App;

		protected override void OnCreate(Bundle bundle)
		{
			Profile.Start();

			// Uncomment the next line to run this as a full screen app (no status bar)
			//Window.AddFlags(WindowManagerFlags.Fullscreen | WindowManagerFlags.TurnScreenOn);

			base.OnCreate(bundle);

#if !LEGACY_RENDERERS
#else
			Forms.SetFlags("UseLegacyRenderers");
#endif
			// null out the assembly on the Resource Manager
			// so all of our tests run without using the reflection APIs
			// At some point the Resources class types will go away so
			// reflection will stop working
			//ResourceManager.Init(null);

			////FormsMaterial.Init(this, bundle);
			//AndroidAppLinks.Init(this);
			//Forms.ViewInitialized += (sender, e) =>
			//{
			//	//				if (!string.IsNullOrWhiteSpace(e.View.StyleId)) {
			//	//					e.NativeView.ContentDescription = e.View.StyleId;
			//	//				}
			//};

			// uncomment to verify turning off title bar works. This is not intended to be dynamic really.
			//Forms.SetTitleBarVisibility (AndroidTitleBarVisibility.Never);

			// When the native control gallery loads up, it'll let us know so we can add the nested native controls
			// MessagingCenter.Subscribe<NestedNativeControlGalleryPage>(this, NestedNativeControlGalleryPage.ReadyForNativeControlsMessage, AddNativeControls);

			// When the native binding gallery loads up, it'll let us know so we can set up the native bindings
			// MessagingCenter.Subscribe<NativeBindingGalleryPage>(this, NativeBindingGalleryPage.ReadyForNativeBindingsMessage, AddNativeBindings);

			// Listen for the message from the status bar color toggle test
			// MessagingCenter.Subscribe<AndroidStatusBarColor>(this, AndroidStatusBarColor.Message, color => SetStatusBarColor(global::Android.Graphics.Color.Red));

			SetUpForceRestartTest();

			// Make the activity accessible to platform unit tests
			DependencyResolver.ResolveUsing((t) =>
			{
				if (t == typeof(Context))
				{
					return this;
				}

				return null;
			});

			DependencyService.Register<IMultiWindowService, MultiWindowService>();

#if LEGACY_RENDERERS
			if ((int)Build.VERSION.SdkInt >= 21)
			{
				// Show a purple status bar if we're looking at legacy renderers
				Window.SetStatusBarColor(Color.MediumPurple.ToAndroid());
			}
#endif
		}

		public void ReloadApplication()
		{
			App.Windows[0].Page = App.CreateDefaultMainPage();
		}

		protected override void OnResume()
		{
			base.OnResume();
			Profile.Stop();
		}

		[Export("hasInternetAccess")]
		public bool HasInternetAccess()
		{
			try
			{
				using (var httpClient = new HttpClient())
				using (var httpResponse = httpClient.GetAsync(@"https://www.github.com"))
				{
					httpResponse.Wait();
					if (httpResponse.Result.StatusCode == System.Net.HttpStatusCode.OK)
						return true;
					else
						return false;
				}
			}
			catch
			{
				return false;
			}
		}

		[Export("IsPreAppCompat")]
		public bool IsPreAppCompat()
		{
			return false;
		}

		[Java.Interop.Export("BackgroundApp")]
		public void BackgroundApp()
		{
			Intent intent = new Intent();
			intent.SetAction(Intent.ActionMain);
			intent.AddCategory(Intent.CategoryHome);
			this.StartActivity(intent);
		}

		[Java.Interop.Export("ForegroundApp")]
		public void ForegroundApp()
		{
			// this only works pre API 29
			Intent intent = new Intent(ApplicationContext, typeof(Activity1));
			intent.SetAction(Intent.ActionMain);
			intent.AddCategory(Intent.CategoryLauncher);
			this.ApplicationContext.StartActivity(intent);
		}
	}
}
