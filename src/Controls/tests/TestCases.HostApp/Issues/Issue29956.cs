namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 29956, "ImageButton border is obscured with AspectFill and URI sources", PlatformAffected.Android, isInternetRequired: true)]
public class Issue29956 : ContentPage
{
	public Issue29956()
	{
		Content = new VerticalStackLayout
		{
			Padding = 16,
			Spacing = 4,
			Children =
			{
				CreateCaseLabel("URI source - AspectFill"),
				CreateImageButton("UriAspectFill", CreateUriSource(), Aspect.AspectFill),
				CreateCaseLabel("URI source - Center"),
				CreateImageButton("UriAspectCenter", CreateUriSource(), Aspect.Center),
				CreateCaseLabel("File source - AspectFill"),
				CreateImageButton("FileAspectFill", ImageSource.FromFile("groceries.png"), Aspect.AspectFill)
			}
		};
	}

	static ImageSource CreateUriSource() =>
		ImageSource.FromUri(new Uri("https://aka.ms/campus.jpg"));

	static Label CreateCaseLabel(string text) =>
		new()
		{
			Text = text,
			FontSize = 14
		};

	static ImageButton CreateImageButton(string automationId, ImageSource source, Aspect aspect) =>
		new()
		{
			AutomationId = automationId,
			Source = source,
			Aspect = aspect,
			WidthRequest = 220,
			HeightRequest = 130,
			BorderColor = Colors.Red,
			BorderWidth = 6
		};
}
