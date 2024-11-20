namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 12484, "Unable to set ControlTemplate for TemplatedView in Xamarin.Forms version 5.0", PlatformAffected.Android)]
	public partial class Issue12484 : TestContentPage
	{
		public Issue12484()
		{
			InitializeComponent();
		}

		protected override void Init()
		{
			Title = "Issue 12484";
		}
	}

	public class Issue12484CustomView : TemplatedView
	{
		public class Issue12484Template : ContentView
		{
			public Issue12484Template()
			{
				var content = new StackLayout()
				{
					Children =
					{
						new Label()
						{
							Text = "If a label with text `Success` does not show up this test has failed"
						}
					}
				};
				Content = content;
			}
		}

		public Issue12484CustomView()
		{
			ControlTemplate = new ControlTemplate(typeof(Issue12484Template));
		}
	}
}