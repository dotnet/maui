namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 25912, "ToolbarItem color when used with IconImageSource is always white", PlatformAffected.iOS | PlatformAffected.macOS)]
	public class Issue25912 : NavigationPage
	{
		public Issue25912()
		{
			Navigation.PushAsync(new Issue25912MainPage());
		}
	}

	public partial class Issue25912MainPage : TestContentPage
	{

		public Issue25912MainPage()
		{
			InitializeComponent();
		}

		protected override void Init()
		{
		}

		void Button_Clicked(object sender, EventArgs e)
		{
			toolbarItem.IconImageSource = new FontImageSource
			{
				Glyph = "+",
				Color = Colors.Green
			};
		}

	}
}
