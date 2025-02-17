using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 27715, "ScrollView inside a Grid expands width past device screen when rotated", PlatformAffected.iOS)]
	public partial class Issue27715 : ContentPage
	{
		public Issue27715()
		{
			InitializeComponent();
			BindingContext = new Issue27715_ViewModel();

		}
	}

	public class Issue27715_ViewModel
	{
		static string[] items = new[] { "Project 1", "Project 2", "Project 3", "Project 4", "Project 5" };

		public ObservableCollection<string> Projects { get; set; } = new ObservableCollection<string>(items);

		public Issue27715_ViewModel()
		{
		}
	}

}