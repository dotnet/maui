using System.Diagnostics;

using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using NUnit.Framework;
using Xamarin.UITest;
#endif

namespace Xamarin.Forms.Controls.Issues
{

	public class NavPageNameObject
	{
		public string PageName { get; private set; }

		public NavPageNameObject (string pageName)
		{
			PageName = pageName;
		}
	}

	[Preserve (AllMembers=true)]
	[Issue (IssueTracker.Github, 892, "NavigationPages as details in MasterDetailPage don't work as expected", PlatformAffected.Android)]
	public class Issue892 : TestMasterDetailPage
	{
		protected override void Init ()
		{
			var cells = new [] {
				new NavPageNameObject ("Close Master"),
				new NavPageNameObject ("Page 1"),
				new NavPageNameObject ("Page 3"),
				new NavPageNameObject ("Page 4"),
				new NavPageNameObject ("Page 5"),
				new NavPageNameObject ("Page 6"),
				new NavPageNameObject ("Page 7"),
				new NavPageNameObject ("Page 8"),
			};

			var template = new DataTemplate (typeof (TextCell));
			template.SetBinding (TextCell.TextProperty, "PageName");

			var listView = new ListView { 
				ItemTemplate = template,
				ItemsSource = cells
			};

			listView.BindingContext = cells;

			listView.ItemTapped += (sender, e) => {
				var cellName = ((NavPageNameObject)e.Item).PageName;
				if (cellName == "Close Master") {
					IsPresented = false;
				} else {
					Detail = new CustomNavDetailPage (cellName);
				}
			};

			var master = new ContentPage {
				Padding = new Thickness(0, 20, 0, 0),
				Title = "Master",
				Content = listView
			};
				
			Master = master;
			Detail = new CustomNavDetailPage ("Initial Page");

			MessagingCenter.Subscribe<NestedNavPageRootView> (this, "PresentMaster", (sender) => {
				IsPresented = true;
			});
		}

		// Issue892
		// NavigationPage nested in MasterDetail not working as expected Android

#if UITEST
		[Test]
		[Description ("Change pages in the Master ListView, and navigate to the end and back")]
		[UiTest (typeof(MasterDetailPage))]
		[UiTest (typeof(NavigationPage))]
		public void Issue892TestsNavigateChangePagesNavigate ()
		{
			NavigateToEndAndBack ();

			RunningApp.Tap (q => q.Marked ("Present Master"));
			
			RunningApp.Tap (q => q.Marked ("Page 5"));

			RunningApp.Tap (q => q.Marked ("Close Master"));

			RunningApp.Screenshot ("Select new detail navigation");

			NavigateToEndAndBack ();
		}

		void NavigateToEndAndBack ()
		{
			RunningApp.WaitForElement (q => q.Button ("Push next page")); // still required on iOS
			RunningApp.Tap (q => q.Marked ("Push next page"));
			RunningApp.Screenshot ("Pushed first page");

			RunningApp.WaitForElement (q => q.Button ("Push next next page")); // still required on iOS
			RunningApp.Tap (q => q.Marked ("Push next next page"));
			RunningApp.Screenshot ("Pushed second page");

			RunningApp.WaitForElement (q => q.Marked ("You are at the end of the line"));
			RunningApp.Screenshot ("Pushed last page");

			RunningApp.Tap (q => q.Marked ("Check back one"));
			RunningApp.WaitForElement (q => q.Marked ("Pop one"));
			RunningApp.Back ();
			RunningApp.Screenshot ("Navigate Back");

			RunningApp.Tap (q => q.Marked ("Check back two"));
			RunningApp.WaitForElement (q => q.Marked ("Pop two"));
			RunningApp.Back ();
			RunningApp.Screenshot ("Navigate Back");

			RunningApp.Tap (q => q.Marked ("Check back three"));
			RunningApp.WaitForElement (q => q.Marked ("At root"));
			RunningApp.Screenshot ("At root");
		}
#endif

	}

	public class CustomNavDetailPage : NavigationPage
	{
		public CustomNavDetailPage (string pageName)
		{
			PushAsync (new NestedNavPageRootView (pageName));
		}
	}

	public class NestedNavPageRootView : ContentPage
	{
		public NestedNavPageRootView (string pageTitle)
		{
			Title = pageTitle;
			BackgroundColor = Color.FromHex("#666");

			var label = new Label {
				Text = "Not Tapped"
			};

			Content = new StackLayout {
				Children = {
					label,
					new Button {
						Text = "Check back three",
						Command = new Command (() => { label.Text = "At root"; })
					},
					new Button {
						Text = "Push next page",
						Command = new Command (() => Navigation.PushAsync (new NestedNavPageOneLevel ()))
					},
					new Button {
						Text = "Present Master",
						Command = new Command (() => {
							MessagingCenter.Send<NestedNavPageRootView> (this, "PresentMaster");
						})
					}
				}
			};
		}
	}

	public class NestedNavPageOneLevel : ContentPage
	{
		public NestedNavPageOneLevel ()
		{
			Title = "One pushed";
			BackgroundColor = Color.FromHex("#999");

			var label = new Label {
				Text = "Not Tapped"
			};

			Content = new StackLayout {
				Children = {
					label,
					new Button {
						Text = "Check back two",
						Command = new Command (() => { label.Text = "Pop two"; })
					},
					new Button {
						Text = "Push next next page",
						Command = new Command (() => Navigation.PushAsync (new NestedNavPageTwoLevels ()))
					}
				}
			};
		}
	}

	public class NestedNavPageTwoLevels : ContentPage
	{
		public NestedNavPageTwoLevels ()
		{
			Title = "Two pushed";
			BackgroundColor = Color.FromHex("#BBB");

			var label = new Label {
				Text = "Not Tapped",
				TextColor = Color.Red
			};

			var label2 = new Label {
				Text = "You are at the end of the line",
				TextColor = Color.Red
			};

			Content = new StackLayout {
				Children = {
					label,
					label2,
					new Button {
						Text = "Check back one",
						Command = new Command (() => { label.Text = "Pop one"; })
					},
				}
			};
		}
	}
}
