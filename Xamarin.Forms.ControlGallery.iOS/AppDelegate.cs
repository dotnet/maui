using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting.Lifetime;
using System.IO;
#if __UNIFIED__
using UIKit;
using Foundation;
using CoreGraphics;
using AdvancedColorPicker;
#else
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using MonoTouch.CoreGraphics;
#endif
using Xamarin.Forms;
using Xamarin.Forms.ControlGallery.iOS;
using Xamarin.Forms.Controls;
using Xamarin.Forms.Maps.iOS;
using Xamarin.Forms.Platform.iOS;
using System.Globalization;

[assembly: Dependency (typeof (TestCloudService))]
[assembly: Dependency (typeof (StringProvider))]
[assembly: Dependency (typeof (CacheService))]
[assembly: ExportRenderer (typeof (DisposePage), typeof(DisposePageRenderer))]
[assembly: ExportRenderer (typeof (DisposeLabel), typeof(DisposeLabelRenderer))]
namespace Xamarin.Forms.ControlGallery.iOS
{
	public class CacheService : ICacheService
	{
		public void ClearImageCache ()
		{
			var documents = Environment.GetFolderPath (Environment.SpecialFolder.MyDocuments);
			var cache = Path.Combine (documents, ".config", ".isolated-storage", "ImageLoaderCache");
			foreach (var file in Directory.GetFiles (cache)) {
				File.Delete (file); 
			}
		}
	}

	public class DisposePageRenderer : PageRenderer
	{
		protected override void Dispose (bool disposing)
		{
			if (disposing) {
				((DisposePage) Element).SendRendererDisposed ();
			}
			base.Dispose (disposing);

		}
	}

	public class DisposeLabelRenderer : LabelRenderer
	{
		protected override void Dispose (bool disposing)
		{

			if (disposing) {
				((DisposeLabel) Element).SendRendererDisposed ();
			}
			base.Dispose (disposing);
		}
	}

	public class StringProvider : IStringProvider
	{
		public string CoreGalleryTitle {
			get { return "iOS Core Gallery"; }
		}
	}

	public class TestCloudService : ITestCloudService
	{
		public bool IsOnTestCloud ()
		{
			var isInTestCloud = Environment.GetEnvironmentVariable ("XAMARIN_TEST_CLOUD");
		
			return isInTestCloud != null && isInTestCloud.Equals("1");
		}

		public string GetTestCloudDeviceName ()
		{
			return Environment.GetEnvironmentVariable ("XTC_DEVICE_NAME");
		}

		public string GetTestCloudDevice()
		{
			return Environment.GetEnvironmentVariable ("XTC_DEVICE");
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
	[Register ("AppDelegate")]
	public partial class AppDelegate : FormsApplicationDelegate
	{

		public override bool FinishedLaunching (UIApplication uiApplication, NSDictionary launchOptions)
		{
			App.IOSVersion = int.Parse (UIDevice.CurrentDevice.SystemVersion.Substring (0, 1));

	#if !_CLASSIC_
			Xamarin.Calabash.Start ();	
	#endif
			Forms.Init ();
			FormsMaps.Init ();
			Forms.ViewInitialized += (object sender, ViewInitializedEventArgs e) => {
				// http://developer.xamarin.com/recipes/testcloud/set-accessibilityidentifier-ios/
				if (null != e.View.AutomationId && null != e.NativeView) {
				//	e.NativeView.AccessibilityIdentifier = e.View.StyleId;
				}
			};

			var app = new App ();

			var mdp = app.MainPage as MasterDetailPage;
			var detail = mdp?.Detail as NavigationPage;
			if (detail != null) {
				detail.Pushed += (sender, args) => {
					var nncgPage = args.Page as NestedNativeControlGalleryPage;

					if (nncgPage != null) {
						AddNativeControls (nncgPage);
					}

					var nncgPage1 = args.Page as NativeBindingGalleryPage;

					if (nncgPage1 != null)
					{
						AddNativeBindings(nncgPage1);
					}
				};
			}	

			LoadApplication (app);
			return base.FinishedLaunching (uiApplication, launchOptions);
		}

		void AddNativeControls (NestedNativeControlGalleryPage page)
		{
			if (page.NativeControlsAdded) {
				return;
			}

			StackLayout sl = page.Layout;

			// Create and add a native UILabel
			var originalText = "I am a native UILabel";
			var longerText =
				"I am a native UILabel with considerably more text. Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.";

			var uilabel = new UILabel {
				MinimumFontSize = 14f,
				Text = originalText,
				Lines = 0,
				LineBreakMode = UILineBreakMode.WordWrap,
				Font = UIFont.FromName ("Helvetica", 24f)
			};

			sl?.Children.Add (uilabel);

			// Create and add a native Button 
			var uibutton = new UIButton (UIButtonType.RoundedRect);
			uibutton.SetTitle ("Toggle Text Amount", UIControlState.Normal);
			uibutton.Font = UIFont.FromName ("Helvetica", 14f);
			

			uibutton.TouchUpInside += (sender, args) => {
				uilabel.Text = uilabel.Text == originalText ? longerText : originalText;
				uilabel.SizeToFit ();
			};

			sl?.Children.Add (uibutton.ToView ());

			// Create some control which we know don't behave correctly with regard to measurement
			var difficultControl0 = new BrokenNativeControl {
				MinimumFontSize = 14f,
				Font = UIFont.FromName ("Helvetica", 14f),
				Lines = 0,
				LineBreakMode = UILineBreakMode.WordWrap,
				Text = "Doesn't play nice with sizing. That's why there's a big gap around it."
			};

			var difficultControl1 = new BrokenNativeControl {
				MinimumFontSize = 14f,
				Font = UIFont.FromName ("Helvetica", 14f),
				Lines = 0,
				LineBreakMode = UILineBreakMode.WordWrap,
				Text = "Custom size fix specified. No gaps."
			};

			var explanation0 = new UILabel {
				MinimumFontSize = 14f,
				Text = "The next control is a customized label with a bad SizeThatFits implementation.",
				Lines = 0,
				LineBreakMode = UILineBreakMode.WordWrap,
				Font = UIFont.FromName ("Helvetica", 24f)
			};

			var explanation1 = new UILabel {
				MinimumFontSize = 14f,
				Text = "The next control is the same broken class as above, but we pass in an override to the GetDesiredSize method.",
				Lines = 0,
				LineBreakMode = UILineBreakMode.WordWrap,
				Font = UIFont.FromName ("Helvetica", 24f)
			};

			// Add a misbehaving control
			sl?.Children.Add (explanation0);
			sl?.Children.Add (difficultControl0);

			// Add the misbehaving control with a custom delegate for FixSize
			sl?.Children.Add (explanation1);
			sl?.Children.Add (difficultControl1, FixSize);

			page.NativeControlsAdded = true;
		}

		SizeRequest? FixSize (NativeViewWrapperRenderer renderer, double width, double height)
		{
			var uiView = renderer.Control;
			var view = renderer.Element;

			if (uiView == null || view == null) {
				return null;
			}

#if __UNIFIED__
			var constraint = new CGSize (width, height);
#else
			var constraint = new SizeF ((float)width, (float)height);
#endif
			
			// Let the BrokenNativeControl determine its size (which we know will be wrong)
			var badRect = uiView.SizeThatFits (constraint);

			// And we'll use the width (which is fine) and substitute our own height
			return new SizeRequest (new Size (badRect.Width, 20));
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

			var uibuttonColor = new UIButton(UIButtonType.RoundedRect);
			uibuttonColor.SetTitle("Toggle Text Color Binding", UIControlState.Normal);
			uibuttonColor.Font = UIFont.FromName("Helvetica", 14f);
			uibuttonColor.TouchUpInside += (sender, args) => uilabel.TextColor = UIColor.Blue;

			var nativeColorConverter = new ColorConverter();

			uilabel.SetBinding("Text", new Binding("NativeLabel"));
			uilabel.SetBinding(nameof(uilabel.TextColor), new Binding("NativeLabelColor", converter: nativeColorConverter));

			var uiView = new UIView(new RectangleF(0, 0, width, heightCustomLabelView));
			uiView.Add(uilabel);
			sl?.Children.Add(uiView);
			sl?.Children.Add(uibuttonColor.ToView());
#if !_CLASSIC_
			var colorPicker = new ColorPickerView(new CGRect(0, 0, width, 300));
			colorPicker.SetBinding("SelectedColor", new Binding("NativeLabelColor", BindingMode.TwoWay, nativeColorConverter), "ColorPicked");
			sl?.Children.Add(colorPicker);
#endif
			page.NativeControlsAdded = true;
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
#endif
}
