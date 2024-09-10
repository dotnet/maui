namespace Maui.Controls.Sample.Issues
{
    [Issue(IssueTracker.None, 24147, "Test Handlers for Memory Leaks", PlatformAffected.All)]
    public class MemoryTests : NavigationPage
	{
        public class MemoryTestPage : ContentPage
        {
            Entry entryText;
            public MemoryTestPage()
            {
                Label label = new Label()
                {
                    Text = "Choose a Handler to Test:",
                    HorizontalOptions = LayoutOptions.Start,
                    VerticalOptions = LayoutOptions.Start
                };

                Picker picker = new Picker
                {
                    Title = "Choose a Handler",
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Center
                };

                picker.Items.Add("DatePicker");
                picker.Items.Add("WebView");

                // Handle the selection change
                picker.SelectedIndexChanged += (sender, args) =>
                {
                    if (picker.SelectedIndex != -1)
                    {
                        string selectedItem = picker.Items[picker.SelectedIndex];
                        RunMemoryTest(selectedItem);
                    }
                };

                entryText = new Entry
                {
                    AutomationId = "DataTypeEntry",
                    Placeholder = "This Entry is for Appium to navigate with only.",
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
                    Children = { label, picker, entryText, runMemoryTestButton }
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
                switch(dataType) {
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
		public MemoryTests(): base(new MemoryTestPage()) {}
	}
}
