namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, "19609", "Button clicked event and command will not be occurred in EmptyView of CollectionView", PlatformAffected.UWP)]
	public partial class Issue19609 : ContentPage
	{
		public Issue19609()
		{
			InitializeComponent();
		}

		void Button_Clicked(object sender, EventArgs e)
		{
			(sender as Button).Text = "Clicked";
		}
	}
}