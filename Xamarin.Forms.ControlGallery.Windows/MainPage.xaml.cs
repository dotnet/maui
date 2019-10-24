using System;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Xamarin.Forms.Controls;
using Xamarin.Forms.Platform.WinRT;

namespace Xamarin.Forms.ControlGallery.Windows
{
	public sealed partial class MainPage
	{
		public MainPage ()
		{
			InitializeComponent ();

			var app = new Controls.App ();

			// When the native control gallery loads up, it'll let us know so we can add the nested native controls
			MessagingCenter.Subscribe<NestedNativeControlGalleryPage>(this, NestedNativeControlGalleryPage.ReadyForNativeControlsMessage, AddNativeControls);

			LoadApplication (app);
		}

		static void AddNativeControls (NestedNativeControlGalleryPage page)
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
			var button = new global::Windows.UI.Xaml.Controls.Button { Content = "Click to toggle font size", Height = 80 };
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

					// The broken control tries sizes itself to be the width of the screen
					var wrongSize = Window.Current.Bounds.Width * (int)DisplayProperties.ResolutionScale / 100;

					// We can re-center it by offsetting it during the Arrange call
					double diff = Math.Abs(finalSize.Width - wrongSize) / -2;
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