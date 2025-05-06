namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 20435, "iOS: FlyoutItemImageStyle breaks selection of first item on startup", PlatformAffected.iOS)]
public class Issue20435 : Shell
{
	public Issue20435()
	{
        this.FlyoutBehavior = FlyoutBehavior.Flyout;
        this.Title = "FlyoutItemImageStyle_issue";

        var imageStyle = new Style(typeof(Image))
        {
            Class = "FlyoutItemImageStyle"
        };
        imageStyle.Setters.Add(new Setter { Property = Image.HeightRequestProperty, Value = 15 });
        imageStyle.Setters.Add(new Setter { Property = Image.WidthRequestProperty, Value = 15 });

        this.Resources = new ResourceDictionary
        {
            imageStyle
        };

        var flyoutItem = new FlyoutItem
        {
            FlyoutDisplayOptions = FlyoutDisplayOptions.AsMultipleItems
        };

        var shellContent1 = new ShellContent
        {
            Title = "Page1",
            Route = "MainPage1",
            ContentTemplate = new DataTemplate(typeof(Issue20435_MainPage))
        };

        var shellContent2 = new ShellContent
        {
            Title = "Page2",
            Route = "MainPage2",
            ContentTemplate = new DataTemplate(typeof(Issue20435_MainPage))
        };

        flyoutItem.Items.Add(shellContent1);
        flyoutItem.Items.Add(shellContent2);

        this.Items.Add(flyoutItem);
	}

	public class Issue20435_MainPage : ContentPage
	{
		public Issue20435_MainPage()
		{
			var OpenFlyoutButton = new Button
			{
				Text = "Open Flyout",
				AutomationId = "OpenFlyoutButton"
			};

			OpenFlyoutButton.Clicked += (sender, args) => Shell.Current.FlyoutIsPresented = true;
			Content = new StackLayout()
			{
				Children = { OpenFlyoutButton }
			};
		}
	}
}