using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues;

[XamlCompilation(XamlCompilationOptions.Compile)]
[Issue(IssueTracker.Github, 33034, "SafeAreaEdges works correctly only on the first tab in Shell. Other tabs have content colliding with the display cutout in the landscape mode.", PlatformAffected.Android)]
public partial class Issue33034 : TestShell
{
	public Issue33034()
	{
		InitializeComponent();
	}

	protected override void Init()
	{
		// Create TabBar with two tabs using the same content page
		var tabBar = new TabBar();
		
		var tab = new Tab { Title = "Tabs" };
		
		tab.Items.Add(new ShellContent
		{
			Title = "First Tab",
			AutomationId = "FirstTab",
			ContentTemplate = new DataTemplate(typeof(Issue33034TabContent)),
			Route = "tab1"
		});
		
		tab.Items.Add(new ShellContent
		{
			Title = "Second Tab",
			AutomationId = "SecondTab",
			ContentTemplate = new DataTemplate(typeof(Issue33034TabContent)),
			Route = "tab2"
		});
		
		tabBar.Items.Add(tab);
		Items.Add(tabBar);
	}
}
