using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 12642, "[iOS] Rapid ShellContent Navigation Causes Blank Screens",
		PlatformAffected.iOS)]
	public class Issue12642 : TestShell
	{
		protected override void Init()
		{
			var page = AddTopTab("Tab 1");
			var page2 = AddTopTab("Tab 2");

			Label successLabel = new Label()
			{
				Text = "Success",
				AutomationId = "Success",
				IsVisible = false
			};

			page.Content = CreateContent();
			page2.Content = CreateContent(successLabel);

			StackLayout CreateContent(Label label = null)
			{
				StackLayout layout = null;
				layout = new StackLayout()
				{
					Children =
					{
						new Label()
						{
							Text = "Click quickly between the tabs. If you stop clicking and the content is blank then the test has failed."
						},
						new Button()
						{
							Text = "Run Test Automated",
							AutomationId = "AutomatedRun",
							Command = new Command(async () =>
							{
								successLabel.IsVisible = false;
								for(int i = 0; i < 20; i++)
								{
									this.CurrentItem = Items[0].Items[0].Items[0];
									await Task.Delay(10);
									this.CurrentItem = Items[0].Items[0].Items[1];
									await Task.Delay(10);
								}
								successLabel.IsVisible = true;
							})
						}
					}
				};

				if (label != null)
					layout.Children.Add(label);

				return layout;
			}
		}
	}
}
