using System;

using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 41038, "MasterDetailPage loses menu icon on iOS after reusing NavigationPage as Detail")]
	public class Bugzilla41038 : TestMasterDetailPage // or TestMasterDetailPage, etc ...
	{
		NavigationPage _navPage;

		protected override void Init()
		{
			Title = "Main";

			var btnViewA = new Button() { Text = "ViewA" };
			btnViewA.Clicked += Button_Clicked;

			var btnViewB = new Button() { Text = "ViewB" };
			btnViewB.Clicked += Button_Clicked;

			var stack = new StackLayout();
			stack.Children.Add(btnViewA);
			stack.Children.Add(btnViewB);

			var master = new ContentPage() { Title = "Master", Content = stack };

			_navPage = new NavigationPage(new ViewA());

			Master = master;
			Detail = _navPage;
	
		}

		private async void Button_Clicked(object sender, EventArgs e)
		{
			Page root = _navPage.Navigation.NavigationStack[0];

			await _navPage.Navigation.PopToRootAsync(false);

			Page newRoot = null;

			var btn = (Button)sender;
			if (btn.Text == "ViewA")
				newRoot = new ViewA();
			else
				newRoot = new ViewB();

				
			await _navPage.Navigation.PushAsync(newRoot);
			_navPage.Navigation.RemovePage(root);
			IsPresented = false;

		}

		public class ViewA : ContentPage
		{
			public ViewA()
			{
				Title = "ViewA";
				Content = new Label() { Text = "View A" };
			}
		}

		public class ViewB : ContentPage
		{
			public ViewB()
			{
				Title = "ViewB";
				Content = new Label() { Text = "View B" };
			}
		}

		#if UITEST &&  __IOS__
		[Test]
		public void Bugzilla41038Test()
		{
			RunningApp.WaitForElement("Master");
			RunningApp.Tap("Master");
			RunningApp.WaitForElement("ViewB");
			RunningApp.Tap("ViewB");
			RunningApp.WaitForElement("Master");
			RunningApp.Screenshot("I see the master toggle");
		}
		#endif
	}
}
