using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.Navigation)]
	[NUnit.Framework.Category(Core.UITests.UITestCategories.Bugzilla)]
#endif

	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 53179,
		"PopAsync crashing after RemovePage when support packages are updated to 25.1.1",
		PlatformAffected.Android, issueTestNumber: 1)]
	public class Bugzilla53179_1 : TestNavigationPage
	{
		ContentPage _intermediate1;
		ContentPage _intermediate2;

		protected override async void Init()
		{
			_intermediate1 = Intermediate();
			_intermediate2 = Intermediate();

			await PushAsync(Root());
			await PushAsync(_intermediate1);
			await PushAsync(_intermediate2);
			await PushAsync(Last());
		}

		const string StartTest = "Start Test";
		const string RootLabel = "Root";

		ContentPage Last()
		{
			var test = new Button { Text = StartTest };

			var instructions = new Label
			{
				Text =
					$"Tap the button labeled '{StartTest}'. The app should navigate to a page displaying the label "
					+ $"'{RootLabel}'. If the application crashes, the test has failed."
			};

			var layout = new StackLayout();

			layout.Children.Add(instructions);
			layout.Children.Add(test);

			test.Clicked += (sender, args) =>
			{
				Navigation.RemovePage(_intermediate2);
				Navigation.RemovePage(_intermediate1);

				Navigation.PopAsync(true);
			};

			return new ContentPage { Content = layout };
		}

		static ContentPage Root()
		{
			return new ContentPage { Content = new Label { Text = RootLabel } };
		}

		static ContentPage Intermediate()
		{
			return new ContentPage { Content = new Label { Text = "Page" } };
		}

#if UITEST
		[Test]
		public void PopAsyncAfterRemovePageDoesNotCrash()
		{
			RunningApp.WaitForElement(StartTest);
			RunningApp.Tap(StartTest);
			RunningApp.WaitForElement(RootLabel);
		}
#endif
	}
}