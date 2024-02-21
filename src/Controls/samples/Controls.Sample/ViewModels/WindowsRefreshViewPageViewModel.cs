using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Controls.Sample.Models;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.ViewModels
{
	public class WindowsRefreshViewPageViewModel : INotifyPropertyChanged
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

		public ObservableCollection<RefreshItem> Items { get; private set; }

		public ICommand RefreshCommand => new Command(async () => await RefreshItemsAsync());

		public WindowsRefreshViewPageViewModel()
		{
			_random = new Random();
			Items = new ObservableCollection<RefreshItem>();
			AddItems();
		}

		void AddItems()
		{
			for (int i = 0; i < 50; i++)
			{
				Items.Add(new RefreshItem
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

		#region INotifyPropertyChanged

		public event PropertyChangedEventHandler? PropertyChanged;

		void OnPropertyChanged([CallerMemberName] string propertyName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		#endregion
	}
}
