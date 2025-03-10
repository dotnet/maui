namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 27101, "PlatformView cannot be null here Exception in Windows")]
	public class Issue27101 : NavigationPage
	{
		public Issue27101() : base(new Issue27101MainPage())
		{
		}
	}

	public partial class Issue27101MainPage : TestContentPage
	{
		public Issue27101MainPage()
		{
			InitializeComponent();
		}

		protected override void Init()
		{

		}

		void OnNavigateButtonClicked(object sender, EventArgs e)
		{
			Navigation.PushAsync(new Issue27101SecondPage());
		}
	}
}