namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Bugzilla, 41205, "UWP CreateDefault passes string instead of object")]
	public class Bugzilla41205 : TestContentPage
	{
		const string _success = "Pass";


		public class ViewModel
		{
			public string Text { get { return _success; } }
		}


		public class CustomListView : ListView
		{
			protected override Cell CreateDefault(object item)
			{
				if (item is ViewModel)
				{
					var newTextCell = new TextCell();
					newTextCell.SetBinding(TextCell.TextProperty, nameof(ViewModel.Text));
					return newTextCell;
				}
				return base.CreateDefault("Fail");
			}
		}

		protected override void Init()
		{
			var listView = new CustomListView
			{
				ItemsSource = new[]
				{
					new ViewModel(),
					new ViewModel(),
				}
			};

			Content = listView;
		}
	}
}
