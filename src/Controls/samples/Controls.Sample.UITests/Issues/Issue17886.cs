using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls.Shapes;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 17886, "Shadow wrong scaling")]
public class Issue17886 : TestContentPage
{
	protected override void Init()
	{
		Title = "Issue 17886";

		VerticalStackLayout layout = new VerticalStackLayout
		{
			Margin = new Thickness(0, 30),
		};

		Content = layout;

		Label label = new Label
		{
			AutomationId = "WaitForStubControl",
			Text = "Shadow wrong scaling"
		};
		
		layout.Children.Add(label);

		Border border = new Border();
		border.WidthRequest = border.HeightRequest = 200;
		border.StrokeShape = new RoundRectangle() { CornerRadius = 30 };
		border.BackgroundColor = Colors.Bisque;

		layout.Children.Add(border);

		border.Shadow = new Shadow
		{
			Offset = new Point(0, 10),
			Radius = 10,
			Brush = Colors.Red
		};
	}
}