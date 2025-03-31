using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 28321, "CV RemainingItemsThresholdReachedCommand fires on initial data load", PlatformAffected.Android)]
public partial class Issue28321 : ContentPage
{
	public readonly record struct Data(string Text, string AutomationId);
	public Issue28321()
	{
		InitializeComponent();
		BindingContext = new Issue28321ViewModel();
	}

	public class Issue28321ViewModel : ViewModel
	{

		public ObservableCollection<string> Items { get; set; }
		public Command LoadMoreItemsCommand { get; }
		public Issue28321ViewModel()
		{
			LoadMoreItemsCommand = new Command(() =>
			{
				int size = Items.Count;
				for (int i = size; i < size + 10; i++)
					Items.Add($"Item{i}");
			});
			_ = LoadItems();
		}

		async Task LoadItems()
		{
			await Task.Delay(1000);
			Items = [.. Enumerable.Range(0, 4).Select(x => $"Item{x}").ToList()];
			OnPropertyChanged(nameof(Items));
		}
	}
}