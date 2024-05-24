using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 45702, "Disabling back press on modal page causes app to crash", PlatformAffected.Android)]
	public class Bugzilla45702 : NavigationPage
	{
		public Bugzilla45702() : base(new MainPage())
		{
		}

		public class MainPage : ContentPage
		{
			public MainPage()
			{
				Navigation.PushAsync(new NavigationPage(new FlyoutPage() { Flyout = new ContentPage() { Title = "Flyout" }, Detail = new DetailPage45702() }));

#pragma warning disable CS0618 // Type or member is obsolete
				MessagingCenter.Subscribe<DetailPage45702>(this, "switch", SwitchControl);
#pragma warning restore CS0618 // Type or member is obsolete
			}

			void SwitchControl(object sender)
			{
				Application.Current.MainPage = new NavigationPage(new ContentPage { Content = new Label { Text = "Success" } });
			}

			[Preserve(AllMembers = true)]
			class DetailPage45702 : ContentPage
			{
				public DetailPage45702()
				{
					var button = new Button { AutomationId = "ClickMe", Text = "Click me" };
					button.Clicked += Button_Clicked;
					Content = button;
				}

				void Button_Clicked(object sender, System.EventArgs e)
				{
#pragma warning disable CS0618 // Type or member is obsolete
					MessagingCenter.Send(this, "switch");
#pragma warning restore CS0618 // Type or member is obsolete
				}
			}
		}
	}
}