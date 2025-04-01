namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 23484, "TabbedPage Clear and Adding existing page does not display across the entire screen", PlatformAffected.iOS)]
	public class Issue23484 : TabbedPage
	{
		public Page _persistedTestPage;

		public Issue23484()
		{
			_persistedTestPage = new NavigationPage(new TestPage(this)) { Title = "Test" };
			Children.Add(_persistedTestPage);
			Children.Add(new NavigationPage(new TestPage(this)) { Title = "Test" });
			Children.Add(new NavigationPage(new TestPage(this)) { Title = "Test" });
			Children.Add(new NavigationPage(new TestPage(this)) { Title = "Test" });
			Children.Add(new NavigationPage(new TestPage(this)) { Title = "Test" });
		}

		public async void ClearAndAddPages()
		{
			await Task.Delay(100);
			Children.Clear();
			await Task.Delay(100);

			Children.Add(_persistedTestPage);
			Children.Add(new NavigationPage(new TestPage(this)) { Title = "Test" });
			Children.Add(new NavigationPage(new TestPage(this)) { Title = "Test" });
			Children.Add(new NavigationPage(new TestPage(this)) { Title = "Test" });
			Children.Add(new NavigationPage(new TestPage(this)) { Title = "Test" });
		}

		class TestPage : ContentPage
		{
			Label _label;
			public TestPage(Issue23484 tabbedPage)
			{
				Title = "Test Page";
				_label = new Label()
				{
					AutomationId = "SizeLabel"
				};

				BackgroundColor = Colors.Red;
				var grid =
					new Grid()
					{
						_label,
						new Button
						{
							Text = "Clear and Add Pages",
							AutomationId = "RecreateButton",
							Command = new Command(() => tabbedPage.ClearAndAddPages())
						},
						new Label() { Text = "Bottom Of Page"}
					};

				grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
				grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
				grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });

				grid.SetRow(grid.Children[0], 0);
				grid.SetRow(grid.Children[1], 1);
				grid.SetRow(grid.Children[2], 2);

				Content = grid;
			}

			protected override void OnSizeAllocated(double width, double height)
			{
				base.OnSizeAllocated(width, height);
				_label.Text = $"{width} x {height}";
			}
		}
	}
}
