using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Issues
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
	}
}