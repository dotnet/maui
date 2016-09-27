using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Xamarin.Forms.ControlGallery.WP8.Resources;
using Xamarin.Forms;
using Xamarin.Forms.ControlGallery.WP8;
using Xamarin.Forms.Controls;
using Xamarin.Forms.Maps.WP8;
using Xamarin.Forms.Platform.WinPhone;

[assembly: Dependency (typeof (StringProvider))]
namespace Xamarin.Forms.ControlGallery.WP8
{
	public class StringProvider : IStringProvider
	{
		public string CoreGalleryTitle { get { return "WP8 Core Gallery"; } }
	}

#if PRE_APPLICATION_CLASS
	public partial class MainPage : PhoneApplicationPage
	{
		public MainPage ()
		{
			InitializeComponent();

			Forms.Init ();
			FormsMaps.Init ();

			Content = CoreGallery.GetMainPage ().ConvertPageToUIElement (this);

			MessagingCenter.Subscribe<RootPagesGallery, Type>(this, Messages.ChangeRoot, (sender, pagetype) =>
			{
				var page = ((Page) Activator.CreateInstance(pagetype));
				app.MainPage = page;
			});

			MessagingCenter.Subscribe<HomeButton>(this, Messages.GoHome, (sender) => {
				var page = FormsApp.GetFormsApp ();
				app.MainPage = page;
			});
		}
	}

#else
	public partial class MainPage : FormsApplicationPage
	{
		// Constructor
		public MainPage()
		{
			InitializeComponent();

			Forms.Init ();
			FormsMaps.Init ();

			var app = new Controls.App ();

			// When the native control gallery loads up, it'll let us know so we can add the nested native controls
			MessagingCenter.Subscribe<NestedNativeControlGalleryPage>(this, NestedNativeControlGalleryPage.ReadyForNativeControlsMessage, AddNativeControls);

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
			var button = new System.Windows.Controls.Button { Content = "Click to toggle font size", Height = 80 };
			button.Click += (sender, args) => { textBlock.FontSize = textBlock.FontSize == 14 ? 24 : 14; };

			sl?.Children.Add (button.ToView ());

			// Create a control which we know doesn't behave correctly with regard to measurement
			var difficultControl = new BrokenNativeControl {
				Text = "Not Sized/Arranged Properly"
			};

			var difficultControl2 = new BrokenNativeControl {
				Text = "Fixed"
			};

			// Add the misbehaving controls, one with a custom delegate for ArrangeOverride
			sl?.Children.Add (difficultControl);
			sl?.Children.Add (difficultControl2,
				arrangeOverrideDelegate: (renderer, finalSize) => {
					if (finalSize.Width <= 0 || double.IsInfinity (finalSize.Width)) {
						return null;
					}

					FrameworkElement frameworkElement = renderer.Control;

					frameworkElement.Measure (finalSize);

					// The broken control always sizes itself to be 600 wide
					// We can re-center it by offsetting it during the Arrange call
					double diff = (finalSize.Width - 600) / 2;
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
#endif
}
