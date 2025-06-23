using Microsoft.Maui.Layouts;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 23491, "BindableLayout.ItemsSource no longer works in 8.0.61", PlatformAffected.All)]
	public class Issue23491 : ContentPage
	{
		Label headerLabel;
		Label label1;
		Label label2;
		public Issue23491()
		{
			headerLabel = new Label
			{
				Text = "Below is the Content of the Child FlexLayout",
				TextColor = Colors.Red
			};

			headerLabel.AutomationId = "HeaderLabel";

			// Child FlexLayout
			label1 = new Label
			{
				Text = "First label inside the child flexlayout",
				Padding = new Thickness(5)
			};

			label2 = new Label
			{
				Text = "Second label inside the child flexLayout",
				Padding = new Thickness(5)
			};

			var childFlexLayout = new FlexLayout
			{
				Margin = new Thickness(10),
				Wrap = FlexWrap.Wrap,
				Children = { label1, label2 }
			};

			// Main FlexLayout
			var mainLayout = new FlexLayout
			{
				Direction = FlexDirection.Column,
				Children = { headerLabel, childFlexLayout }
			};

			// Set the content of the page
			Content = mainLayout;
		}
	}
}