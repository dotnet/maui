using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 59097, "[Android] Calling PopAsync via TapGestureRecognizer causes an application crash", PlatformAffected.Android)]
	public class Bugzilla59097 : TestNavigationPage // or TestMasterDetailPage, etc ...
	{
		protected override void Init()
		{
			Navigation.PushAsync(new ContentPage { Content = new Label { Text = "previous page " } });
			Navigation.PushAsync(new ToPopPage());
		}

		public class ToPopPage : ContentPage
		{
			public ToPopPage()
			{
				var boxView = new BoxView { WidthRequest = 100, HeightRequest = 100, Color = Color.Red, AutomationId = "boxView" };
				var tapGesture = new TapGestureRecognizer { NumberOfTapsRequired = 1, Command = new Command(PopPageBack) };
				boxView.GestureRecognizers.Add(tapGesture);
				var layout = new StackLayout();
				layout.Children.Add(boxView);
				Content = layout;
			}

			async void PopPageBack(object obj)
			{
				await Navigation.PopAsync(true);
			}
		}


#if UITEST
		[Test]
		public void Bugzilla59097Test()
		{
			RunningApp.WaitForElement(q => q.Marked("boxView"));
			RunningApp.Tap(q => q.Marked("boxView"));
		}
#endif
	}
}