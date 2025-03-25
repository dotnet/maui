namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 8814,
		"[Bug] UWP Shell cannot host CollectionView/CarouselView",
		PlatformAffected.UWP)]
	public class Issue8814 : TestShell
	{
		const string Success = "Success";

		protected override void Init()
		{
			var cv = new CollectionView();
			var items = new List<string>() { Success, "two", "three" };
			cv.ItemTemplate = new DataTemplate(() =>
			{

				var layout = new StackLayout();

				var label = new Label();
				label.SetBinding(Label.TextProperty, new Binding("."));
				label.SetBinding(Label.AutomationIdProperty, new Binding("."));

				layout.Children.Add(label);

				return layout;
			});

			cv.ItemsSource = items;

			var page = CreateContentPage<FlyoutItem>("CollectionView");

			var instructions = new Label { Text = "The should be a CollectionView visible below. If not, this test has failed. Unfortunately, without the fix for this bug, these instructions also won't be visible. 🤔" };

			page.Content = new StackLayout()
			{
				Children =
				{
					instructions,
					cv
				}
			};
		}
	}
}
