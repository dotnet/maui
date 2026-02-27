using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 31377, "TabbedPage overflow 'More' button does not work", PlatformAffected.iOS)]
public partial class Issue31377 : TabbedPage
{
	public Issue31377()
	{
		InitializeComponent();
		BindingContext = new TabbedPageViewModel();
	}

	public class TabbedPageViewModel
	{
		public ObservableCollection<TabbedPageItemSource> ItemsSource { get; } =
		[
			new TabbedPageItemSource { Name = "Tab 1", ImageUrl = "dotnet_bot.png" },
			new TabbedPageItemSource { Name = "Tab 2", ImageUrl = "dotnet_bot.png" },
			new TabbedPageItemSource { Name = "Tab 3", ImageUrl = "dotnet_bot.png" },
			new TabbedPageItemSource { Name = "Tab 4", ImageUrl = "dotnet_bot.png" },
			new TabbedPageItemSource { Name = "Tab 5", ImageUrl = "dotnet_bot.png" },
			new TabbedPageItemSource { Name = "Tab 6", ImageUrl = "dotnet_bot.png" },
			new TabbedPageItemSource { Name = "Tab 7", ImageUrl = "dotnet_bot.png" },
			new TabbedPageItemSource { Name = "Tab 8", ImageUrl = "dotnet_bot.png" },
			new TabbedPageItemSource { Name = "Tab 9", ImageUrl = "dotnet_bot.png" },
			new TabbedPageItemSource { Name = "Tab 10", ImageUrl = "dotnet_bot.png" }
		];
	}

	public class TabbedPageItemSource
	{
		public string Name { get; set; }
		public string ImageUrl { get; set; }
	}
}