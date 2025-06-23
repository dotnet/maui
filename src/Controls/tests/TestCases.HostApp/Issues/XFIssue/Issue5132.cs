namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 5132, "Unable to specify automation properties on the hamburger/flyout icon", PlatformAffected.Default)]
public class Issue5132 : TestShell
{
	string _idIconElement = "shellIcon";
	string _titleElement = "Connect";
	protected override void Init()
	{
		Title = "Shell";
		FlyoutIcon = new FontImageSource
		{
			Glyph = "\uf224",
			FontFamily = "Ion",
			Size = 20,
			AutomationId = _idIconElement
		};

		FlyoutIcon.SetValue(SemanticProperties.HintProperty, "This as Shell FlyoutIcon");

		FlyoutIcon.SetValue(SemanticProperties.DescriptionProperty, "Shell Icon");

		Items.Add(new FlyoutItem
		{
			Title = _titleElement,
			Items = {
				new Tab { Title = "library",
					Items = {
								new ContentPage { Title = "Library",  Content = new ScrollView { Content = new Label  { Text = "Turn accessibility on and make sure the help text is read on iOS, on Android it will read the AutomationID if specified and then the HelpText this allows UITest to work " } } }
							}
					}
			}
		});
	}

	// 	static string DefaultFontFamily()
	// 	{
	// 		var fontFamily = "";
	// #pragma warning disable CS0618 // Type or member is obsolete
	// #pragma warning disable CS0618 // Type or member is obsolete
	// #pragma warning disable CS0618 // Type or member is obsolete
	// #pragma warning disable CS0618 // Type or member is obsolete
	// #pragma warning disable CS0612 // Type or member is obsolete
	// #pragma warning disable CS0612 // Type or member is obsolete
	// #pragma warning disable CS0612 // Type or member is obsolete
	// #pragma warning disable CS0612 // Type or member is obsolete
	// 		switch (Device.RuntimePlatform)
	// 		{
	// 			case Device.iOS:
	// 				fontFamily = "Ionicons";
	// 				break;
	// 			case Device.WinUI:
	// 				fontFamily = "Assets/Fonts/ionicons.ttf#ionicons";
	// 				break;
	// 			case Device.Android:
	// 			default:
	// 				fontFamily = "fonts/ionicons.ttf#";
	// 				break;
	// 		}
	// #pragma warning restore CS0612 // Type or member is obsolete
	// #pragma warning restore CS0612 // Type or member is obsolete
	// #pragma warning restore CS0612 // Type or member is obsolete
	// #pragma warning restore CS0612 // Type or member is obsolete
	// #pragma warning restore CS0618 // Type or member is obsolete
	// #pragma warning restore CS0618 // Type or member is obsolete
	// #pragma warning restore CS0618 // Type or member is obsolete
	// #pragma warning restore CS0618 // Type or member is obsolete

	// 		return fontFamily;
	// 	}
}
