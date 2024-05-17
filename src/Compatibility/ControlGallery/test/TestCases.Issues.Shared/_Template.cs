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
	[Category(UITestCategories.ManualReview)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1, "Issue Description", PlatformAffected.Default)]
	public class Issue1 : TestContentPage // or TestFlyoutPage, etc ...
	{
		protected override void Init()
		{
			// Initialize ui here instead of ctor
			Content = new Label
			{
				AutomationId = "Issue1Label",
				Text = "See if I'm here"
			};

			BindingContext = new ViewModelIssue1();
		}

#if UITEST
		[Test]
		[Compatibility.UITests.FailsOnMauiIOS]
		public void Issue1Test()
		{
			RunningApp.WaitForElement("Issue1Label");
			// Delete this and all other UITEST sections if there is no way to automate the test. Otherwise, be sure to rename the test and update the Category attribute on the class. Note that you can add multiple categories.
			RunningApp.Screenshot("I am at Issue1");
			RunningApp.WaitForElement(q => q.Marked("Issue1Label"));
			RunningApp.Screenshot("I see the Label");
		}
#endif
	}

	[Preserve(AllMembers = true)]
	public class ViewModelIssue1
	{
		public ViewModelIssue1()
		{

		}
	}

	[Preserve(AllMembers = true)]
	public class ModelIssue1
	{
		public ModelIssue1()
		{

		}
	}
}