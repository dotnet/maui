using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using NUnit.Framework;
using Xamarin.UITest;
#endif


namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	public class TabbedPageWithListName
	{
		public string Name { get; set; }
	}

	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.None, 0, "TabbedPage with list", PlatformAffected.All)]
#if UITEST
	[Category(Compatibility.UITests.UITestCategories.TabbedPage)]
#endif
	public class TabbedPageWithList : TestTabbedPage
	{
		protected override void Init()
		{
			Title = "Tabbed Page with List";
			Children.Add(new ContentPage { Title = "Tab Two" });
			Children.Add(new ListViewTest());
		}

#if UITEST
		[Test]
		[Compatibility.UITests.FailsOnMauiIOS]
		public void TabbedPageWithListViewIssueTestsAllElementsPresent ()
		{
			RunningApp.WaitForElement (q => q.Marked ("Tab Two"));
			RunningApp.WaitForElement (q => q.Marked ("List Page"));
			RunningApp.Screenshot ("All elements present");
		}

		[Test]
		[Compatibility.UITests.FailsOnMauiIOS]
		public void TabbedPageWithListViewIssueTestsNavigateToAndVerifyListView ()
		{
			RunningApp.Tap (q => q.Marked ("List Page"));

			RunningApp.WaitForElement (q => q.Marked ("Jason"));
			RunningApp.WaitForElement (q => q.Marked ("Ermau"));
			RunningApp.WaitForElement (q => q.Marked ("Seth"));

			RunningApp.Screenshot ("ListView correct");
		}
#endif

		public class ListViewTest : ContentPage
		{
			public ListViewTest()
			{
				Title = "List Page";

				var items = new[] {
					new TabbedPageWithListName () { Name = "Jason" },
					new TabbedPageWithListName () { Name = "Ermau" },
					new TabbedPageWithListName () { Name = "Seth" }
				};

				var cellTemplate = new DataTemplate(typeof(TextCell));
				cellTemplate.SetBinding(TextCell.TextProperty, "Name");

				Content = new ListView()
				{
					ItemTemplate = cellTemplate,
					ItemsSource = items
				};
			}
		}
	}
}
