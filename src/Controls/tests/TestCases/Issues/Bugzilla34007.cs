using System;
using System.Xml.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 34007, "Z order drawing of children views are different on Android, iOS, Win", PlatformAffected.Android | PlatformAffected.iOS)]
	public class Bugzilla34007 : TestContentPage
	{
		protected override void Init()
		{
			var grid = new Grid();

			var button0 = new Button
			{
				AutomationId = "Button0",
				Text = "Button 0",
				HorizontalOptions = LayoutOptions.Fill,
				VerticalOptions = LayoutOptions.Fill
			};

			var button1 = new Button
			{
				AutomationId = "Button1",
				Text = "Button 1",
				HorizontalOptions = LayoutOptions.Fill,
				VerticalOptions = LayoutOptions.Fill
			};

			var lastButtonTappedLabel = new Label();

			Action reorder = () =>
			{
				// Get the last item in the grid
				var item = grid.Children[1];

				// Remove it
				grid.Children.RemoveAt(1);

				// And put it back as the 0th item
				grid.Children.Insert(0, item);
			};

			button0.Clicked += (sender, args) =>
			{
				lastButtonTappedLabel.Text = "Button 0 was tapped last";
			};

			button1.Clicked += (sender, args) =>
			{
				lastButtonTappedLabel.Text = "Button 1 was tapped last";

				reorder();
			};

			grid.Add(button0, 0, 0);
			grid.Add(button1, 0, 0);

			Content = new StackLayout
			{
				Children = { grid, lastButtonTappedLabel }
			};
		}
	}
}