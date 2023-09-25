using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Issues
{
	public class Issue17610Item
	{
		public string Name { get; set; }
		public Color Color { get; set; }
	}

	public class Issue17610ViewModel : INotifyPropertyChanged
	{
		const int RefreshDuration = 2;

		int _itemNumber = 1;
		readonly Random _random;
		bool _isRefreshing;

		public bool IsRefreshing
		{
			get { return _isRefreshing; }
			set
			{
				_isRefreshing = value;
				OnPropertyChanged();
			}
		}

		public ObservableCollection<Issue17610Item> Items { get; private set; }

		public ICommand RefreshCommand => new Command(async () => await RefreshItemsAsync());

		public Issue17610ViewModel()
		{
			_random = new Random();
			Items = new ObservableCollection<Issue17610Item>();

			AddItems();
			FireAndForget(RefreshItemsAsync());
		}

		void AddItems()
		{
			for (int i = 0; i < 50; i++)
			{
				Items.Add(new Issue17610Item
				{
					Color = Color.FromRgb(_random.Next(0, 255), _random.Next(0, 255), _random.Next(0, 255)),
					Name = $"Item {_itemNumber++}"
				});
			}
		}

		async Task RefreshItemsAsync()
		{
			IsRefreshing = true;
			await Task.Delay(TimeSpan.FromSeconds(RefreshDuration));
			AddItems();
			IsRefreshing = false;
		}

		async void FireAndForget(Task task, Action<Exception> errorCallback = null)
		{
			try
			{
				await task.ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				errorCallback?.Invoke(ex);
#if DEBUG
				throw;
#endif
			}
		}

		#region INotifyPropertyChanged

		public event PropertyChangedEventHandler PropertyChanged;

		void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		#endregion
	}

	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 17610, "RefreshView indicator hidden behind Navigation bar", PlatformAffected.Android)]
	public partial class Issue17610 : ContentPage
	{
		public Issue17610()
		{
			InitializeComponent();

			BindingContext = new Issue17610ViewModel();
		}
	}
}