// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

using System;
using System.Globalization;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Xamarin.Forms;
using Xamarin.Forms.ControlGallery.WindowsUniversal;
using Xamarin.Forms.Controls;
using Xamarin.Forms.Platform.UWP;


namespace Xamarin.Forms.ControlGallery.WindowsUniversal
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class MainPage
	{
		Controls.App _app;

		public MainPage()
		{
			InitializeComponent();

			// some tests need to window to be large enough to click on things
			// can we make this only open to window size for UI Tests?
			//var bounds = ApplicationView.GetForCurrentView().VisibleBounds;
			//var scaleFactor = DisplayInformation.GetForCurrentView().RawPixelsPerViewPixel;
			//var size = new Windows.Foundation.Size(bounds.Width * scaleFactor, bounds.Height * scaleFactor);
			//ApplicationView.PreferredLaunchViewSize = size;
			//ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;


			_app = new Controls.App();

			// When the native control gallery loads up, it'll let us know so we can add the nested native controls
			MessagingCenter.Subscribe<NestedNativeControlGalleryPage>(this, NestedNativeControlGalleryPage.ReadyForNativeControlsMessage, AddNativeControls);

			// When the native binding gallery loads up, it'll let us know so we can set up the native bindings
			MessagingCenter.Subscribe<NativeBindingGalleryPage>(this, NativeBindingGalleryPage.ReadyForNativeBindingsMessage, AddNativeBindings);

			LoadApplication(_app);

			CoreWindow.GetForCurrentThread().KeyDown += OnKeyDown;

		}

		void OnKeyDown(CoreWindow coreWindow, KeyEventArgs args)
		{
			if (args.VirtualKey == VirtualKey.Escape)
			{
				_app.Reset();
				args.Handled = true;
			}
			else if (args.VirtualKey == VirtualKey.F1)
			{
				_app.PlatformTest();
			}
		}

		void AddNativeControls(NestedNativeControlGalleryPage page)
		{
			if (page.NativeControlsAdded)
			{
				return;
			}

			StackLayout sl = page.Layout;

			// Create and add a native TextBlock
			var originalText = "I am a native TextBlock";
			var textBlock = new TextBlock
			{
				Text = originalText,
				FontSize = 14,
				FontFamily = new FontFamily("HelveticaNeue")
			};

			sl?.Children.Add(textBlock);

			// Create and add a native Button 
			var button = new Windows.UI.Xaml.Controls.Button { Content = "Toggle Font Size", Height = 80 };
			button.Click += (sender, args) => { textBlock.FontSize = textBlock.FontSize == 14 ? 24 : 14; };

			sl?.Children.Add(button.ToView());

			// Create a control which we know doesn't behave correctly with regard to measurement
			var difficultControl = new BrokenNativeControl
			{
				Text = "Not Sized/Arranged Properly"
			};

			var difficultControl2 = new BrokenNativeControl
			{
				Text = "Fixed"
			};

			// Add the misbehaving controls, one with a custom delegate for ArrangeOverrideDelegate
			sl?.Children.Add(difficultControl);
			sl?.Children.Add(difficultControl2,
				arrangeOverrideDelegate: (renderer, finalSize) =>
				{
					if (finalSize.Width <= 0 || double.IsInfinity(finalSize.Width))
					{
						return null;
					}

					FrameworkElement frameworkElement = renderer.Control;

					frameworkElement.Measure(finalSize);

					// The broken control always tries to size itself to the screen width
					// So figure that out and we'll know how far off it's laying itself out
					Rect bounds = ApplicationView.GetForCurrentView().VisibleBounds;
					double scaleFactor = DisplayInformation.GetForCurrentView().RawPixelsPerViewPixel;
					var screenWidth = new Size(bounds.Width * scaleFactor, bounds.Height * scaleFactor);

					// We can re-center it by offsetting it during the Arrange call
					double diff = Math.Abs(screenWidth.Width - finalSize.Width) / -2;
					frameworkElement.Arrange(new Rect(diff, 0, finalSize.Width - diff, finalSize.Height));

					// Arranging the control to the left will make it show up past the edge of the stack layout
					// We can fix that by clipping it manually
					var clip = new RectangleGeometry { Rect = new Rect(-diff, 0, finalSize.Width, finalSize.Height) };
					frameworkElement.Clip = clip;

					return finalSize;
				}
			);

			page.NativeControlsAdded = true;
		}

		void AddNativeBindings(NativeBindingGalleryPage page)
		{
			if (page.NativeControlsAdded)
				return;

			StackLayout sl = page.Layout;

			var txbLabel = new TextBlock
			{
				FontSize = 14,
				FontFamily = new FontFamily("HelveticaNeue")
			};

			var txbBox = new TextBox
			{
				FontSize = 14,
				FontFamily = new FontFamily("HelveticaNeue")
			};

			var btnColor = new Windows.UI.Xaml.Controls.Button { Content = "Toggle Label Color", Height = 80 };
			btnColor.Click += (sender, args) => txbLabel.Foreground = new SolidColorBrush(Windows.UI.Colors.Pink);

			var btnTextBox = new Windows.UI.Xaml.Controls.Button { Content = "Change text textbox", Height = 80 };
			btnTextBox.Click += (sender, args) => txbBox.Text = "Hello 2 way native";

			txbLabel.SetBinding("Text", new Binding("NativeLabel"));
			txbBox.SetBinding("Text", new Binding("NativeLabel", BindingMode.TwoWay), "TextChanged");
			txbLabel.SetBinding("Foreground", new Binding("NativeLabelColor", BindingMode.TwoWay, new ColorToBrushNativeBindingConverter()));

			var grd = new StackPanel();
			grd.Children.Add(txbLabel);
			grd.Children.Add(btnColor);

			sl?.Children.Add(grd.ToView());

			sl?.Children.Add(txbBox);
			sl?.Children.Add(btnTextBox.ToView());

			page.NativeControlsAdded = true;
		}

		class ColorToBrushNativeBindingConverter : IValueConverter
		{
			public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
			{
				if (value is Color)
					return new SolidColorBrush(ToWindowsColor((Color)value));

				return null;
			}

			public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
			{
				if (value is SolidColorBrush)
					return ToColor(((SolidColorBrush)value).Color);

				return null;
			}
			public static Windows.UI.Color ToWindowsColor(Color color)
			{
				return Windows.UI.Color.FromArgb((byte)(color.A * 255), (byte)(color.R * 255), (byte)(color.G * 255), (byte)(color.B * 255));
			}
			public static Color ToColor(Windows.UI.Color color)
			{
				return Color.FromRgba(color.R, color.G, color.B, color.A);
			}
		}
	}
}
