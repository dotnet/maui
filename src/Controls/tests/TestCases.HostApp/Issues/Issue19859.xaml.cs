namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 19859, "NavigationPage: BarBackgroundColor, BarTextColor and Title not updating", PlatformAffected.Android)]
	public partial class Issue19859 : FlyoutPage
    {
        public Issue19859()
        {
            InitializeComponent();
        }

		private static int _counter = 0;

		private async void Button_OnClicked(object sender, EventArgs e)
		{
			var oldBarBackgroundColor = NavPage.BarBackgroundColor;
			NavPage.BarBackgroundColor = oldBarBackgroundColor.Equals(Colors.Yellow)
				? Colors.Red
				: Colors.Yellow;
			var newBarBackgroundColor = NavPage.BarBackgroundColor;

			var oldNavPageBarTextColor = NavPage.BarTextColor;
			NavPage.BarTextColor = oldNavPageBarTextColor.Equals(Colors.Yellow)
				? Colors.Red
				: Colors.Yellow;
			var newNavPageBarTextColor = NavPage.BarTextColor;

			var oldNavPageTitle = NavPage.Title;
			NavPage.Title = oldNavPageTitle == "1"
				? "2"
				: "1";
			var newNavPageTitle = NavPage.Title;

			Btn.Text = $"{_counter++}";

			await DisplayAlert("Alert", $"Changed:{Environment.NewLine}{nameof(oldBarBackgroundColor)}: {oldBarBackgroundColor} => {nameof(newBarBackgroundColor)}: {newBarBackgroundColor}{Environment.NewLine}{nameof(oldNavPageBarTextColor)}: {oldNavPageBarTextColor} => {nameof(newNavPageBarTextColor)}: {newNavPageBarTextColor}{Environment.NewLine}{nameof(oldNavPageTitle)}: {oldNavPageTitle} => {nameof(newNavPageTitle)}: {newNavPageTitle}", "OK");
		}
	}
}