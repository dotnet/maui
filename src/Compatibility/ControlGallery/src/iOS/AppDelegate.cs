using System;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Compatibility.Platform.iOS;
using Microsoft.Maui.Controls.ControlGallery;
using Microsoft.Maui.Controls.ControlGallery.iOS;
using Microsoft.Maui.Controls.ControlGallery.Issues;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using ObjCRuntime;
using UIKit;
using IOPath = System.IO.Path;
using Size = Microsoft.Maui.Graphics.Size;

[assembly: Dependency(typeof(TestCloudService))]
[assembly: Dependency(typeof(CacheService))]
[assembly: ExportEffect(typeof(BorderEffect), nameof(BorderEffect))]
namespace Microsoft.Maui.Controls.ControlGallery.iOS
{
	public class BorderEffect : PlatformEffect
	{
		protected override void OnAttached()
		{
			Control.BackgroundColor = UIColor.Blue;

			var childLabel = (Element as ScrollView)?.Content as Label;
			if (childLabel != null)
				childLabel.Text = "Success";
		}

		protected override void OnDetached()
		{
			Control.BackgroundColor = UIColor.Brown;
		}
	}

	public class CacheService : ICacheService
	{
		public void ClearImageCache()
		{
			var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			var cache = IOPath.Combine(documents, ".config", ".isolated-storage", "ImageLoaderCache");
			foreach (var file in Directory.GetFiles(cache))
			{
				File.Delete(file);
			}
		}
	}

	public class TestCloudService : ITestCloudService
	{
		public bool IsOnTestCloud()
		{
			var isInTestCloud = Environment.GetEnvironmentVariable("XAMARIN_TEST_CLOUD");

			return isInTestCloud != null && isInTestCloud.Equals("1", StringComparison.Ordinal);
		}

		public string GetTestCloudDeviceName()
		{
			return Environment.GetEnvironmentVariable("XTC_DEVICE_NAME");
		}

		public string GetTestCloudDevice()
		{
			return Environment.GetEnvironmentVariable("XTC_DEVICE");
		}
	}

	[Register("AppDelegate")]
	public partial class AppDelegate : MauiUIApplicationDelegate
	{
		protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

		public override bool FinishedLaunching(UIApplication uiApplication, NSDictionary launchOptions)
		{
			UISwitch.Appearance.OnTintColor = UIColor.Red;
			var versionPart = UIDevice.CurrentDevice.SystemVersion.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
			App.IOSVersion = int.Parse(versionPart[0]);

#if ENABLE_TEST_CLOUD
			Xamarin.Calabash.Start();
#endif

			//Forms.Init();
			//FormsMaps.Init();
			//FormsMaterial.Init();

			//Forms.ViewInitialized += (object sender, ViewInitializedEventArgs e) =>
			//{
			//	if (null != e.View.AutomationId && null != e.NativeView)
			//	{
			//		//	e.NativeView.AccessibilityIdentifier = e.View.StyleId;
			//	}
			//};

			if (App.IOSVersion == 11)
			{
				// 'Large' Title bar text
				UINavigationBar.Appearance.LargeTitleTextAttributes = new UIStringAttributes
				{
					ForegroundColor = UIColor.FromRGB(0xE7, 0x63, 0x3B), // e7963b dark
					Font = UIFont.FromName("GillSans-Italic", 40)
				};
			}

			// When the native control gallery loads up, it'll let us know so we can add the nested native controls
#pragma warning disable CS0618 // Type or member is obsolete
			//MessagingCenter.Subscribe<NestedNativeControlGalleryPage>(this, NestedNativeControlGalleryPage.ReadyForNativeControlsMessage, AddNativeControls);
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
			MessagingCenter.Subscribe<Bugzilla40911>(this, Bugzilla40911.ReadyToSetUp40911Test, SetUp40911Test);
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
			MessagingCenter.Subscribe<Issue5503>(this, Issue5503.ChangeUITableViewAppearanceBgColor, (s) =>
			{
				UITableView.Appearance.BackgroundColor = UITableView.Appearance.BackgroundColor == null ? UIColor.Red : null;
			});
#pragma warning restore CS0618 // Type or member is obsolete

			// When the native binding gallery loads up, it'll let us know so we can set up the native bindings
#pragma warning disable CS0618 // Type or member is obsolete
			//MessagingCenter.Subscribe<NativeBindingGalleryPage>(this, NativeBindingGalleryPage.ReadyForNativeBindingsMessage, AddNativeBindings);
#pragma warning restore CS0618 // Type or member is obsolete

			return base.FinishedLaunching(uiApplication, launchOptions);
		}





		#region Stuff for repro of Bugzilla case 40911

		void SetUp40911Test(Bugzilla40911 page)
		{
			var button = new Button { Text = "Start" };

			button.Clicked += (s, e) =>
			{
				StartPressed40911();
			};

			page._40911Layout.Children.Add(button);
		}

		public void StartPressed40911()
		{
			var loginViewController = new UIViewController { View = { BackgroundColor = UIColor.White } };
			var button = UIButton.FromType(UIButtonType.System);
			button.SetTitle("Login", UIControlState.Normal);
			button.Frame = new CGRect(20, 100, 200, 44);
			loginViewController.View.AddSubview(button);

			button.TouchUpInside += (sender, e) =>
			{
				Maui.Controls.Application.Current.MainPage = new ContentPage { Content = new Label { Text = "40911 Success" } };
				loginViewController.DismissViewController(true, null);
			};

#pragma warning disable CA1416 // TODO: 'UIApplication.KeyWindow' is unsupported on: 'ios' 13.0 and later
#pragma warning disable CA1422 // Validate platform compatibility
			var window = UIApplication.SharedApplication.KeyWindow;
#pragma warning restore CA1422 // Validate platform compatibility
#pragma warning restore CA1416
			var vc = window.RootViewController;
			while (vc.PresentedViewController != null)
			{
				vc = vc.PresentedViewController;
			}

			vc.PresentViewController(loginViewController, true, null);
		}

		#endregion

		[Export("navigateToTest:")]
		public string NavigateToTest(string test)
		{
			return (Microsoft.Maui.Controls.Application.Current as App).NavigateToTestPage(test).ToString();
		}

		[Export("reset:")]
		public string Reset(string str)
		{
			(Microsoft.Maui.Controls.Application.Current as App).Reset();
			return String.Empty;
		}

		[Export("iOSVersion")]
		public int iOSVersion()
		{
			return App.IOSVersion;
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
	}

	[Register("KVOUISlider")]
	public class KVOUISlider : UISlider
	{

		public KVOUISlider()
		{
			ValueChanged += (s, e) => KVOValue = Value;
		}

		float _kVOValue;
		[Export("kvovalue")]
		public float KVOValue
		{
			get
			{

				return _kVOValue;
			}
			set
			{

				WillChangeValue(nameof(KVOValue).ToLowerInvariant());
				_kVOValue = Value = value;
				DidChangeValue(nameof(KVOValue).ToLowerInvariant());
			}
		}
	}

	public class ColorConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is Graphics.Color)
				return ((Graphics.Color)value).ToPlatform();
			return value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is UIColor)
				return ((UIColor)value).ToColor();
			return value;
		}
	}
}
