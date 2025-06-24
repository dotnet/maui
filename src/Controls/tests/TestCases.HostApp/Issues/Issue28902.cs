using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 28902, "Label disappears when android:hardwareAccelerated=\"false\"", PlatformAffected.Android)]
	public partial class Issue28902 : TestContentPage
	{
		const string TargetLabelId = "TestLabel";
		const string TargetLabelText = "Test";

		protected override void Init()
		{
			var grid = new Grid
			{
				RowDefinitions = GridRowDefinitionCollection.Parse("Auto, *")
			};

			var label = new Label
			{
				Text = TargetLabelText,
				AutomationId = TargetLabelId
			};
			Grid.SetRow(label, 0);

			var webView = new WebView
			{
				Source = "https://google.com"
			};
			Grid.SetRow(webView, 1);

			grid.Children.Add(label);
			grid.Children.Add(webView);

			Content = grid;
		}
	}
}