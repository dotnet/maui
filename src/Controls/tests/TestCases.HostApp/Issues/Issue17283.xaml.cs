using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 17283, "[Android] CarouselView doesn't scroll to the right Position after changing the ItemSource with Loop enabled", PlatformAffected.Android)]
	public partial class Issue17283 : ContentPage
	{
		public Issue17283()
		{
			InitializeComponent();
			BindingContext = new Issue17283ViewModel();
		}
	}

	public class Issue17283ViewModel : INotifyPropertyChanged
	{
		ObservableCollection<string> _items;
		public ObservableCollection<string> Items
		{
			get => _items;
			set
			{
				_items = value;
				OnPropertyChanged();
			}
		}

		int _position;
		public int Position
		{
			get => _position;
			set
			{
				_position = value;
				OnPropertyChanged();
			}
		}

		public Command ReloadItemsCommand { get; set; }
		public Command GoToLastItemCommand { get; set; }

		public Issue17283ViewModel()
		{
			Items = new ObservableCollection<string> { "1", "2", "3", "4", "5" };
			ReloadItemsCommand = new Command(ReloadItems);
			GoToLastItemCommand = new Command(() => Position = Items.Count - 1);
		}

		async void ReloadItems()
		{
			var currentPosition = Position;
			Items = new ObservableCollection<string> { "1", "2", "3", "4", "5last" };
			await Task.Delay(300);
			Position = currentPosition;
		}
		public event PropertyChangedEventHandler PropertyChanged;
		void OnPropertyChanged([CallerMemberName] string propertyName = "") =>
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}