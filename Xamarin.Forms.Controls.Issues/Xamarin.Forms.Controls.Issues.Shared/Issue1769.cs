using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers=true)]
	[Issue (IssueTracker.Github, 1769, "PushAsync with Switch produces NRE", PlatformAffected.Android)]
	public class Issue1769
		: ContentPage
	{
		public Issue1769()
		{
			var button =  new Button()
				{
					Text = "Go To Page 2"
				};

			var switchDemo = new SwitchDemoPage();

			button.Clicked += async (sender, args) => {
				await ((Button)sender).Navigation.PushAsync(switchDemo);
			};

			Content = button;
		}

		class SwitchDemoPage : ContentPage
		{
			Label _label;

			public SwitchDemoPage()
			{
				Label header = new Label
				{
					Text = "Switch",
#pragma warning disable 618
					Font = Font.BoldSystemFontOfSize(50),
#pragma warning restore 618
					HorizontalOptions = LayoutOptions.Center
				};

				Switch switcher = new Switch
				{
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.CenterAndExpand
				};
				switcher.Toggled += switcher_Toggled;

				_label = new Label
				{
					Text = "Switch is now False",
#pragma warning disable 618
					Font = Font.SystemFontOfSize(NamedSize.Large),
#pragma warning restore 618
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.CenterAndExpand
				};

				// Accomodate iPhone status bar.
				Padding = Device.RuntimePlatform == Device.iOS ? new Thickness(10, 20, 10, 5) : new Thickness(10, 0, 10, 5);

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
				_label.Text = string.Format("Switch is now {0}", e.Value);
			}
		}
	}
}
