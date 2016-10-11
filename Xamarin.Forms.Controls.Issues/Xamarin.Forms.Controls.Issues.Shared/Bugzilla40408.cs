using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using System;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 40408, "MasterDetailPage and TabbedPage only firing Appearing once", PlatformAffected.WinRT)]
	public class Bugzilla40408 : TestNavigationPage
	{
		protected override void Init()
		{
			BarBackgroundColor = Color.Red;
			
			var contentPage2 = new ContentPage();
			contentPage2.Title = "Page 2";
			contentPage2.BackgroundColor = Color.Green;

			contentPage2.Appearing += ContentPage2_Appearing;
			contentPage2.Disappearing += ContentPage2_Disappearing;
			
			var tabbedPage1 = new TabbedPage();
			tabbedPage1.Appearing += TabbedPage1_Appearing;
			tabbedPage1.Disappearing += TabbedPage1_Disappearing;

			var contentPage3 = new ContentPage() { Title = "Page 3" };
			contentPage3.BackgroundColor = Color.Pink;
			tabbedPage1.Children.Add(contentPage3);
			
			var masterDetailPage1 = new MasterDetailPage();
			masterDetailPage1.Title = "Page 3";
			var master1 = new ContentPage();
			master1.BackgroundColor = Color.Yellow;
			master1.Title = "Master 1";
			var detail1 = new ContentPage();
			detail1.Title = "Detail 1";
			detail1.BackgroundColor = Color.Purple;
			masterDetailPage1.Master = master1;
			masterDetailPage1.Detail = detail1;
			masterDetailPage1.BackgroundColor = Color.Yellow;

			masterDetailPage1.Appearing += MasterDetailPage1_Appearing;
			masterDetailPage1.Disappearing += MasterDetailPage1_Disappearing;
			
			var contentPage1 = new ContentPage();
			SetHasBackButton(contentPage1, true);
			contentPage1.BackgroundColor = Color.Blue;
			contentPage1.Title = "Page 1";
			var stack = new StackLayout();
			contentPage1.Content = stack;

			stack.Children.Add(new Button() { Text = "View Content", Command = new Command(() => PushAsync(contentPage2)) });
			stack.Children.Add(new Button() { Text = "View Master Detail", Command = new Command(() => PushAsync(masterDetailPage1)) });
			stack.Children.Add(new Button() { Text = "View Tabbed Page", Command = new Command(() => PushAsync(tabbedPage1)) });
			
			PushAsync(contentPage1);
		}

		void ContentPage2_Disappearing(object sender, EventArgs e)
		{
			DisplayAlert("Disappearing", "ContentPage", "OK");
		}

		void TabbedPage1_Disappearing(object sender, EventArgs e)
		{
			DisplayAlert("Disappearing", "TabbedPage", "OK");
		}

		void MasterDetailPage1_Disappearing(object sender, EventArgs e)
		{
			DisplayAlert("Disappearing", "MasterDetailPage", "OK");
		}

		void TabbedPage1_Appearing(object sender, EventArgs e)
		{
			DisplayAlert("Appearing", "TabbedPage", "OK");
		}

		void ContentPage2_Appearing(object sender, EventArgs e)
		{
			DisplayAlert("Appearing", "ContentPage", "OK");
		}

		void MasterDetailPage1_Appearing(object sender, EventArgs e)
		{
			DisplayAlert("Appearing", "MasterDetailPage", "OK");
		}
	}
}