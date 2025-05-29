namespace Maui.Controls.Sample.Issues;
[Issue(IssueTracker.Github, 28838, "Incorrect Text Color Applied to Selected Tab in TabbedPage on Android", PlatformAffected.Android)]
public class Issue28838 : TabbedPage
{
	public Issue28838()
	{
		// Set the UnselectedTabColor property , it should be applied only to the unselected tab.
		UnselectedTabColor = Colors.Red;

		// Add tabs (pages)
		Children.Add(new Issue28838Tab1());
		Children.Add(new Issue28838Tab2());
		Children.Add(new Issue28838Tab3());
	}
}

public class Issue28838Tab1 : ContentPage
{
	public Issue28838Tab1()
	{
		Title = "Tab 1";
		var verticalStackLayout = new VerticalStackLayout
		{
			VerticalOptions = LayoutOptions.Center,
			Children = {
					new Label
						{
							HorizontalOptions = LayoutOptions.Center,
							Text = "Tab 1",
							AutomationId = "Tab1"
						},
					}
		};
		Content = verticalStackLayout;
	}
}

public class Issue28838Tab2 : ContentPage
{
	public Issue28838Tab2()
	{
		Title = "Tab 2";
		var verticalStackLayout = new VerticalStackLayout
		{
			VerticalOptions = LayoutOptions.Center,
			Children = {
					new Label
						{
							HorizontalOptions = LayoutOptions.Center,
							Text = "Tab 2"
						}
					}
		};
		Content = verticalStackLayout;
	}
}
public class Issue28838Tab3 : ContentPage
{
	public Issue28838Tab3()
	{
		Title = "Tab 3";
		var verticalStackLayout = new VerticalStackLayout
		{
			VerticalOptions = LayoutOptions.Center,
			Children = {
					new Label
						{
							HorizontalOptions = LayoutOptions.Center,
							Text = "Tab 3"
						}
					}
		};
		Content = verticalStackLayout;
	}
}