using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Issues
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
	}
}