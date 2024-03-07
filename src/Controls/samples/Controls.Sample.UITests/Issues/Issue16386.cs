using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 16386, "Process the hardware enter key as \"Done\"", PlatformAffected.Android)]
	public class Issue16386 : TestContentPage
	{
		protected override void Init()
		{
			var entry = new Entry()
			{
				AutomationId = "HardwareEnterKeyEntry"
			};

			Content =
				new VerticalStackLayout()
				{
					new Label()
					{
						Text = "Focus entry and hit the Enter key on the hardware keyboard. A success label should appear."
					},
					entry
				};

			entry.Completed += (sender, args) =>
			{
				(Content as VerticalStackLayout)
					.Children.Add(new Label() { Text = "Success", AutomationId = "Success" });
			};
		}
	}
}
