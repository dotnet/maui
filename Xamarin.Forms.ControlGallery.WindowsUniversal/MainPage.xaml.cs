// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

using System;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Xamarin.Forms.Controls;
using Xamarin.Forms.Platform.UWP;

namespace Xamarin.Forms.ControlGallery.WindowsUniversal
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class MainPage
    {
        public MainPage()
        {
            InitializeComponent();

			var app = new Controls.App ();

			var mdp = app.MainPage as MasterDetailPage;

			var detail = mdp?.Detail as NavigationPage;
			if (detail != null) {
				detail.Pushed += (sender, args) => {
					var nncgPage = args.Page as NestedNativeControlGalleryPage;

					if (nncgPage != null) {
						AddNativeControls (nncgPage);
					}
				};
			} 

			LoadApplication (app);
        }

		void AddNativeControls (NestedNativeControlGalleryPage page)
		{
			if (page.NativeControlsAdded) {
				return;
			}

			StackLayout sl = page.Layout;

			// Create and add a native TextBlock
			var originalText = "I am a native TextBlock";
			var textBlock = new TextBlock {
				Text = originalText,
				FontSize = 14,
				FontFamily = new FontFamily ("HelveticaNeue")
			};

			sl?.Children.Add (textBlock);

			// Create and add a native Button 
			var button = new Windows.UI.Xaml.Controls.Button { Content = "Toggle Font Size", Height = 80 };
			button.Click += (sender, args) => { textBlock.FontSize = textBlock.FontSize == 14 ? 24 : 14; };

			sl?.Children.Add (button.ToView ());

			// Create a control which we know doesn't behave correctly with regard to measurement
			var difficultControl = new BrokenNativeControl {
				Text = "Not Sized/Arranged Properly"
			};

			var difficultControl2 = new BrokenNativeControl {
				Text = "Fixed"
			};

			// Add the misbehaving controls, one with a custom delegate for ArrangeOverrideDelegate
			sl?.Children.Add (difficultControl);
			sl?.Children.Add (difficultControl2,
				arrangeOverrideDelegate: (renderer, finalSize) => {
					if (finalSize.Width <= 0 || double.IsInfinity (finalSize.Width)) {
						return null;
					}

					FrameworkElement frameworkElement = renderer.Control;

					frameworkElement.Measure (finalSize);

					// The broken control always tries to size itself to the screen width
					// So figure that out and we'll know how far off it's laying itself out
					Rect bounds = ApplicationView.GetForCurrentView ().VisibleBounds;
					double scaleFactor = DisplayInformation.GetForCurrentView ().RawPixelsPerViewPixel;
					var screenWidth = new Size (bounds.Width * scaleFactor, bounds.Height * scaleFactor);
					
					// We can re-center it by offsetting it during the Arrange call
					double diff = Math.Abs(screenWidth.Width - finalSize.Width) / -2;
					frameworkElement.Arrange (new Rect (diff, 0, finalSize.Width - diff, finalSize.Height));

					// Arranging the control to the left will make it show up past the edge of the stack layout
					// We can fix that by clipping it manually
					var clip = new RectangleGeometry { Rect = new Rect (-diff, 0, finalSize.Width, finalSize.Height) };
					frameworkElement.Clip = clip;

					return finalSize;
				}
			);

			page.NativeControlsAdded = true;
		}
    }
}
