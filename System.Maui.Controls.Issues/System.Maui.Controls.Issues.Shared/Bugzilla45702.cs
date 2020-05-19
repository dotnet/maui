using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.Forms.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.Navigation)]
#endif

	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 45702, "Disabling back press on modal page causes app to crash", PlatformAffected.Android)]
	public class Bugzilla45702 : TestNavigationPage
	{
		protected override void Init()
		{
			Navigation.PushAsync(new NavigationPage(new MasterDetailPage() { Master = new ContentPage() { Title = "Master" }, Detail = new DetailPage45702() }));

			MessagingCenter.Subscribe<DetailPage45702>(this, "switch", SwitchControl);
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
				var button = new Button { Text = "Click me" };
				button.Clicked += Button_Clicked;
				Content = button;
			}

			void Button_Clicked(object sender, System.EventArgs e)
			{
				MessagingCenter.Send(this, "switch");
			}
		}

#if UITEST
		[Test]
		public void Issue45702Test() 
		{
			RunningApp.WaitForElement (q => q.Marked ("Click me"));
			RunningApp.Tap (q => q.Marked ("Click me"));
			RunningApp.WaitForElement (q => q.Marked ("Success"));
		}
#endif
	}
}