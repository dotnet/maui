using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 5030, "App crash after clicking the done/next button when on landscape", PlatformAffected.Android)]
	public class Issue5030 : TestContentPage
	{
		protected override void Init()
		{
			Content = new StackLayout
			{
				Children = {
					new Label { Text = "Rotate device to landscape. Select editor, and click done/next button on virtual keyboard" },
					new Editor()
				}
			};
		}
	}
}