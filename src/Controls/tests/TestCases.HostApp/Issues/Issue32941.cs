using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 32941, "Label Overlapped by Android Status Bar When Using SafeAreaEdges=Container in .NET MAUI", PlatformAffected.Android)]
public class Issue32941 : TestShell
{
	protected override void Init()
	{
		var shellContent1 = new ShellContent
		{
			Title = "Home",
			Route = "Issue32941_ContentPage_1",
			Content = new Issue32941_ContentPage_1()
		};
		var shellContent2 = new ShellContent
		{
			Title = "Home",
			Route = "Issue32941_ContentPage_2",
			Content = new Issue32941_ContentPage_2()
		};
		Items.Add(shellContent1);
		Items.Add(shellContent2);
	}
}

public class Issue32941_ContentPage_1 : ContentPage
{
	public Issue32941_ContentPage_1()
	{
		var label = new Label
		{
			Text = "Click the button to navigate to the next page.",
		};

		var button = new Button
		{
			Text = "Go to Next Page",
			AutomationId = "NavigateToNextPageBtn",
		};

		button.Clicked += async (s, e) =>
		{
			await Shell.Current.GoToAsync("//Issue32941_ContentPage_2");
		};

		Content = new StackLayout
		{
			Padding = new Thickness(20),
			Spacing = 20,
			Children =
				{
					label,
					button
				}
		};
	}
}

public class Issue32941_ContentPage_2 : ContentPage
{
	public Issue32941_ContentPage_2()
	{
		Shell.SetNavBarIsVisible(this, false);
		SafeAreaEdges = new SafeAreaEdges(SafeAreaRegions.Container);
		var verticalStackLayout = new VerticalStackLayout
		{
			Padding = new Thickness(20),
			Spacing = 20
		};

		var label = new Label
		{
			Text = "Test Label. This label should not be overlapped by the Android status bar when SafeAreaEdges is set to Container.",
			FontSize = 24,
			AutomationId = "testLabel"
		};

		verticalStackLayout.Children.Add(label);

		Content = verticalStackLayout;
	}
}