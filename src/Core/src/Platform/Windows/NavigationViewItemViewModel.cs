using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
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
				dest.RemoveAt(dest.Count - 1);
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
		bool _isSelected;
		WBrush? _selectedBackground;
		WBrush? _unselectedBackground;
		WBrush? _selectedForeground;
		WBrush? _selectedTitleColor;
		WBrush? _unselectedTitleColor;
		WBrush? _unselectedForeground;
		WBrush? _iconColor;
		ObservableCollection<NavigationViewItemViewModel>? _menuItemsSource;
		WIconElement? _icon;
		WeakReference<object>? _data;

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
			get => IconColor ?? (IsSelected ? SelectedForeground : UnselectedForeground);
		}

		public WBrush? Background
		{
			get => (IsSelected ? SelectedBackground : UnselectedBackground) ?? new SolidColorBrush(Microsoft.UI.Colors.Transparent); //The Background color is set to null since both SelectedBackground and UnselectedBackground return null. Adding a default transparent background ensures it is never null, preventing rendering inconsistencies.
		}

		public object? Data
		{
			get => _data?.GetTargetOrDefault();
			set => _data = value is null ? null : new(value);
		}

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

		public WBrush? TitleColor
		{
			get => IsSelected ? SelectedTitleColor : UnselectedTitleColor;
		}

		public WBrush? SelectedTitleColor
		{
			get => _selectedTitleColor;
			set
			{
				_selectedTitleColor = value;
				OnPropertyChanged(nameof(TitleColor));
			}
		}

		public WBrush? UnselectedTitleColor
		{
			get => _unselectedTitleColor;
			set
			{
				_unselectedTitleColor = value;
				OnPropertyChanged(nameof(TitleColor));
			}
		}

		public WBrush? IconColor
		{
			get => _iconColor;
			set
			{
				_iconColor = value;
				OnPropertyChanged(nameof(IconColor));
				UpdateForeground();
			}
		}

		public WBrush? SelectedForeground
		{
			get => _selectedForeground;
			set
			{
				_selectedForeground = value;
				UpdateForeground();
			}
		}

		public WBrush? UnselectedForeground
		{
			get => _unselectedForeground;
			set
			{
				_unselectedForeground = value;
				UpdateForeground();
			}
		}

		void UpdateForeground()
		{
			OnPropertyChanged(nameof(Foreground));

			if (Icon is WIconElement bi)
			{
				if (Foreground is null)
				{
					bi.ClearValue(WIconElement.ForegroundProperty);
				}
				else
				{
					bi.Foreground = Foreground;
				}
			}
		}

		public bool IsSelected
		{
			get => _isSelected;
			set
			{
				_isSelected = value;
				OnPropertyChanged(nameof(Background));
				OnPropertyChanged(nameof(TitleColor));
				UpdateForeground();
			}
		}

		void OnPropertyChanged(string args) =>
			OnPropertyChanged(new PropertyChangedEventArgs(args));

		void OnPropertyChanged(PropertyChangedEventArgs args) =>
			PropertyChanged?.Invoke(this, args);
	}
}
