using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 45702, "Disabling back press on modal page causes app to crash", PlatformAffected.Android)]
	public class Bugzilla45702 : TestNavigationPage
	{
		protected override void Init()
		{
			Navigation.PushAsync(new NavigationPage(new MasterDetailPage() { Master = new ContentPage() { Title = "Master" }, Detail = new DetailPage() }));

			MessagingCenter.Subscribe<DetailPage>(this, "switch", SwitchControl);
		}

		void SwitchControl(object sender)
		{
			Application.Current.MainPage = new NavigationPage(new ContentPage { Content = new Label { Text = "Success" } });
		}

		class DetailPage : ContentPage
		{
			public DetailPage()
			{
				var button = new Button { Text = "Click me" };
				button.Clicked += Button_Clicked;
				Content = button;
			}

			void Button_Clicked(object sender, System.EventArgs e)
			{
				MessagingCenter.Send(this, "switch");
			}
		}
	}
}