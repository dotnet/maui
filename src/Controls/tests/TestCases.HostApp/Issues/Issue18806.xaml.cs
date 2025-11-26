using System.ComponentModel;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 18806, "SwipeItemView won't render FontImageSource on first opening", PlatformAffected.Android)]
	public partial class Issue18806 : ContentPage
	{
		public Issue18806()
		{
			InitializeComponent();
		}

		private void Button_Clicked(object sender, EventArgs e)
		{
			swipeView1.Open(OpenSwipeItem.RightItems, false);
			swipeView2.Open(OpenSwipeItem.LeftItems, false);
		}
	}
}