using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 32221, "[iOS] ScrollView does not resize when children are removed from StackLayout at runtime", PlatformAffected.iOS)]

public class Issue32221 : ContentPage
{
	int labelCount = 3;
	StackLayout labelStack;

	public Issue32221()
	{
		// Create the label stack
		labelStack = new StackLayout
		{
			BackgroundColor = Colors.Beige,
			Spacing = 10
		};

		// Add initial labels
		for (int i = 1; i <= labelCount; i++)
		{
			labelStack.Children.Add(new Label
			{
				Text = $"Label {i}",
				FontSize = 18,
				Padding = new Thickness(10)
			});
		}

		// Create ScrollView to hold the label stack
		var scrollView = new ScrollView
		{
			Content = labelStack
		};

		// Create buttons
		var addButton = new Button
		{
			Text = "Add Label",
			AutomationId = "AddLabelButton"
		};
		addButton.Clicked += OnAddLabelClicked;

		var removeButton = new Button
		{
			Text = "Remove Label",
			AutomationId = "RemoveLabelButton"
		};
		removeButton.Clicked += OnRemoveLabelClicked;

		// Create the main layout
		var mainLayout = new VerticalStackLayout
		{
			Padding = 20,
			Children = { scrollView, addButton, removeButton }
		};

		// Set the page content
		Content = mainLayout;
	}

	void OnAddLabelClicked(object sender, EventArgs e)
	{
		labelCount++;
		labelStack.Children.Add(new Label
		{
			Text = $"Label {labelCount}",
			FontSize = 18,
			Padding = new Thickness(10)
		});
	}

	void OnRemoveLabelClicked(object sender, EventArgs e)
	{
		if (labelStack.Children.Count > 0)
		{
			labelStack.Children.RemoveAt(labelStack.Children.Count - 1);
			labelCount--;
		}
	}
}
