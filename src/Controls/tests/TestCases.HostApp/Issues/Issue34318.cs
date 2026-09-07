using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 34318, "Shell Navigating event should fire on ShellContent change", PlatformAffected.All)]
public class Issue34318 : Shell
{
	Label labelA;
	Label labelB;
	Label navigatingCountLabel;
	int navigatingCount;

	public Issue34318()
	{
		labelA = new Label
		{
			Text = "Waiting",
			AutomationId = "ResultLabelA"
		};

		labelB = new Label
		{
			Text = "Waiting",
			AutomationId = "ResultLabelB"
		};

		navigatingCountLabel = new Label
		{
			Text = "0",
			AutomationId = "NavigatingCountLabel"
		};

		var section = new ShellSection();

		var pageA = new Issue34318_PageA(labelA);

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
						labelB,
						navigatingCountLabel
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
				navigatingCount++;

				labelA.Text = "Navigating";
				labelB.Text = "Navigating";
				navigatingCountLabel.Text = navigatingCount.ToString();
			});
		};
	}

	public class Issue34318_PageA : ContentPage
	{
		public Issue34318_PageA(Label label)
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
