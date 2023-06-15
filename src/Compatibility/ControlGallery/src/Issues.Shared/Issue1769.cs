using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Devices;

#if UITEST
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github5000)]
	[NUnit.Framework.Category(UITestCategories.Switch)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1769, "PushAsync with Switch produces NRE", PlatformAffected.Android)]
	public class Issue1769 : TestContentPage
	{
		const string GoToPageTwoButtonText = "Go To Page 2";
		const string SwitchLabelText = "Switch";
		const string SwitchAutomatedId = nameof(SwitchAutomatedId);
		const string SwitchIsNowLabelTextFormat = "Switch is now {0}";

		class SwitchDemoPage : ContentPage
		{
			readonly Label _label;

			public SwitchDemoPage()
			{
				var header = new Label
				{
					Text = SwitchLabelText,
					FontSize = 50,
					HorizontalOptions = LayoutOptions.Center
				};

				var switcher = new Switch
				{
					AutomationId = SwitchAutomatedId,
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.CenterAndExpand
				};
				switcher.Toggled += switcher_Toggled;

				_label = new Label
				{
					Text = string.Format(SwitchIsNowLabelTextFormat, switcher.IsToggled),
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
						switcher,
						_label
					}
				};
			}

			void switcher_Toggled(object sender, ToggledEventArgs e)
			{
				_label.Text = string.Format(SwitchIsNowLabelTextFormat, e.Value);
			}
		}

		protected override void Init()
		{
			var button = new Button()
			{
				Text = GoToPageTwoButtonText
			};
			button.Clicked += async (sender, args) =>
			{
				await ((Button)sender).Navigation.PushAsync(new SwitchDemoPage());
			};

			Content = button;
		}

#if UITEST
		[Test]
		[Compatibility.UITests.FailsOnMauiIOS]
		public void Issue1769Test()
		{
			RunningApp.WaitForElement(q => q.Marked(GoToPageTwoButtonText));
			RunningApp.Tap(q => q.Marked(GoToPageTwoButtonText));

			RunningApp.WaitForElement(q => q.Marked(SwitchAutomatedId));
			RunningApp.WaitForElement(q => q.Marked(string.Format(SwitchIsNowLabelTextFormat, false)));
			RunningApp.Tap(q => q.Marked(SwitchAutomatedId));
			RunningApp.WaitForElement(q => q.Marked(string.Format(SwitchIsNowLabelTextFormat, true)));
		}
#endif
	}
}