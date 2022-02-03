using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.UI.Xaml.Controls;
using WBrush = Microsoft.UI.Xaml.Media.Brush;

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

	internal class NavigationViewItemViewModel : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler? PropertyChanged;

		object? _content;
		WBrush? _foreground;
		private bool _isSelected;
		private WBrush? _selectedBackground;
		private WBrush? _unselectedBackground;

		public object? Content
		{
			get { return _content; }
			set { this.SetProperty(ref _content, value, OnPropertyChanged); }
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
