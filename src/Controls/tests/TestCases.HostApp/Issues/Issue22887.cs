namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 22887, "Microsoft.Maui.Controls.ImageSource.FromFile fails in iOS when image in subfolder")]
public class Issue22887 : TestContentPage
{
	protected override void Init()
	{
		Content = CreateContent();
	}

	VerticalStackLayout CreateContent()
	{
		var image = new Image
		{
			Source = ImageSource.FromFile("subfolder/dotnet_bot_red.png"),
			AutomationId = "ImageView"
		};

		return new VerticalStackLayout()
		{
			Padding = new Thickness(20),
			Children =
			{
				image,
				new Label
				{
					Text = "If the image is displayed, the test has passed.",
					AutomationId = "descriptionLabel"
				}
			}
		};
	}
}