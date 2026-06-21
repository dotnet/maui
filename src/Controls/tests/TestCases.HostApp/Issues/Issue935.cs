namespace Maui.Controls.Sample.Issues
{

	public class CustomViewCell : ViewCell
	{
		public CustomViewCell()
		{
			int tapsFired = 0;

			Height = 50;

			var label = new Label
			{
				AutomationId = "TestLabel",
				Text = "I have been selected:"
			};

			if (this is CustomViewCellBindingContext)
				label.Text = "If you can read this text the UI Test has failed";

			Tapped += (s, e) =>
			{
				tapsFired++;
				label.Text = "I have been selected:" + tapsFired;

				var cell = (CustomViewCell)s;
			};

			View = label;
		}
	}



	public class CustomViewCellBindingContext : CustomViewCell
	{
		public CustomViewCellBindingContext()
		{
		}
	}


	[Issue(IssueTracker.Github, 935, "ViewCell.ItemTapped only fires once for ListView.SelectedItem", PlatformAffected.Android)]
	public class Issue935 : TestContentPage
	{
		protected override void Init()
		{
			Title = "List Page";

			var items = new[] {
				new CustomViewCellBindingContext()
			};

			var cellTemplate = new DataTemplate(typeof(CustomViewCell));

			var list = new ListView()
			{
				ItemTemplate = cellTemplate,
				ItemsSource = items
			};
			SafeAreaEdges = new SafeAreaEdges(SafeAreaRegions.Container);
			Content = list;
		}
	}
}