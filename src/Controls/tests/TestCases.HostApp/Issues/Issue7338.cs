namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 7338, "[Bug] CollectionView crash if source is empty in XF 4.2.0.709249",
		PlatformAffected.iOS)]
	class Issue7338 : NavigationPage
	{
		public Issue7338() : base(new MainPage())
		{
		}

		public class MainPage : ContentPage
		{
			const string Success = "success";

			public MainPage()
			{
				Navigation.PushAsync(CreateRoot());
			}

			Page CreateRoot()
			{
				var page = new ContentPage() { Title = "Issue7338" };

				var instructions = new Label { AutomationId = Success, Text = "If you can see this label, the test has passed." };

				var layout = new StackLayout();

				var cv = new CollectionView
				{
					ItemsLayout = new GridItemsLayout(orientation: ItemsLayoutOrientation.Horizontal),
					ItemTemplate = new DataTemplate(() =>
					{
						return Template();
					})
				};

				layout.Children.Add(instructions);
				layout.Children.Add(cv);

				page.Content = layout;

				return page;
			}

			View Template()
			{
				var label1 = new Label { Text = "Text", HeightRequest = 100 };
				return label1;
			}
		}
	}
}