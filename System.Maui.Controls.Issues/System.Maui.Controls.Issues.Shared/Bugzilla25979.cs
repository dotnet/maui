using System;

using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Bugzilla, 25979, "https://bugzilla.xamarin.com/show_bug.cgi?id=25979")]
	public class Bugzilla25979 : TestNavigationPage // or TestMasterDetailPage, etc ...
	{
		[Preserve (AllMembers = true)]
		internal sealed class MyPage : ContentPage
		{
			public MyPage()
			{
				Title = "Page 1";
				AutomationId = "PageOneId";

				var b = new Button {
					AutomationId = "PageOneButtonId",
					Text = "Next"
				};
				b.Clicked += async (sender, e) => {
					await Navigation.PushAsync (new MyPage2());
				};
				
				Content = new StackLayout { 
					BackgroundColor = Color.Red,
					Children = {
						new Label { Text = "Page 1", FontSize=Device.GetNamedSize(NamedSize.Large, typeof(Label)) },
						b
					}
				};
			}
		}

		[Preserve (AllMembers = true)]
		internal sealed class MyPage2 : ContentPage
		{
			public MyPage2()
			{
				Title = "Page 2";
				AutomationId = "PageTwoId";

				var b = new Button {
					AutomationId = "PageTwoButtonId",
					Text = "Next"
				};
				b.Clicked += async (sender, e) => {
					await Navigation.PushAsync (new MyPage3());
					Navigation.NavigationStack[0].BindingContext = null;
					Navigation.RemovePage(Navigation.NavigationStack[0]);
				};
				
				Content = new StackLayout { 
					BackgroundColor = Color.Red,
					Children = {
						new Label { Text = "Page 2", FontSize=Device.GetNamedSize(NamedSize.Large, typeof(Label)) }, 
						b
					}
				};
			}

			protected override void OnAppearing ()
			{
				base.OnAppearing();
				Navigation.NavigationStack[0].BindingContext = null;
				Navigation.RemovePage(Navigation.NavigationStack[0]);
			}
		}

		[Preserve (AllMembers = true)]
		internal sealed class MyPage3 : ContentPage
		{
			public MyPage3 ()
			{
				AutomationId = "PageThreeId";
				Title = "PageThreeId";

				var label = new Label { Text = "Page 3" };

				Content = new StackLayout {
					Children = {
						label,
						new Button { 
							AutomationId = "PopButton",
							Text = "Try to Pop",
							Command = new Command(
								async() => {
									await Navigation.PopAsync();
									label.Text = "PopAttempted";
								}
							)}
						}
				};
			}
		}

		protected override void Init ()
		{
			// Initialize ui here instead of ctor
			Navigation.PushAsync (new MyPage () { Title="Navigation Stack" });
		}

#if UITEST
		[Test]
		public void Bugzilla25979Test ()
		{
			RunningApp.WaitForElement (q => q.Marked ("PageOneId"));
			RunningApp.Screenshot ("At page one");
			RunningApp.WaitForElement (q => q.Marked ("PageOneButtonId"));
			RunningApp.Tap (q => q.Marked ("PageOneButtonId"));
#if __MACOS__
			System.Threading.Thread.Sleep(2000);
#endif

			RunningApp.WaitForElement (q => q.Marked ("PageTwoId"));
			RunningApp.Screenshot ("At page two - I didn't crash");
			RunningApp.WaitForElement (q => q.Marked ("PageTwoButtonId"));
			RunningApp.Tap (q => q.Marked ("PageTwoButtonId"));
#if __MACOS__
			System.Threading.Thread.Sleep(2000);
#endif

			RunningApp.WaitForElement (q => q.Marked ("PageThreeId"));
			RunningApp.Screenshot ("At page three - I didn't crash");

			RunningApp.WaitForElement (q => q.Marked ("PopButton"));
			RunningApp.Tap (q => q.Marked ("PopButton"));
			RunningApp.WaitForElement (q => q.Marked ("PopAttempted"));
		}
#endif
	}
}
