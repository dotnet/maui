using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 28720, "Support for KeepLastItemInView for CV2", PlatformAffected.iOS)]
public partial class Issue28720 : ContentPage
{
	private ObservableCollection<string> _items;
	public ObservableCollection<string> Items
	{
		get => _items;
		set
		{
			if (_items != value)
			{
				_items = value;
				OnPropertyChanged();
			}
		}
	}

	public Command AddItemCommand => new(() =>
	{
		Items.Add($"Item{_items.Count}");
	});

	public Issue28720()
	{
		InitializeComponent();
		Items = [.. Enumerable.Range(0, 20).Select(x => $"Item{x}")];
		BindingContext = this;
	}
}