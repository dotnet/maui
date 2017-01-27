using System;
using System.Globalization;
using AppKit;
using CoreGraphics;
using Foundation;
using Xamarin.Forms.Controls;
using Xamarin.Forms.Controls.Issues;
using Xamarin.Forms.Platform.MacOS;

namespace Xamarin.Forms.ControlGallery.MacOS
{
	[Register("AppDelegate")]
	public class AppDelegate : FormsApplicationDelegate
	{

		NSWindow _window;
		public AppDelegate()
		{
			ObjCRuntime.Runtime.MarshalManagedException += (sender, args) =>
			{
				Console.WriteLine(args.Exception.ToString());
			};

			var style = NSWindowStyle.Closable | NSWindowStyle.Resizable | NSWindowStyle.Titled;

			var rect = new CoreGraphics.CGRect(200, 1000, 1024, 768);
			//var rect = NSWindow.FrameRectFor(NSScreen.MainScreen.Frame, style);
			_window = new NSWindow(rect, style, NSBackingStore.Buffered, false);
			_window.Title = "Twitter XF Mac";
			_window.TitleVisibility = NSWindowTitleVisibility.Hidden;
		}

		public override NSWindow MainWindow
		{
			get { return _window; }
		}

		public override void DidFinishLaunching(NSNotification notification)
		{
			Forms.Init();
			FormsMaps.Init();

			var app = new App();
			// When the native control gallery loads up, it'll let us know so we can add the nested native controls
			MessagingCenter.Subscribe<NestedNativeControlGalleryPage>(this, NestedNativeControlGalleryPage.ReadyForNativeControlsMessage, AddNativeControls);
			MessagingCenter.Subscribe<Bugzilla40911>(this, Bugzilla40911.ReadyToSetUp40911Test, SetUp40911Test);

			// When the native binding gallery loads up, it'll let us know so we can set up the native bindings
			MessagingCenter.Subscribe<NativeBindingGalleryPage>(this, NativeBindingGalleryPage.ReadyForNativeBindingsMessage, AddNativeBindings);

			LoadApplication(app);
			base.DidFinishLaunching(notification);
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

			var uilabel = new NSTextField
			{
				StringValue = originalText,
				MaximumNumberOfLines = 0,
				LineBreakMode = NSLineBreakMode.ByWordWrapping,
				Font = NSFont.FromFontName("Helvetica", 24f)
			};

			sl?.Children.Add(uilabel);

			// Create and add a native Button 
			var uibutton = NSButtonExtensions.CreateButton("Toggle Text Amount", () =>
			{
				uilabel.StringValue = uilabel.StringValue == originalText ? longerText : originalText;
				uilabel.SizeToFit();
			});
			uibutton.Font = NSFont.FromFontName("Helvetica", 14f);


			sl?.Children.Add(uibutton.ToView());

			// Create some control which we know don't behave correctly with regard to measurement
			var difficultControl0 = new BrokenNativeControl
			{
				Font = NSFont.FromFontName("Helvetica", 14f),
				MaximumNumberOfLines = 0,
				LineBreakMode = NSLineBreakMode.ByWordWrapping,
				StringValue = "Doesn't play nice with sizing. That's why there's a big gap around it."
			};

			var difficultControl1 = new BrokenNativeControl
			{
				Font = NSFont.FromFontName("Helvetica", 14f),
				MaximumNumberOfLines = 0,
				LineBreakMode = NSLineBreakMode.ByWordWrapping,
				StringValue = "Custom size fix specified. No gaps."
			};

			var explanation0 = new NSTextField
			{
				StringValue = "The next control is a customized label with a bad SizeThatFits implementation.",
				MaximumNumberOfLines = 0,
				LineBreakMode = NSLineBreakMode.ByWordWrapping,
				Font = NSFont.FromFontName("Helvetica", 14f),
			};

			var explanation1 = new NSTextField
			{
				StringValue = "The next control is the same broken class as above, but we pass in an override to the GetDesiredSize method.",
				MaximumNumberOfLines = 0,
				LineBreakMode = NSLineBreakMode.ByWordWrapping,
				Font = NSFont.FromFontName("Helvetica", 14f),
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
			var badRect = uiView.FittingSize;

			// And we'll use the width (which is fine) and substitute our own height
			return new SizeRequest(new Size(badRect.Width, 20));
		}

		void AddNativeBindings(NativeBindingGalleryPage page)
		{
			if (page.NativeControlsAdded)
				return;

			StackLayout sl = page.Layout;

			int width = 200;
			int heightCustomLabelView = 100;

			var uilabel = new NSTextField(new CGRect(0, 0, width, heightCustomLabelView))
			{
				BackgroundColor = NSColor.Clear,
				Editable = false,
				Bezeled = false,
				DrawsBackground = false,
				MaximumNumberOfLines = 0,
				LineBreakMode = NSLineBreakMode.ByWordWrapping,
				Font = NSFont.FromFontName("Helvetica", 24f),
				StringValue = "DefaultText"
			};

			var uibuttonColor = NSButtonExtensions.CreateButton("Toggle Text Color Binding", () => uilabel.TextColor = NSColor.Blue);
			uibuttonColor.Font = NSFont.FromFontName("Helvetica", 14f);

			uilabel.SetBinding("StringValue", new Binding("NativeLabel"));
			uilabel.SetBinding(nameof(uilabel.TextColor), new Binding("NativeLabelColor", converter: new ColorConverter()));

			sl?.Children.Add(uilabel);
			sl?.Children.Add(uibuttonColor.ToView());
			//var colorPicker = new NSColorWell();
			//colorPicker.SetBinding("SelectedColor", new Binding("NativeLabelColor", BindingMode.TwoWay, new ColorConverter()), "ColorPicked");
			//sl?.Children.Add(colorPicker);
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

			page.Layout.Children.Add(button);
		}

		public void StartPressed40911()
		{
			var loginViewController = new NSViewController { View = { } };
			var button = NSButtonExtensions.CreateButton("Login", () =>
			{
				Xamarin.Forms.Application.Current.MainPage = new ContentPage { Content = new Label { Text = "40911 Success" } };
				//loginViewController.DismissViewController()true, null);

			});

			button.Frame = new CGRect(20, 100, 200, 44);
			loginViewController.View.AddSubview(button);

			var window = NSApplication.SharedApplication.KeyWindow;
			var vc = window.ContentViewController;
			while (vc.PresentedViewControllers.Length > 0)
			{
				vc = vc.PresentedViewControllers[0];
			}

			//vc.PresentViewController(loginViewController, new NSViewControllerPresentationAnimator();
		}

		#endregion

		public class ColorConverter : IValueConverter
		{
			public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
			{
				if (value is Color)
					return ((Color)value).ToNSColor();
				return value;
			}

			public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
			{
				if (value is NSColor)
					return ((NSColor)value).ToColor();
				return value;
			}
		}
	}
}

