using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 34318, "Shell Navigating event should fire on ShellContent change", PlatformAffected.All)]
public class Issue34318 : Shell
{
	public Issue34318()
	{
		var section = new ShellSection();

		var pageA = new Issue34318_PageA(this);

		var contentA = new ShellContent
		{
			Content = pageA
		};

		var contentB = new ShellContent
		{
			Content = new ContentPage
			{
				Title = "PageB",
				Content = new Label
				{
					Text = "Page B",
					AutomationId = "PageBLabel"
				}
			}
		};

		section.Items.Add(contentA);
		section.Items.Add(contentB);

		var item = new ShellItem();
		item.Items.Add(section);

		Items.Add(item);

		// 🔥 Evento directo, sin MessagingCenter
		Navigating += (_, __) =>
		{
			pageA.SetNavigatingFired();
		};
	}

	public class Issue34318_PageA : ContentPage
	{
		Label resultLabel;

		public Issue34318_PageA(Shell shell)
		{
			Title = "PageA";

			resultLabel = new Label
			{
				Text = "Waiting",
				AutomationId = "ResultLabel"
			};

			var button = new Button
			{
				Text = "Change Content",
				AutomationId = "ChangeContentButton"
			};

			button.Clicked += (s, e) =>
			{
				Element parent = this;

				while (parent != null && parent is not ShellSection)
					parent = parent.Parent;

				if (parent is ShellSection section && section.Items.Count > 1)
				{
					section.CurrentItem = section.Items[1];
				}
			};

			Content = new VerticalStackLayout
			{
				Children = { button, resultLabel }
			};
		}

		public void SetNavigatingFired()
		{
			MainThread.BeginInvokeOnMainThread(() =>
			{
				resultLabel.Text = "Navigating";
			});
		}
	}
}