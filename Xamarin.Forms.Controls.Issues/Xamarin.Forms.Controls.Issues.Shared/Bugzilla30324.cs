using System;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(Core.UITests.UITestCategories.MasterDetailPage)]
#endif
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Bugzilla, 30324, "Detail view of MasterDetailPage does not get appearance events on Android when whole MasterDetailPage disappears/reappears")]
	public class Bugzilla30324 : TestNavigationPage
	{
		Label _lbl;
		int _count;

		protected override void Init ()
		{
			MasterDetailPage page = new MasterDetailPage();
			page.Master = new Page () { Title = "Master", BackgroundColor = Color.Red };
			_lbl = new Label ();	

			var otherPage = new ContentPage () {
				Title = "Other",
				Content = new StackLayout { Children = {
						new Button () {
							Text = "navigate back",
							HorizontalOptions = LayoutOptions.Center,
							VerticalOptions = LayoutOptions.Center,

							Command = new Command (() => Navigation.PopAsync()) 
						}
					}
				}
			};

			page.Detail = new ContentPage () {
				Title = "Detail",
				Content = new StackLayout { Children = {
						_lbl,
						new Button () {
							Text = "navigate",
							HorizontalOptions = LayoutOptions.Center,
							VerticalOptions = LayoutOptions.Center,

							Command = new Command (() => Navigation.PushAsync (otherPage)) 
						}
					}
				}
			};

			page.Appearing += (sender, e) => 
			{
				System.Diagnostics.Debug.WriteLine("Appear MDP");
			};
			page.Disappearing += (sender, e) => 
			{
				System.Diagnostics.Debug.WriteLine("Disappear MDP");
			};
			page.Detail.Appearing += (sender, args) =>
			{
				if(_count ==2)
					_lbl.Text = "Appear detail";
				System.Diagnostics.Debug.WriteLine("Appear detail");
			};
			page.Detail.Disappearing += (sender, args) => {
				System.Diagnostics.Debug.WriteLine ("Disappear detail");
				_lbl.Text = "Disappear detail";
				page.Detail.BackgroundColor = Color.Green;
				_count++;
			};
			page.Master.Appearing += (sender, e) => 
			{
				System.Diagnostics.Debug.WriteLine("Appear master");
			};
			Navigation.PushAsync (page);
		}

		#if UITEST
		[Test]
		public void Bugzilla30324Test ()
		{
			RunningApp.Tap (q => q.Marked ("navigate"));
			RunningApp.Tap (q => q.Marked ("navigate back"));
			RunningApp.WaitForElement (q => q.Marked ("Disappear detail"));
			RunningApp.Tap (q => q.Marked ("navigate"));
			RunningApp.Tap (q => q.Marked ("navigate back"));
			RunningApp.WaitForElement (q => q.Marked ("Appear detail"));
		}
		#endif
	}


}


