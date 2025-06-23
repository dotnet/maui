namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 24428, "AppThemeBinding BarBackground with GradientBrush in NavigationPage not working", PlatformAffected.All)]
	public partial class Issue24428NavPage : NavigationPage
	{
		public Issue24428NavPage() : base(new Issue24428ContentPage())
		{
			InitializeComponent();
		}
	}

	public class Issue24428ContentPage : ContentPage
	{
		public Issue24428ContentPage()
		{
			var label1 = new Label()
			{
				AutomationId = "lightThemeLabel",
				Text = "Light theme"
			};

			var label2 = new Label()
			{
				AutomationId = "darkThemeLabel",
				Text = "Dark theme"
			};

			label1.SetAppTheme(IsVisibleProperty, true, false);
			label2.SetAppTheme(IsVisibleProperty, false, true);

			Content = new VerticalStackLayout()
			{
				label1,
				label2
			};
		}
	}
}