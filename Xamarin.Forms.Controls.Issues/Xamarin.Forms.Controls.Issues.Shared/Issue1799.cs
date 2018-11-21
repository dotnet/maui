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
	[Category(UITestCategories.ListView)]
	[NUnit.Framework.Category(UITestCategories.UwpIgnore)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1799, "[iOS] listView without data crash on ipad.", PlatformAffected.iOS)]
	public class Issue1799 : TestContentPage
	{
		const string ListView = "ListView1799";
		const string Success = "Success";

		protected override void Init()
		{
			var layout = new StackLayout();

			var listView = new ListView { IsRefreshing = true, AutomationId = ListView, IsPullToRefreshEnabled = false };

			var instructions = new Label { Text = "Pull the the ListView down and release. If the application crashes, the test has failed." };

			var success = new Label { Text = Success };

			layout.Children.Add(instructions);
			layout.Children.Add(success);
			layout.Children.Add(listView);

			Content = layout;
		}

#if UITEST
		[Test]
		public void ListViewWithoutDataDoesNotCrash()
		{
			var result = RunningApp.WaitForElement(ListView);
			var listViewRect = result[0].Rect;

			RunningApp.DragCoordinates(listViewRect.CenterX, listViewRect.Y, listViewRect.CenterX, listViewRect.Y + 50);

			RunningApp.WaitForElement(Success);
		}
#endif
	}
}