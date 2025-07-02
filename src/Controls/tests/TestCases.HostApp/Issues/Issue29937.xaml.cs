using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 29937, "[iOS/MacCatalyst] Setting SelectedItem Programmatically and Then Immediately Setting ItemsSource to Null Causes a Crash", PlatformAffected.iOS | PlatformAffected.macOS)]
public partial class Issue29937 : ContentPage
{
	private ObservableCollection<string> items;
	public ObservableCollection<string> Items
	{
		get => items;
		set
		{
			items = value;
			OnPropertyChanged();
		}
	}

	public Issue29937()
	{
		InitializeComponent();
		Items = new ObservableCollection<string>();
		for (int i = 1; i <= 5; i++)
		{
			Items.Add($"Item{i}");
		}
		BindingContext = this;
	}

	private void OnTestButtonClicked(object sender, EventArgs e)
	{
		try
		{
			// This is the scenario that causes the crash:
			// 1. Set SelectedItem programmatically to a valid item
			collectionView.SelectedItem = Items.FirstOrDefault();
			
			// 2. Immediately set ItemsSource to null
			Items = null;
			
			// If we reach here without crashing, the issue is fixed
			var successLabel = this.FindByName<Label>("SuccessLabel");
			if (successLabel != null)
			{
				successLabel.IsVisible = true;
			}
		}
		catch (Exception ex)
		{
			// This would be the crash scenario
			System.Diagnostics.Debug.WriteLine($"Crash occurred: {ex}");
			throw;
		}
	}
}