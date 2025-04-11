using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 25842, "Pickers Inside CollectionView get SelectedItem Cleared on Scrolling", PlatformAffected.iOS)]
public partial class Issue25842 : ContentPage
{
	public Issue25842()
	{
		InitializeComponent();
		BindingContext = new Issue25842ViewModel();
	}

	class Issue25842ViewModel
	{
		public ObservableCollection<Student> Students { get; set; }

		public Issue25842ViewModel()
		{
			Students = new()
			{
				new () { Name = "John Doe", Country = "United States" },
				new () { Name = "Jane Smith", Country = "Canada" },
				new () { Name = "Sam Brown", Country = "India" },
				new () { Name = "Lisa White", Country = "United States" },
				new () { Name = "Tom Green", Country = "Canada" },
				new () { Name = "Emma Black", Country = "India" },
				new () { Name = "Noah Blue", Country = "United States" },
				new () { Name = "Olivia Yellow", Country = "Canada" },
				new () { Name = "Liam Brown", Country = "India" },
				new () { Name = "Sophia Pink", Country = "United States" }
			};
		}

		public class Student
		{
			public required string Name { get; set; }
			public required string Country { get; set; }
			public ObservableCollection<string> Countries { get; set; } = new() { "United States", "Canada", "India" };
		}
	}
}
