using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 24105, "CarouselView doesn't scroll to the right Position after changing the ItemSource with Loop enabled", PlatformAffected.All)]
	public partial class Issue24105 : ContentPage
	{
		public Issue24105()
		{
			InitializeComponent();
			BindingContext = new Issue24105ViewModel();
		}	
	}

	public class Issue24105ViewModel : INotifyPropertyChanged
	{
		ObservableCollection<string> _items;
		public ObservableCollection<string> Items
		{
			get=>_items;
			set
			{
				_items=value;
				OnPropertyChanged();
			}
		}

		int _position;
		public int Position
		{
			get=>_position;
			set
			{
				_position=value;
				OnPropertyChanged();
			}
		}

		public Command ReloadItemsCommand{get;set;}
		public Command GoToLastItemCommand{get;set;}


		public Issue24105ViewModel()
		{
			Items = new ObservableCollection<string>{ "one", "two", "three", "four", "five" };
			ReloadItemsCommand = new Command(ReloadItems);
			GoToLastItemCommand = new Command(()=> Position = Items.Count-1);
		}

		async void ReloadItems()
		{
			var currentPosition = Position;
			Position = currentPosition;
			Items = new ObservableCollection<string>{ "one", "two", "three", "four", "five2" };
			await Task.Delay(300);
			Position = currentPosition;
		}
		public event PropertyChangedEventHandler PropertyChanged;
		void OnPropertyChanged([CallerMemberName] string propertyName = "") =>
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}