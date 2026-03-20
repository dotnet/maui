
namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 34318, "Shell Navigating event should fire on ShellContent change", PlatformAffected.All)]
public class Issue34318 : Shell
{
	public Issue34318()
	{
		// When Shell.Navigating fires, broadcast a message the page test can observe
		Navigating += (s, e) => MessagingCenter.Send(this, "NavigatingFired");

		var section = new ShellSection();

		var contentA = new ShellContent
		{
			Content = new Issue34318_PageA()
		};

		var contentB = new ShellContent
		{
			Content = new ContentPage { Title = "PageB", Content = new Label { Text = "Page B", AutomationId = "PageBLabel" } }
		};

		section.Items.Add(contentA);
		section.Items.Add(contentB);

		var item = new ShellItem();
		item.Items.Add(section);

		Items.Add(item);
	}

	public class Issue34318_PageA : ContentPage
	{
		Label resultLabel;

		public Issue34318_PageA()
		{
			Title = "PageA";
			resultLabel = new Label { Text = "Waiting", AutomationId = "ResultLabel" };

			var button = new Button { Text = "Change Content", AutomationId = "ChangeContentButton" };
			button.Clicked += (s, e) =>
			{
				if (Parent is ShellSection section && section.Items.Count > 1)
				{
					section.CurrentItem = section.Items[1];
				}
			};

			// Update the label when the shell raises Navigating (message sent from Shell)
			MessagingCenter.Subscribe<Shell>(this, "NavigatingFired", (sender) =>
			{
				MainThread.BeginInvokeOnMainThread(() =>
				{
					resultLabel.Text = "Navigating";
				});
			});

			Content = new VerticalStackLayout
			{
				Children = { button, resultLabel }
			};
		}
	}
}
