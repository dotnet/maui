namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 22306, "Resizing buttons' parents resolves layout", PlatformAffected.iOS)]
	public partial class Issue22306 : ContentPage
	{
		bool shouldSizeDown = false;

		public Issue22306()
		{
			InitializeComponent();
		}

		void ChangeBoundsPressed(object sender, EventArgs e)
		{
			shouldSizeDown = !shouldSizeDown;

			RowAbove.Height = new GridLength(shouldSizeDown ? 3 : 1, GridUnitType.Star);
			RowBelow.Height = new GridLength(shouldSizeDown ? 3 : 1, GridUnitType.Star);
			ColumnBefore.Width = new GridLength(shouldSizeDown ? 3 : 1, GridUnitType.Star);
			ColumnAfter.Width = new GridLength(shouldSizeDown ? 3 : 1, GridUnitType.Star);
		}
	}
}
