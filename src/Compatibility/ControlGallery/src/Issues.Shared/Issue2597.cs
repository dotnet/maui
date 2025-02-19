using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Devices;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github5000)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 2597, "Stepper control .IsEnabled doesn't work", PlatformAffected.Android)]
	public class Issue2597 : TestContentPage
	{
		Label _label;

		protected override void Init()
		{
			Label header = new Label
			{
				Text = "Stepper",
				HorizontalOptions = LayoutOptions.Center
			};

			Stepper stepper = new Stepper
			{
				Minimum = 0,
				Maximum = 10,
				Increment = 0.1,
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.CenterAndExpand,
				IsEnabled = false
			};
			stepper.ValueChanged += OnStepperValueChanged;

			_label = new Label
			{
				Text = "Stepper value is 0",
				FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Label)),
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.CenterAndExpand
			};

			// Accomodate iPhone status bar.
			Padding = DeviceInfo.Platform == DevicePlatform.iOS ? new Thickness(10, 20, 10, 5) : new Thickness(10, 0, 10, 5);

			// Build the page.
			Content = new StackLayout
			{
				Children =
				{
					header,
					stepper,
					_label
				}
			};
		}

		void OnStepperValueChanged(object sender, ValueChangedEventArgs e)
		{
			_label.Text = string.Format("Stepper value is {0:F1}", e.NewValue);
		}

#if UITEST
		[Test]
		[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.UwpIgnore)]
		[Compatibility.UITests.FailsOnMauiIOS]
		public void Issue2597Test()
		{
#if __IOS__
			RunningApp.Tap(x => x.Marked("Increment"));
#else
			RunningApp.Tap("+");
#endif

			RunningApp.WaitForElement(q => q.Marked("Stepper value is 0"));


#if __IOS__
			RunningApp.Tap(x => x.Marked("Decrement"));
#else
			RunningApp.Tap("−");
#endif

			RunningApp.WaitForElement(q => q.Marked("Stepper value is 0"));
		}
#endif
	}
}

