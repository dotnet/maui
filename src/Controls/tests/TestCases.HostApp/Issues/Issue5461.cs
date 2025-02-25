namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 5461, "[Android] ScrollView crashes when setting ScrollbarFadingEnabled to false in Custom Renderer",
		PlatformAffected.Android)]
	public class Issue5461 : TestContentPage
	{
		const string Success = "If you can see this, the test has passed";
		protected override void Init()
		{
			ScrollView scrollView = new ScrollbarFadingEnabledFalseScrollView()
			{
				Content = new StackLayout()
				{
					Children =
					{
						new Label()
						{
							Text = Success
						}
					},
					HeightRequest = 2000
				}
			};

			Content = scrollView;
		}

		public class ScrollbarFadingEnabledFalseScrollView : ScrollView { }
	}
}
