using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 32493, "Selected item color changes from lightskyblue to lightgray after scrolling on iOS 26.1", PlatformAffected.iOS)]
public partial class Issue32493 : ContentPage
{
	public ObservableCollection<TestItem> Items { get; set; }

	public Issue32493()
	{
		InitializeComponent();

		Items = new ObservableCollection<TestItem>();
		for (int i = 0; i < 50; i++)
		{
			Items.Add(new TestItem
			{
				Title = $"Item {i}",
				Description = $"Description for item {i}"
			});
		}

		BindingContext = this;
	}

	public class TestItem
	{
		public string Title { get; set; }
		public string Description { get; set; }
	}
}
