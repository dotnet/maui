using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[NUnit.Framework.Category(Core.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 51825, "[iOS] Korean input in SearchBar doesn't work", PlatformAffected.iOS)]
	public class Bugzilla51815 : TestContentPage
	{
		protected override void Init()
		{
			var sb = new SearchBar { AutomationId = "Bugzilla51825SearchBar" };
			var text = new Label { AutomationId = "Bugzilla51825Label" };
			sb.TextChanged += (sender, e) =>
			{
				text.Text = sb.Text;
			};

			Content = new StackLayout
			{
				Children =
				{
					sb,
					new Button
					{
						AutomationId = "Bugzilla51825Button",
						Text = "Change SearchBar text",
						Command = new Command(() =>
						{
							sb.Text = "Test";
						})
					},
					text,
					new Label
					{
						Text = "The label above should match the text in the SearchBar; " +
							"additionally, typing Korean characters should properly combine them."
					}
				}
			};
		}

#if UITEST
		[Test]
		public void Bugzilla51825Test()
		{
			RunningApp.WaitForElement(q => q.Marked("Bugzilla51825SearchBar"));
			RunningApp.EnterText("Bugzilla51825SearchBar", "Hello");
			var label = RunningApp.WaitForFirstElement("Bugzilla51825Label");

			Assert.IsNotEmpty(label.ReadText());
			Assert.AreEqual("Hello", label.ReadText());

			RunningApp.Tap("Bugzilla51825Button");

			var labelChange2 = RunningApp.WaitForFirstElement("Bugzilla51825Label");
			Assert.AreEqual("Test", labelChange2.ReadText());
		}
#endif
	}
}