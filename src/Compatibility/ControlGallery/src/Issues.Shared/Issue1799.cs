using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github5000)]
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
		[Compatibility.UITests.FailsOnMauiIOS]
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