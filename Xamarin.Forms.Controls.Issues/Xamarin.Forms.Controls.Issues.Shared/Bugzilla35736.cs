using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using Xamarin.Forms.Core.UITests;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 35736, "[iOS] Editor does not update Text value from autocorrect when losing focus", PlatformAffected.iOS)]
	public class Bugzilla35736 : TestContentPage
	{
		protected override void Init()
		{
			var editor = new Editor
			{
				AutomationId = "Bugzilla35736Editor"
			};
			var label = new Label
			{
				AutomationId = "Bugzilla35736Label",
				Text = ""
			};

			Content = new StackLayout
			{
				Children =
				{
					editor,
					label,
					new Button
					{
						AutomationId = "Bugzilla35736Button",
						Text = "Click to set label text",
						Command = new Command(() => { label.Text = editor.Text; })
					}
				}
			};
		}


#if UITEST && __IOS__
		[Test]
		[Category(UITestCategories.ManualReview)]
		[Ignore]
		public void Bugzilla35736Test() 
		{
			RunningApp.WaitForElement(q => q.Marked("Bugzilla35736Editor"));
			RunningApp.EnterText(q => q.Marked("Bugzilla35736Editor"), "Testig");
			RunningApp.Tap(q => q.Marked("Bugzilla35736Button"));
			Assert.AreEqual("Testing", RunningApp.Query(q => q.Marked("Bugzilla35736Label"))[0].Text);
		}
#endif
	}
}