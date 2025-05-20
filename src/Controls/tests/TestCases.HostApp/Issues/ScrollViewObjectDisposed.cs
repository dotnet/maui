namespace Maui.Controls.Sample.Issues
{
	// ScrollViewObjectDisposedTest (src\Compatibility\ControlGallery\src\Issues.Shared\ScrollViewObjectDisposed.cs)
	[Issue(IssueTracker.None, 0, "Object Disposed Exception in ScrollView", PlatformAffected.All)]
	public class ScrollViewObjectDisposed : ContentPage
	{
		const string Instructions = "Tap the button. If the app does not crash and the red label displays \"Success\", this test has passed.";
		const string Success = "Success";
		const string TestButtonId = "TestButtonId";
		Label _status;
		ScrollView _scroll;

		public ScrollViewObjectDisposed()
		{
			InitTest();
		}

		void InitTest()
		{
			_status = new Label() { Text = "Test is running...", BackgroundColor = Colors.Red, TextColor = Colors.White };

			_scroll = new ScrollView
			{
				Content = _status
			};

			Button nextButton = new Button { Text = "Next", AutomationId = TestButtonId };
			nextButton.Clicked += NextButton_Clicked;
			var stack = new StackLayout
			{
				Children =
				{
					new Label { Text = Instructions },
					_scroll,
					nextButton
				}
			};
			this.Content = stack;
		}
		void NextButton_Clicked(object sender, EventArgs e)
		{
			_status.Text = string.Empty;
			InitTest();
			_status.Text = Success;
		}
	}
}

