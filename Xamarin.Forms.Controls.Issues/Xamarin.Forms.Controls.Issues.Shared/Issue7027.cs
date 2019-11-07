using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

//#if UITEST
//using Xamarin.Forms.Core.UITests;
//using Xamarin.UITest;
//using NUnit.Framework;
//#endif

namespace Xamarin.Forms.Controls.Issues
{
//#if UITEST
//	[Category(UITestCategories.TableView)]
//#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 7027, "[Bug] Tabbing through entries crashes when inside tableview",
		PlatformAffected.iOS)]
	public class Issue7027 : TestContentPage
	{
		const string Entry1 = "Entry1";
		const string Entry2 = "Entry2";
		const string Entry3 = "Entry3";
		const string Entry4 = "Entry4";

		public Issue7027()
		{
			Title = "Issue 7027";
		}

		protected override void Init()
		{
			TableView tableView = new TableView();

			var section1 = new TableSection("Section One")
			{
				new ViewCell { View = new Entry { Placeholder = "Entry 1", AutomationId = Entry1 } },
				new ViewCell { View = new Entry { Placeholder = "Entry 2", AutomationId = Entry2 } }
			};

			var section2Stack = new StackLayout() { Orientation = StackOrientation.Horizontal };
			section2Stack.Children.Add(new Entry { Placeholder = "Entry 3", AutomationId = Entry3 });
			section2Stack.Children.Add(new Entry { Placeholder = "Entry 4", AutomationId = Entry4 });

			var section2 = new TableSection("Section Two")
			{
				new ViewCell { View = section2Stack }
			};

			var section3 = new TableSection("Section Three")
			{
				new ViewCell { View = new Entry { Placeholder = "Entry 5" } },
				new ViewCell { View = new Entry { Placeholder = "Entry 6" } }
			};

			tableView.Root.Add(section1);
			tableView.Root.Add(section2);
			tableView.Root.Add(section3);

			var layout = new StackLayout { Children = { tableView } };

			Content = layout;
		}

//#if UITEST
//		[Test]
//		public void TabInTableViewWillNotCrash()
//		{
//			RunningApp.WaitForElement(Entry1);
//			RunningApp.Tap(Entry1);
//			RunningApp.EnterText("First entry");
//			RunningApp.DismissKeyboard();
//			RunningApp.PressEnter();
//		}
//#endif
	}
}
