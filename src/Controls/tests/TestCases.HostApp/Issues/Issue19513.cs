using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues;
[Issue(IssueTracker.Github, 19513, "HorizontalStackLayout Crashes Debugger on Negative Spacing", PlatformAffected.UWP)]
public class Issue19513:ContentPage
{
	public Issue19513()
	{
		var stackLayout = new StackLayout
		{
			AutomationId = "StackLayout",
			Spacing = -60
		};
 
		var Image1 = new Image
		{
			Source = "groceries.png",
			AutomationId = "Image1"
		};
 
		var Image2 = new Image
		{
			Source = "shopping_cart.png",
			AutomationId = "Image2"
		};
 
		stackLayout.Children.Add(Image1);
		stackLayout.Children.Add(Image2);
 
		Content = stackLayout;
	}
}