namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 7156, "Fix for wrong secondary ToolbarItem size on Windows", PlatformAffected.UWP)]
	public class Issue7156 : NavigationPage
	{
		public Issue7156()
		{
			Navigation.PushAsync(new Issue7156MainPage());
		}
	}

	public partial class Issue7156MainPage : TestContentPage
	{
		public Issue7156MainPage()
		{
			InitializeComponent();
		}

		protected override void Init()
		{

		}

		void OnItemClicked(object sender, EventArgs e)
		{
			ToolbarItem item = (ToolbarItem)sender;
			messageLabel.Text = $"You clicked the \"{item.Text}\" toolbar item.";
		}
	}
}