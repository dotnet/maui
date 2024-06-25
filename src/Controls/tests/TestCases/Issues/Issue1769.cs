using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Devices;

namespace Maui.Controls.Sample.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1769, "PushAsync with Switch produces NRE", PlatformAffected.Android)]
	public class Issue1769 : NavigationPage
	{
		public Issue1769() : base(new MainPage())
		{
		}

		public class MainPage : ContentPage
		{
			const string GoToPageTwoButtonText = "Go To Page 2";
			const string SwitchLabelText = "Switch";
			const string SwitchAutomatedId = nameof(SwitchAutomatedId);
			const string SwitchIsNowLabelTextFormat = "Switch is now {0}";

			public MainPage()
			{
				var button = new Button()
				{
					AutomationId = GoToPageTwoButtonText,
					Text = GoToPageTwoButtonText
				};
				button.Clicked += async (sender, args) =>
				{
					await ((Button)sender).Navigation.PushAsync(new SwitchDemoPage());
				};

				Content = button;
			}

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

#pragma warning disable CS0618 // Type or member is obsolete
					var switcher = new Switch
					{
						AutomationId = SwitchAutomatedId,
						HorizontalOptions = LayoutOptions.Center,
						VerticalOptions = LayoutOptions.CenterAndExpand
					};
#pragma warning restore CS0618 // Type or member is obsolete
					switcher.Toggled += switcher_Toggled;

#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable CS0612 // Type or member is obsolete
					_label = new Label
					{
						AutomationId = string.Format(SwitchIsNowLabelTextFormat, switcher.IsToggled),
						Text = string.Format(SwitchIsNowLabelTextFormat, switcher.IsToggled),
						FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Label)),
						HorizontalOptions = LayoutOptions.Center,
						VerticalOptions = LayoutOptions.CenterAndExpand
					};
#pragma warning restore CS0612 // Type or member is obsolete
#pragma warning restore CS0618 // Type or member is obsolete

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
		}
	}
}