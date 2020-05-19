using System.Maui.CustomAttributes;
using System.Maui.Internals;
using System.Maui.Xaml;

#if UITEST
using System.Maui.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace System.Maui.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.ManualReview)]
#endif
#if APP
	[XamlCompilation(XamlCompilationOptions.Compile)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 2, "Issue Description (Markup)", PlatformAffected.Default)]
	public partial class _TemplateMarkup : TestContentPage
	{
		public _TemplateMarkup()
		{
#if APP
			InitializeComponent();
#endif
		}

		protected override void Init()
		{
			BindingContext = new ViewModelIssue2();
		}

#if UITEST
		[Test]
		public void Issue2Test()
		{
			// Delete this and all other UITEST sections if there is no way to automate the test. Otherwise, be sure to rename the test and update the Category attribute on the class. Note that you can add multiple categories.
			RunningApp.Screenshot("I am at Issue2");
			RunningApp.WaitForElement(q => q.Marked("Issue2Label"));
			RunningApp.Screenshot("I see the Label");
		}
#endif
	}

	[Preserve(AllMembers = true)]
	public class ViewModelIssue2
	{
		public ViewModelIssue2()
		{

		}
	}

	[Preserve(AllMembers = true)]
	public class ModelIssue2
	{
		public ModelIssue2()
		{

		}
	}
}