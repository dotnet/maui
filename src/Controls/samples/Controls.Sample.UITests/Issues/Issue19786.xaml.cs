using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 19786, "[Android] Crash removing item from CarouselView", PlatformAffected.All)]
	public partial class Issue19786 : ContentPage
	{
		public Command AddItemCommand { get; set; }
		public Command RemoveItemCommand { get; set; }
		public Command GoToNextItemCommand { get; set; }

		private int _position;
		public int Position
		{
			get => _position;
			set
			{
				_position = value;
				OnPropertyChanged();
			}
		}

		private ObservableCollection<string> _items = new();
		public ObservableCollection<string> Items
		{
			get => _items;
			set
			{
				_items = value;
				OnPropertyChanged();
			}
		}

		public Issue19786()
		{
			InitializeComponent();

			AddItemCommand = new Command(() =>
			{
				Items.Add(Items.Count.ToString());
			});

			RemoveItemCommand = new Command(() =>
			{
				if (Items.Count > 0)
					Items.RemoveAt(Items.Count - 1);
			});

			GoToNextItemCommand = new Command(() =>
			{
				if (Position < Items.Count - 1)
					Position++;
			});

#if !WINDOWS
			BindingContext = this;
#endif
		}
	}
}