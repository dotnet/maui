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
	[Category(UITestCategories.LifeCycle)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 4303, "[Android] TabbedPage's child is appearing before it should be", PlatformAffected.Android)]
	public class Issue4303 : TestTabbedPage // or TestMasterDetailPage, etc ...
	{
		bool appeared = false;
		bool childAppeared = false;
		bool navigatedToChhild = false;
		const string Success = "Success";
		const string ChildSuccess = "ChildSuccess";
		const string Fail = "Fail";
		const string lblAutomationID = "lblAssert";
		const string lblChildAutomationID = "lblChildAssert";
		const string btnAutomationID = "btnGo";
		Label lbl;
		Label childLbl;
		TabbedPage childTab;
		protected override void Init()
		{
			childTab = new TabbedPage { Title = "Tab4" };
			childTab.Appearing += ChildTabAppearing;

			lbl = new Label { Text = "Default", AutomationId = lblAutomationID };
			childLbl = new Label { Text = "Default", AutomationId = lblChildAutomationID };
			var btn = new Button { Text = "Go to Tab4", AutomationId = btnAutomationID, Command = new Command(() => CurrentPage = childTab) };
			var page1 = new ContentPage { Title = "Page 1", Content = new StackLayout { Children = { lbl, btn } } };
			var page2 = new ContentPage { Title = "Page 2" };
			var page3 = new ContentPage { Title = "Page 3" };

			childTab.Children.Add(new ContentPage { Title = "Tab Child Page 1", Content = childLbl });

			Children.Add(page1);
			Children.Add(page2);
			Children.Add(page3);

			Children.Add(childTab);
		}

		protected override void OnCurrentPageChanged()
		{
			base.OnCurrentPageChanged();
			navigatedToChhild = (CurrentPage == childTab);
			UpdateLabel();
		}

		void ChildTabAppearing(object sender, System.EventArgs e)
		{
			childAppeared = true;
			UpdateLabel();
		}

		void UpdateLabel()
		{
			if(appeared && !navigatedToChhild && !childAppeared)
			{
				lbl.Text = Success;
			}
			else if (appeared && navigatedToChhild && childAppeared)
			{
				childLbl.Text = ChildSuccess;
			}
			else
			{
				childLbl.Text = lbl.Text = Fail;
			}
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			appeared = true;
			UpdateLabel();

		}

#if UITEST
		[Test]
		public void Issue4303Test() 
		{
			RunningApp.WaitForElement ( c => c.Text(Success));
			RunningApp.WaitForElement(c => c.Marked(btnAutomationID));
			RunningApp.Tap(c => c.Marked(btnAutomationID));
			RunningApp.WaitForElement(c => c.Text(ChildSuccess));
		}
#endif
	}
}