using System;
using Xamarin.Forms.CustomAttributes;
using System.Linq;

using Xamarin.Forms.Internals;
#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.ListView)]
#endif

	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Bugzilla, 39668, "Overriding ListView.CreateDefault Does Not Work on Windows", PlatformAffected.WinRT)]
	public class Bugzilla39668 : TestContentPage
	{
		[Preserve (AllMembers = true)]
		public class CustomListView : ListView
		{
			protected override Cell CreateDefault (object item)
			{
				var cell = new ViewCell ();

				cell.View = new StackLayout {
					BackgroundColor = Color.Green,
					Children = {
						new Label { Text = "Success" }
					}
				};

				return cell;
			}
		}

		protected override void Init ()
		{
			CustomListView lv = new CustomListView () {
				ItemsSource = Enumerable.Range (0, 10)
			};
			Content = new StackLayout { Children = { new Label { Text = "If the ListView does not have green Cells, this test has failed." }, lv } };
		}

#if UITEST
		[Test]
		public void Bugzilla39668Test ()
		{
			RunningApp.WaitForElement (q => q.Marked ("Success"));
		}
#endif
	}
}
