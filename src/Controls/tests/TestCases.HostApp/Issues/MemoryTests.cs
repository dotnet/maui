namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.None, 24147, "Test Handlers for Memory Leaks", PlatformAffected.All, isInternetRequired: true)]
	public class MemoryTests : NavigationPage
	{
		public class MemoryTestPage : ContentPage
		{
			Entry entryText;
			public MemoryTestPage()
			{
				entryText = new Entry
				{
					AutomationId = "DataTypeEntry",
					Placeholder = "Enter the data type of the Handler you want to test",
				};

				var runMemoryTestButton = new Button
				{
					AutomationId = "RunMemoryTestButton",
					Text = "Run Memory Test"
				};
				runMemoryTestButton.Clicked += OnRunMemoryTestClicked;

				var stackLayout = new StackLayout
				{
					Padding = 10,
					Children = { entryText, runMemoryTestButton }
				};

				Content = stackLayout;
			}

			private void OnRunMemoryTestClicked(object sender, EventArgs e)
			{
				string dataType = entryText.Text;
				RunMemoryTest(dataType);
			}

			private void RunMemoryTest(String dataType)
			{
				switch (dataType)
				{
					case "DatePicker":
						this.Navigation.RunMemoryTest(() =>
						{
							return new DatePicker
							{
								AutomationId = "DatePicker",
								Date = new DateTime(2021, 1, 1)
							};
						});
						break;
					case "WebView": // 22972: Win platform WebView cannot be release after its parent window get closed
						this.Navigation.RunMemoryTest(() =>
						{
							return new WebView
							{
								HeightRequest = 500, // NOTE: non-zero size required for Windows
								Source = new HtmlWebViewSource { Html = "<p>hi</p>" },
							};
						});
						break;
					default:
						break;
				}
			}
		}
		public MemoryTests() : base(new MemoryTestPage()) { }
	}
}
