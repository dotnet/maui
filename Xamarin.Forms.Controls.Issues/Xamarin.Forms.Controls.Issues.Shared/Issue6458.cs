using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.Forms.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 6458, "[Android] Fix load TitleIcon on non app compact", PlatformAffected.Android)]
	public class Issue6458 : TestContentPage // or TestFlyoutPage, etc ...
	{
		protected override void Init()
		{
			NavigationPage.SetTitleIconImageSource(this,
				new FileImageSource
				{
					File = "bank.png",
					AutomationId = "banktitleicon"
				});
			Content = new Label
			{
				AutomationId = "IssuePageLabel",
				Text = "Make sure you run this on Non AppCompact Activity"
			};
		}

#if UITEST && __ANDROID__
		[Test]
		public void Issue6458Test()
		{
			RunningApp.WaitForElement("IssuePageLabel");
			var element = RunningApp.WaitForElement("banktitleicon");

			Assert.AreEqual(1, element.Length, "banktitleicon not found");

			Assert.Greater(element[0].Rect.Height, 10);
			Assert.Greater(element[0].Rect.Width, 10);
		}
#endif
	}
}