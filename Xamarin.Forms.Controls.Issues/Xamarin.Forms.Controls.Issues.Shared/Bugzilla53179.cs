using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 53179, 
		"PopAsync crashing after RemovePage when support packages are updated to 25.1.1", PlatformAffected.Android)]
	public class Bugzilla53179 : TestNavigationPage
	{
		class TestPage : ContentPage
		{
			Button nextBtn, rmBtn, popBtn;

			public TestPage(int index)
			{
				nextBtn = new Button { Text = "Next Page" };
				rmBtn = new Button { Text = "Remove previous pages" };
				popBtn = new Button { Text = "Back" };

				nextBtn.Clicked += async (sender, e) => await Navigation.PushAsync(new TestPage(index + 1));
				rmBtn.Clicked += (sender, e) =>
				{
					var stackSize = Navigation.NavigationStack.Count;
					Navigation.RemovePage(Navigation.NavigationStack[stackSize - 2]);

					stackSize = Navigation.NavigationStack.Count;
					Navigation.RemovePage(Navigation.NavigationStack[stackSize - 2]);

					popBtn.IsVisible = true;
					rmBtn.IsVisible = false;
				};
				popBtn.Clicked += async (sender, e) => await Navigation.PopAsync();

				switch (index)
				{
					case 4:
						nextBtn.IsVisible = false;
						popBtn.IsVisible = false;
						break;
					default:
						rmBtn.IsVisible = false;
						popBtn.IsVisible = false;
						break;
				}

				Content = new StackLayout
				{
					Children = {
					new Label { Text = $"This is page {index}"},
					nextBtn,
					rmBtn,
					popBtn
				}
				};
			}
		}


		protected override void Init()
		{
			PushAsync(new TestPage(1));
		}

#if UITEST
		[Test]
		public void Bugzilla53179Test()
		{
			RunningApp.WaitForElement(q => q.Marked("Next Page"));
			RunningApp.Tap(q => q.Marked("Next Page"));

			RunningApp.WaitForElement(q => q.Marked("Next Page"));
			RunningApp.Tap(q => q.Marked("Next Page"));

			RunningApp.WaitForElement(q => q.Marked("Next Page"));
			RunningApp.Tap(q => q.Marked("Next Page"));

			RunningApp.WaitForElement(q => q.Marked("Remove previous pages"));
			RunningApp.Tap(q => q.Marked("Remove previous pages"));

			RunningApp.WaitForElement(q => q.Marked("Back"));
			RunningApp.Tap(q => q.Marked("Back"));
		}
#endif
	}
}