using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Controls.Sample.Models;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.ViewModels
{
	public class RefreshViewModel : INotifyPropertyChanged
	{
		const int RefreshDuration = 2;

		int _itemNumber = 1;
		readonly Random _random;
		bool _isRefreshing;
		bool _isEnabled;

		public bool IsRefreshing
		{
			get { return _isRefreshing; }
			set
			{
				_isRefreshing = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(RefreshText));
			}
		}

		public bool IsEnabled
		{
			get { return _isEnabled; }
			set
			{
				_isEnabled = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(EnabledText));
			}
		}

		public ObservableCollection<RefreshItem> Items { get; private set; }

		public Command RefreshCommand => new Command(async () => await RefreshItemsAsync(), () => !IsRefreshing);

		public RefreshViewModel()
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
					Color = Color.FromRgb(_random.NextDouble(), _random.NextDouble(), _random.NextDouble()),
					Name = $"Item {_itemNumber++}"
				});
			}
		}

		async Task RefreshItemsAsync()
		{
			RefreshCommand.ChangeCanExecute();
			await Task.Delay(TimeSpan.FromSeconds(RefreshDuration));
			AddItems();
			IsRefreshing = false;
			RefreshCommand.ChangeCanExecute();
		}

		public string RefreshText => $"Is Refreshing: {IsRefreshing}";
		public string EnabledText => $"Is Enabled: {IsEnabled}";

		#region INotifyPropertyChanged

		public event PropertyChangedEventHandler? PropertyChanged;

		void OnPropertyChanged([CallerMemberName] string propertyName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		#endregion
	}
}