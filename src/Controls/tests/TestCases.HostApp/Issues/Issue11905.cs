namespace Maui.Controls.Sample.Issues;

[XamlCompilation(XamlCompilationOptions.Compile)]
[Issue(IssueTracker.Github, 11905, "WebView.EvaluateJavaScriptAsync fails to execute JavaScript containing newline characters", PlatformAffected.iOS | PlatformAffected.UWP)]
public partial class Issue11905 : ContentPage
{
	private WebView Browser;
    public Issue11905()
    {
        Browser = new WebView
        {
            Source = "https://example.com",
			AutomationId = "WebView",
            VerticalOptions = LayoutOptions.Fill,
            HorizontalOptions = LayoutOptions.Fill
        };
        
        Browser.Navigated += Browser_Navigated;
        
        Content = Browser;
    }
    
    private async void Browser_Navigated(object sender, WebNavigatedEventArgs e)
	{
		string jsCode = "document.body.style.backgroundColor = 'blue';\r\n" +
					"var box = document.createElement('div');\r" +
					"box.style.position = 'fixed';\n" +
					"box.style.cssText = 'top:40%; left:20%; width:60%; background-color:red; border:5px solid black; padding:20px;';" +
					"box.innerHTML = '<h2 id=\"newline-test-header\" style=\"color:white\">Newline Test</h2>';" +
					"document.body.appendChild(box);";
		
		await Browser.EvaluateJavaScriptAsync(jsCode);
		
		var result = await Browser.EvaluateJavaScriptAsync(
			"document.getElementById('newline-test-header') !== null");
		
		if (result.ToLowerInvariant() == "true")
		{
			var testLabel = new Label { 
				Text = "TestLabel", 
				AutomationId = "TestLabel",
			};
			
			var grid = new Grid();
			grid.Children.Add(Browser);
			grid.Children.Add(testLabel);
			Content = grid;
		}
	}
}