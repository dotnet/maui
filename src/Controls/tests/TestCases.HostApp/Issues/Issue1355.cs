namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 1355, "Setting Main Page in quick succession causes crash on Android",
		PlatformAffected.Android)]
	public class Issue1355 : TestContentPage
	{
		int _runCount = 0;
		int _maxRunCount = 2;
		const string Success = "Success";

		protected override void Init()
		{
			Appearing += OnAppearing;
		}

		private void OnAppearing(object o, EventArgs eventArgs)
		{
			Application.Current.MainPage = CreatePage();
		}

		ContentPage CreatePage()
		{
			var page = new ContentPage
			{
				Content = new Label { AutomationId = Success, Text = Success },
				Title = $"CreatePage Iteration: {_runCount}"
			};

			page.Appearing += (sender, args) =>
			{
				_runCount += 1;
				if (_runCount <= _maxRunCount)
				{
					Application.Current.MainPage = new NavigationPage(CreatePage());
				}
			};

			return page;
		}
	}
}
