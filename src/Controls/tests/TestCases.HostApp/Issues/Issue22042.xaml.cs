namespace Maui.Controls.Sample.Issues;

[XamlCompilation(XamlCompilationOptions.Compile)]
[Issue(IssueTracker.Github, 22042, "[Android] Border Stroke GradiantBrush can only switch to another gradiantbrush", PlatformAffected.Android)]

public partial class Issue22042 : ContentPage
{
	private Brush _gradiantPurpleBrush => Resources.TryGetValue("PurpleGradiant", out object laColor) ? (Brush)laColor : null;

	public Issue22042()
	{
		InitializeComponent();
	}

	private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
	{
		if (border.Stroke == _gradiantPurpleBrush)
			border.Stroke = Colors.Red;
		else
			border.Stroke = _gradiantPurpleBrush;
	}
}
