using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues;

[XamlCompilation(XamlCompilationOptions.Compile)]
[Issue(IssueTracker.Github, 22633, "[iOS] Crash on when initializing a TabbedPage without children", PlatformAffected.iOS)]
public partial class Issue22633 : TabbedPage
{
	public Issue22633()
	{
		InitializeComponent();
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		_ = SimulateNavigationServiceAsync();
	}

	async Task SimulateNavigationServiceAsync()
	{
		// add a delay to simulate the navigation service
		await Task.Delay(100);

		Children.Add(new ContentPage
		{
			Title = "Page1",
			Content = new VerticalStackLayout()
			{
				new Label() { AutomationId = "label", Text = "Hello, World!" }
			}
		});

		Children.Add(new ContentPage { Title = "Page2" });
	}
}
