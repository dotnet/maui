using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using CoreGraphics;
using Foundation;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.ControlGallery.iOS;
using Xamarin.Forms.Controls;
using Xamarin.Forms.Controls.Issues;
using Xamarin.Forms.Platform.iOS;

[assembly: Dependency(typeof(TestCloudService))]
[assembly: Dependency(typeof(CacheService))]
[assembly: ExportRenderer(typeof(DisposePage), typeof(DisposePageRenderer))]
[assembly: ExportRenderer(typeof(DisposeLabel), typeof(DisposeLabelRenderer))]
[assembly: ExportEffect(typeof(BorderEffect), nameof(BorderEffect))]
namespace Xamarin.Forms.ControlGallery.iOS
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
			var cache = Path.Combine(documents, ".config", ".isolated-storage", "ImageLoaderCache");
			foreach (var file in Directory.GetFiles(cache))
			{
				File.Delete(file);
			}
		}
	}

	public class DisposePageRenderer : PageRenderer
	{
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				((DisposePage)Element).SendRendererDisposed();
			}
			base.Dispose(disposing);

		}
	}

	public class DisposeLabelRenderer : LabelRenderer
	{
		protected override void Dispose(bool disposing)
		{

			if (disposing)
			{
				((DisposeLabel)Element).SendRendererDisposed();
			}
			base.Dispose(disposing);
		}
	}

	public class TestCloudService : ITestCloudService
	{
		public bool IsOnTestCloud()
		{
			var isInTestCloud = Environment.GetEnvironmentVariable("XAMARIN_TEST_CLOUD");

			return isInTestCloud != null && isInTestCloud.Equals("1");
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

#if PRE_APPLICATION_CLASS
	[Register ("AppDelegate")]
	public partial class AppDelegate : UIApplicationDelegate
	{
		 UIWindow window;

		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			// create a new window instance based on the screen size
			window = new UIWindow (UIScreen.MainScreen.Bounds);

			UINavigationBar.Appearance.TintColor = Color.Yellow.ToUIColor ();
			UINavigationBar.Appearance.BarTintColor = Color.Green.ToUIColor ();

			//override navigation bar title with text attributes
			UINavigationBar.Appearance.SetTitleTextAttributes(new UITextAttributes () {
				TextColor = Color.Pink.ToUIColor ()
			});

			Xamarin.Calabash.Start ();
			Forms.Init ();
			FormsMaps.Init ();
			window.RootViewController = FormsApp.GetFormsApp ().CreateViewController ();
		
			MessagingCenter.Subscribe<RootPagesGallery, Type> (this, Messages.ChangeRoot, (sender, pagetype) => {
				window = new UIWindow (UIScreen.MainScreen.Bounds);
				window.RootViewController = ((Page) Activator.CreateInstance(pagetype)).CreateViewController();
				window.MakeKeyAndVisible ();
			});

			MessagingCenter.Subscribe<HomeButton> (this, Messages.GoHome, (sender) => {
				window = new UIWindow (UIScreen.MainScreen.Bounds);
				window.RootViewController = FormsApp.GetFormsApp ().CreateViewController ();
				window.MakeKeyAndVisible ();
			});

			// make the window visible
			window.MakeKeyAndVisible ();

			return true;
		}
	}

#else
	[Register("AppDelegate")]
	public partial class AppDelegate : FormsApplicationDelegate
	{
		App _app;

		public override bool FinishedLaunching(UIApplication uiApplication, NSDictionary launchOptions)
		{
			UISwitch.Appearance.OnTintColor = UIColor.Red;
			var versionPart = UIDevice.CurrentDevice.SystemVersion.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
			App.IOSVersion = int.Parse(versionPart[0]);

			Xamarin.Calabash.Start();
			Forms.SetFlags("SwipeView_Experimental", "MediaElement_Experimental");
			Forms.Init();
			FormsMaps.Init();
			FormsMaterial.Init();

			Forms.ViewInitialized += (object sender, ViewInitializedEventArgs e) =>
			{
				// http://developer.xamarin.com/recipes/testcloud/set-accessibilityidentifier-ios/
				if (null != e.View.AutomationId && null != e.NativeView)
				{
					//	e.NativeView.AccessibilityIdentifier = e.View.StyleId;
				}
			};

			if (App.IOSVersion == 11)
			{
				// 'Large' Title bar text
				UINavigationBar.Appearance.LargeTitleTextAttributes = new UIStringAttributes
				{
					ForegroundColor = UIColor.FromRGB(0xE7, 0x63, 0x3B), // e7963b dark
					Font = UIFont.FromName("GillSans-Italic", 40)
				};
			}

			var app = new App();
			_app = app;

			// When the native control gallery loads up, it'll let us know so we can add the nested native controls
			MessagingCenter.Subscribe<NestedNativeControlGalleryPage>(this, NestedNativeControlGalleryPage.ReadyForNativeControlsMessage, AddNativeControls);
			MessagingCenter.Subscribe<Bugzilla40911>(this, Bugzilla40911.ReadyToSetUp40911Test, SetUp40911Test);
			MessagingCenter.Subscribe<Issue5503>(this, Issue5503.ChangeUITableViewAppearanceBgColor, (s) =>
			{
				UITableView.Appearance.BackgroundColor = UITableView.Appearance.BackgroundColor == null ? UIColor.Red : null;
			});

			// When the native binding gallery loads up, it'll let us know so we can set up the native bindings
			MessagingCenter.Subscribe<NativeBindingGalleryPage>(this, NativeBindingGalleryPage.ReadyForNativeBindingsMessage, AddNativeBindings);

			LoadApplication(app);

			return base.FinishedLaunching(uiApplication, launchOptions);
		}

		void AddNativeControls(NestedNativeControlGalleryPage page)
		{
			if (page.NativeControlsAdded)
			{
				return;
			}

			StackLayout sl = page.Layout;

			// Create and add a native UILabel
			var originalText = "I am a native UILabel";
			var longerText =
				"I am a native UILabel with considerably more text. Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.";

			var uilabel = new UILabel
			{
				MinimumFontSize = 14f,
				Text = originalText,
				Lines = 0,
				LineBreakMode = UILineBreakMode.WordWrap,
				Font = UIFont.FromName("Helvetica", 24f)
			};

			sl?.Children.Add(uilabel);

			// Create and add a native Button 
			var uibutton = new UIButton(UIButtonType.System);
			uibutton.SetTitle("Toggle Text Amount", UIControlState.Normal);
			uibutton.Font = UIFont.FromName("Helvetica", 14f);


			uibutton.TouchUpInside += (sender, args) =>
			{
				uilabel.Text = uilabel.Text == originalText ? longerText : originalText;
				uilabel.SizeToFit();
			};

			sl?.Children.Add(uibutton.ToView());

			// Create some control which we know don't behave correctly with regard to measurement
			var difficultControl0 = new BrokenNativeControl
			{
				MinimumFontSize = 14f,
				Font = UIFont.FromName("Helvetica", 14f),
				Lines = 0,
				LineBreakMode = UILineBreakMode.WordWrap,
				Text = "Doesn't play nice with sizing. That's why there's a big gap around it."
			};

			var difficultControl1 = new BrokenNativeControl
			{
				MinimumFontSize = 14f,
				Font = UIFont.FromName("Helvetica", 14f),
				Lines = 0,
				LineBreakMode = UILineBreakMode.WordWrap,
				Text = "Custom size fix specified. No gaps."
			};

			var explanation0 = new UILabel
			{
				MinimumFontSize = 14f,
				Text = "The next control is a customized label with a bad SizeThatFits implementation.",
				Lines = 0,
				LineBreakMode = UILineBreakMode.WordWrap,
				Font = UIFont.FromName("Helvetica", 24f)
			};

			var explanation1 = new UILabel
			{
				MinimumFontSize = 14f,
				Text = "The next control is the same broken class as above, but we pass in an override to the GetDesiredSize method.",
				Lines = 0,
				LineBreakMode = UILineBreakMode.WordWrap,
				Font = UIFont.FromName("Helvetica", 24f)
			};

			// Add a misbehaving control
			sl?.Children.Add(explanation0);
			sl?.Children.Add(difficultControl0);

			// Add the misbehaving control with a custom delegate for FixSize
			sl?.Children.Add(explanation1);
			sl?.Children.Add(difficultControl1, FixSize);

			page.NativeControlsAdded = true;
		}

		SizeRequest? FixSize(NativeViewWrapperRenderer renderer, double width, double height)
		{
			var uiView = renderer.Control;
			var view = renderer.Element;

			if (uiView == null || view == null)
			{
				return null;
			}

			var constraint = new CGSize(width, height);

			// Let the BrokenNativeControl determine its size (which we know will be wrong)
			var badRect = uiView.SizeThatFits(constraint);

			// And we'll use the width (which is fine) and substitute our own height
			return new SizeRequest(new Size(badRect.Width, 20));
		}

		void AddNativeBindings(NativeBindingGalleryPage page)
		{
			if (page.NativeControlsAdded)
				return;

			StackLayout sl = page.Layout;

			int width = (int)sl.Width;
			int heightCustomLabelView = 100;

			var uilabel = new UILabel(new RectangleF(0, 0, width, heightCustomLabelView))
			{
				MinimumFontSize = 14f,
				Lines = 0,
				LineBreakMode = UILineBreakMode.WordWrap,
				Font = UIFont.FromName("Helvetica", 24f),
				Text = "DefaultText"
			};

			var uibuttonColor = new UIButton(UIButtonType.System);
			uibuttonColor.SetTitle("Toggle Text Color Binding", UIControlState.Normal);
			uibuttonColor.Font = UIFont.FromName("Helvetica", 14f);
			uibuttonColor.TouchUpInside += (sender, args) => uilabel.TextColor = UIColor.Blue;

			var nativeColorConverter = new ColorConverter();

			uilabel.SetBinding("Text", new Binding("NativeLabel"));
			uilabel.SetBinding(nameof(uilabel.TextColor), new Binding("NativeLabelColor", converter: nativeColorConverter));

			var kvoSlider = new KVOUISlider();
			kvoSlider.MaxValue = 100;
			kvoSlider.MinValue = 0;
			kvoSlider.SetBinding(nameof(kvoSlider.KVOValue), new Binding("Age", BindingMode.TwoWay));
			sl?.Children.Add(kvoSlider);

			var uiView = new UIView(new RectangleF(0, 0, width, heightCustomLabelView));
			uiView.Add(uilabel);
			sl?.Children.Add(uiView);
			sl?.Children.Add(uibuttonColor.ToView());
			var colorPicker = new AdvancedColorPicker.ColorPickerView(new CGRect(0, 0, width, 300));
			colorPicker.SetBinding("SelectedColor", new Binding("NativeLabelColor", BindingMode.TwoWay, nativeColorConverter), "ColorPicked");
			sl?.Children.Add(colorPicker);
			page.NativeControlsAdded = true;
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
				Xamarin.Forms.Application.Current.MainPage = new ContentPage { Content = new Label { Text = "40911 Success" } };
				loginViewController.DismissViewController(true, null);
			};

			var window = UIApplication.SharedApplication.KeyWindow;
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
			// According to https://developer.xamarin.com/guides/testcloud/uitest/working-with/backdoors/
			// this method has to return a string
			return _app.NavigateToTestPage(test).ToString();
		}

		[Export("reset:")]
		public string Reset(string str)
		{
			_app.Reset();
			return String.Empty;
		}

		[Export("iOSVersion")]
		public int iOSVersion()
		{
			return App.IOSVersion;
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

				WillChangeValue(nameof(KVOValue).ToLower());
				_kVOValue = Value = value;
				DidChangeValue(nameof(KVOValue).ToLower());
			}
		}
	}

	public class ColorConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is Color)
				return ((Color)value).ToUIColor();
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
#endif
