using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using WBrush = Microsoft.UI.Xaml.Media.Brush;
using WIconElement = Microsoft.UI.Xaml.Controls.IconElement;

namespace Microsoft.Maui.Platform
{
	internal static class NotifyPropertyChangedExtensions
	{
		public static bool SetProperty<T>(
			this INotifyPropertyChanged _,
			ref T
			backingStore,
			T value,
			Action<PropertyChangedEventArgs> onChanged,
			[CallerMemberName] string propertyName = "")
		{
			if (EqualityComparer<T>.Default.Equals(backingStore, value))
				return false;

			backingStore = value;
			onChanged.Invoke(new PropertyChangedEventArgs(propertyName));
			return true;
		}
	}

	internal static class NavigationViewItemViewModelExtensions
	{
		public static NavigationViewItemViewModel? GetWithData(this IEnumerable<NavigationViewItemViewModel> dest, object data)
		{
			foreach (var item in dest)
			{
				if (item.Data == data)
				{
					return item;
				}
			}

			return null;
		}

		public static bool TryGetWithData(this IEnumerable<NavigationViewItemViewModel> dest, object data, out NavigationViewItemViewModel? vm)
		{
			var result = GetWithData(dest, data);
			vm = result;
			return result != null;
		}

		public static void SyncItems<T>(
			this ObservableCollection<NavigationViewItemViewModel> dest,
			IList<T> source,
			Action<NavigationViewItemViewModel, T> updateItem)
		{
			while (dest.Count < source.Count)
			{
				dest.Add(new NavigationViewItemViewModel());
			}

			while (source.Count < dest.Count)
			{
				dest.RemoveAt(0);
			}

			for (var i = 0; i < source.Count; i++)
			{
				T page = source[i];
				var navItem = dest[i];
				updateItem(navItem, page);
				navItem.Data = page;
			}
		}
	}

	internal class NavigationViewItemViewModel : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler? PropertyChanged;

		object? _content;
		WBrush? _foreground;
		bool _isSelected;
		WBrush? _selectedBackground;
		WBrush? _unselectedBackground;
		ObservableCollection<NavigationViewItemViewModel>? _menuItemsSource;
		WIconElement? _icon;

		public object? Content
		{
			get { return _content; }
			set { this.SetProperty(ref _content, value, OnPropertyChanged); }
		}

		public WIconElement? Icon
		{
			get { return _icon; }
			set { this.SetProperty(ref _icon, value, OnPropertyChanged); }
		}

		public WBrush? Foreground
		{
			get { return _foreground; }
			set { this.SetProperty(ref _foreground, value, OnPropertyChanged); }
		}

		public WBrush? Background
		{
			get => IsSelected ? SelectedBackground : UnselectedBackground;
		}

		public object? Data { get; set; }

		public ObservableCollection<NavigationViewItemViewModel>? MenuItemsSource
		{
			get { return _menuItemsSource; }
			set { this.SetProperty(ref _menuItemsSource, value, OnPropertyChanged); }
		}

		public WBrush? SelectedBackground
		{
			get => _selectedBackground;
			set
			{
				_selectedBackground = value;
				OnPropertyChanged(nameof(Background));
			}
		}

		public WBrush? UnselectedBackground
		{
			get => _unselectedBackground;
			set
			{
				_unselectedBackground = value;
				OnPropertyChanged(nameof(Background));
			}
		}

		public bool IsSelected
		{
			get => _isSelected;
			set
			{
				_isSelected = value;
				OnPropertyChanged(nameof(Background));
			}
		}

		void OnPropertyChanged(string args) =>
			OnPropertyChanged(new PropertyChangedEventArgs(args));

		void OnPropertyChanged(PropertyChangedEventArgs args) =>
			PropertyChanged?.Invoke(this, args);
	}
}
