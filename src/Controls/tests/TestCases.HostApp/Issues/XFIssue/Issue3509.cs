namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Github, 3509, "[iOS] NavigationPage.Popped called twice when Navigation.PopAsync is called", PlatformAffected.iOS)]

public class Issue3509 : TestNavigationPage
{
	const string _popPage = "Pop Page";
	protected override void Init()
	{
		int popCount = 0;
		Label label = new Label();

		PushAsync(new ContentPage()
		{
			Content = new StackLayout()
			{
				Children =
				{
					new Label(){ Text = "If the number below is not a one test has failed"},
					label,
					new Button()
					{
						Text = "Push a Page",
						Command = new Command(() =>
						{
							PushAsync(new TestPage());
						})
					}
				}
			}
		});

		PushAsync(new TestPage());
		Popped += (s, e) =>
		{
			popCount++;
			label.Text = $"{popCount}";
		};
	}


	public class TestPage : ContentPage
	{
		bool _popped = false;

		public TestPage()
		{
			Title = "Test page";
			var content = new StackLayout();
			content.Children.Add(new Button
			{
				Text = _popPage,
				Command = new Command(() =>
				{
					Navigation.PopAsync(false);
				}),
			});

			Content = content;
		}

		internal void Popped()
		{
			if (_popped)
				throw new Exception("Already popped");

			_popped = true;
		}
	}
}
