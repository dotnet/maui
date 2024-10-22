namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 20696, "[iOS] FlyoutHeader does not change its size after adding new content", PlatformAffected.iOS)]
public class Issue20696Test : ContentPage
{
	public Issue20696Test()
	{
		Content = new VerticalStackLayout()
		{
			Children =
				{
					new Button()
					{
						Text = "Go To Test",
						AutomationId = "GoToTest",
						Command = new Command(() => this.Window.Page = new Issue20696())
					}
				}
		};
	}
}

public partial class Issue20696 : Shell
{
	public Issue20696()
	{
		InitializeComponent();
	}

	void Button_Clicked(object sender, System.EventArgs e)
	{
		if (Current.FlyoutHeader is StackLayout stackLaylout)
		{
			stackLaylout.Add(new Button
			{
				Text = "Test button 2",
				AutomationId = "TestButton2",
				BackgroundColor = Colors.Red
			});
		}

		Current.FlyoutIsPresented = true;
	}
}
