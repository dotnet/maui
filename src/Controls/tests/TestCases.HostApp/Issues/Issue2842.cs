using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 2842, "ViewCell in TableView not adapting to changed size on iOS", PlatformAffected.iOS)]
	public class Issue2842 : TestContentPage
	{
		protected override void Init()
		{
			var grid = new Grid
			{
				Padding = 10,
				ColumnDefinitions = { new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) } },
				RowDefinitions = { new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) } }
			};

			grid.Add(new Label { Text = "I am initially visible." }, 0, 0);

			Label target = new Label { Text = "Success", AutomationId = "lblSuccess", IsVisible = false, TextColor = Colors.Red };

			grid.Add(target, 0, 1);

			var tableView = new TableView { HasUnevenRows = true, Intent = TableIntent.Settings };

			ViewCell viewCell = new ViewCell { View = grid };
			TableSection item = new TableSection
			{
				viewCell,
				new ImageCell {  ImageSource = "cover1.jpg" }
			};

			tableView.Root.Add(item);

			var button = new Button
			{
				Text = "Click me",
				AutomationId = "btnClick",
				Command = new Command(() =>
				{
					target.IsVisible = true;
					viewCell.ForceUpdateSize();
				})
			};

			var label = new Label { Text = "Tap the button to expand the cell. If the cell does not expand and the red text is on top of the image, this test has failed." };

			Content = new StackLayout { Children = { label, button, tableView }, Margin = 20 };
		}
	}
}