using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 34318, "Shell Navigating event should fire on ShellContent change", PlatformAffected.All)]
public class Issue34318 : Shell
{
	Label labelA;
	Label labelB;

	public Issue34318()
	{
		labelA = new Label
		{
			Text = "Waiting",
			AutomationId = "ResultLabel"
		};

		labelB = new Label
		{
			Text = "Waiting",
			AutomationId = "ResultLabel"
		};

		var section = new ShellSection();

		var pageA = new Issue34318_PageA(this, labelA);

		var contentA = new ShellContent
		{
			Content = pageA
		};

		var contentB = new ShellContent
		{
			Content = new ContentPage
			{
				Title = "PageB",
				Content = new VerticalStackLayout
				{
					Children =
					{
						new Label
						{
							Text = "Page B",
							AutomationId = "PageBLabel"
						},
						labelB
					}
				}
			}
		};

		section.Items.Add(contentA);
		section.Items.Add(contentB);

		var item = new ShellItem();
		item.Items.Add(section);

		Items.Add(item);

		Navigating += (_, __) =>
		{
			MainThread.BeginInvokeOnMainThread(() =>
			{
				labelA.Text = "Navigating";
				labelB.Text = "Navigating";
			});
		};
	}

	public class Issue34318_PageA : ContentPage
	{
		public Issue34318_PageA(Shell shell, Label label)
		{
			Title = "PageA";

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
				Children = { button, label }
			};
		}
	}
}