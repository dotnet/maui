using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1648, "MasterDetailPage throws ArgumentOutOfRangeException", PlatformAffected.UWP)]
	public class GitHub1648 : TestNavigationPage
	{
		protected override void Init()
		{
			Navigation.PushAsync(new MasterDetailPage
			{
				Master = new NavigationPage(new ContentPage())
				{
					Title = "Master"
				},
				Detail = new ContentPage(),
			});
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			Navigation.PushModalAsync(new SimplePage());
		}

		class SimplePage : ContentPage
		{
			public SimplePage ()
			{
				Content = new StackLayout()
				{
					Children = {
						new Label {
							Text = "Success"
						},
						new Button
						{
							Text = "Reload",
							Command = new Command(() => Navigation.PopModalAsync())
						}
					}
				};
			}
		}

#if UITEST
		[Test]
		public void GitHub1648Test()
		{
			RunningApp.WaitForElement("Reload");
			RunningApp.Tap("Reload");
			RunningApp.WaitForElement("Success");
		}
#endif
	}
}