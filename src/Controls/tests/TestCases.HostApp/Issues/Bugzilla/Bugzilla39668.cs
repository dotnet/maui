namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Bugzilla, 39668, "Overriding ListView.CreateDefault Does Not Work on Windows", PlatformAffected.WinRT)]
	public class Bugzilla39668 : TestContentPage
	{

		public class CustomListView : ListView
		{
			protected override Cell CreateDefault(object item)
			{
				var cell = new ViewCell();

				cell.View = new StackLayout
				{
					BackgroundColor = Colors.Green,
					Children = {
						new Label { Text = "Success" }
					}
				};

				return cell;
			}
		}

		protected override void Init()
		{
			CustomListView lv = new CustomListView()
			{
				ItemsSource = Enumerable.Range(0, 10)
			};
			Content = new StackLayout { Children = { new Label { Text = "If the ListView does not have green Cells, this test has failed." }, lv } };
		}
	}
}
