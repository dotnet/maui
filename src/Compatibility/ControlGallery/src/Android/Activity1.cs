using System;
using System.Diagnostics;
using System.Globalization;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Widget;
using Java.Interop;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.Android;
using Microsoft.Maui.Controls.Compatibility.ControlGallery;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
using Microsoft.Maui.Controls.Compatibility.Platform.Android.AppLinks;
using Android.Content;
using Android.Content.Res;
using Android.Views;
using AColor = Android.Graphics.Color;

[assembly: Dependency (typeof (CacheService))]
[assembly: Dependency (typeof (TestCloudService))]
[assembly: ExportRenderer (typeof (DisposePage), typeof (DisposePageRenderer))]
[assembly: ExportRenderer (typeof (DisposeLabel), typeof (DisposeLabelRenderer))]
[assembly: ExportEffect (typeof (BorderEffect), "BorderEffect")]

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Android
{
	public partial class Activity1 
	{
		App _app;

		void AddNativeControls (NestedNativeControlGalleryPage page)
		{
			if (page.NativeControlsAdded) {
				return;
			}

			StackLayout sl = page.Layout;

			// Create and add a native TextView
			var textView = new TextView (this) { Text = "I am a native TextView", TextSize = 14 };
			sl?.Children.Add (textView);

			// Create and add a native Button 
			var button = new global::Android.Widget.Button (this) { Text = "Click to change TextView font size" };
			float originalSize = textView.TextSize;
			button.Click += (sender, args) => { textView.TextSize = textView.TextSize == originalSize ? 24 : 14; };

			sl?.Children.Add (button.ToView ());

			// Create a control which we know doesn't behave correctly with regard to measurement
			var difficultControl0 = new BrokenNativeControl (this) {
				Text = "This native control doesn't play nice with sizing, which is why it's all squished to one side."
			};
			var difficultControl1 = new BrokenNativeControl (this) {
				Text = "Same control, but with a custom GetDesiredSize delegate to accomodate it's sizing problems."
			};

			// Add a misbehaving control 
			sl?.Children.Add (difficultControl0);

			// Add a misbehaving control with a custom delegate for GetDesiredSize
			sl?.Children.Add (difficultControl1, SizeBrokenControl);

			page.NativeControlsAdded = true;
		}

		static SizeRequest? SizeBrokenControl (NativeViewWrapperRenderer renderer,
			int widthConstraint, int heightConstraint)
		{
			global::Android.Views.View nativeView = renderer.Control;

			if ((widthConstraint == 0 && heightConstraint == 0) || nativeView == null) {
				return null;
			}

			int width = global::Android.Views.View.MeasureSpec.GetSize (widthConstraint);
			int widthSpec = global::Android.Views.View.MeasureSpec.MakeMeasureSpec (width * 2,
				global::Android.Views.View.MeasureSpec.GetMode (widthConstraint));
			nativeView.Measure (widthSpec, heightConstraint);
			var size = new Size (nativeView.MeasuredWidth, nativeView.MeasuredHeight);
			return new SizeRequest (size);
		}

		void AddNativeBindings(NativeBindingGalleryPage page)
		{
			if (page.NativeControlsAdded)
				return;

			StackLayout sl = page.Layout;

			var textView = new TextView(this)
			{
				TextSize = 14,
				Text = "This will be text"
			};

			var viewGroup = new LinearLayout(this);
			viewGroup.AddView(textView);

			var buttonColor = new global::Android.Widget.Button(this) { Text = "Change label Color" };
			buttonColor.Click += (sender, e) => textView.SetTextColor(Color.Blue.ToAndroid());

			var colorPicker = new ColorPickerView(this, 200, 200);

			textView.SetBinding(nameof(textView.Text), new Binding("NativeLabel"));
			//this doesn't work because there's not TextColor property
			//textView.SetBinding("TextColor", new Binding("NativeLabelColor", converter: new ColorConverter()));
			colorPicker.SetBinding(nameof(colorPicker.SelectedColor), new Binding("NativeLabelColor", BindingMode.TwoWay, new ColorConverter()), "ColorPicked");

			sl?.Children.Add(viewGroup);
			sl?.Children.Add(buttonColor.ToView());
			sl?.Children.Add(colorPicker);

			page.NativeControlsAdded = true;
		}

		public class ColorConverter : IValueConverter
		{
			public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
			{
				if (value is Color)
					return ((Color)value).ToAndroid();

				return null;
			}

			public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
			{
				if (value is global::Android.Graphics.Color)
					return ((global::Android.Graphics.Color)value).ToColor();

				return null;
			}
		}

		[Export("NavigateToTest")]
		public bool NavigateToTest(string test)
		{
			return _app.NavigateToTestPage(test);
		}

		[Export("Reset")]
		public void Reset()
		{
			_app.Reset();
		}

		void SetUpForceRestartTest()
		{
			// Listen for messages from the app restart test
			MessagingCenter.Subscribe<RestartAppTest>(this, RestartAppTest.ForceRestart, (e) =>
			{
				// We can force a restart by making a configuration change; in this case, we'll enter
				// Car Mode. (The easy way to do this is to change the orientation, but ControlGallery
				// handles orientation changes so they don't cause a restart.)

				var uiModeManager = UiModeManager.FromContext(this);

				if (uiModeManager.CurrentModeType == UiMode.TypeCar)
				{
					// If for some reason we're already in car mode, disable it
					uiModeManager.DisableCarMode(DisableCarModeFlags.None);
				}
				
				uiModeManager.EnableCarMode(EnableCarModeFlags.None);

				// And put things back to normal so we can keep running tests
				uiModeManager.DisableCarMode(DisableCarModeFlags.None);

				((App)Microsoft.Maui.Controls.Application.Current).Reset();
			});
		}
	}
}

