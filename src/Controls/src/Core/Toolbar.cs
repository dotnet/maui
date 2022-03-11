using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Maui.Graphics;
using System.ComponentModel;

namespace Microsoft.Maui.Controls
{
	public partial class Toolbar : Maui.IToolbar, INotifyPropertyChanged
	{
		VisualElement _titleView;
		string _title;
		Color _iconColor;
		Color _barTextColor;
		Brush _barBackground;
		Color _barBackgroundColor;
		ImageSource _titleIcon;
		string _backButtonTitle;
		double? _barHeight;
		IEnumerable<ToolbarItem> _toolbarItems;
		bool _dynamicOverflowEnabled;
		bool _isVisible = false;
		bool _backButtonVisible;
		bool _drawerToggleVisible;

		public Toolbar(Maui.IElement parent)
		{
			_parent = parent;
		}

		public IEnumerable<ToolbarItem> ToolbarItems { get => _toolbarItems; set => SetProperty(ref _toolbarItems, value); }
		public double? BarHeight { get => _barHeight; set => SetProperty(ref _barHeight, value); }
		public string BackButtonTitle { get => _backButtonTitle; set => SetProperty(ref _backButtonTitle, value); }
		public ImageSource TitleIcon { get => _titleIcon; set => SetProperty(ref _titleIcon, value); }
		public Color BarBackgroundColor { get => _barBackgroundColor; set => SetProperty(ref _barBackgroundColor, value); }
		public Brush BarBackground { get => _barBackground; set => SetProperty(ref _barBackground, value); }
		public virtual Color BarTextColor { get => _barTextColor; set => SetProperty(ref _barTextColor, value); }
		public virtual Color IconColor { get => _iconColor; set => SetProperty(ref _iconColor, value); }
		public virtual string Title { get => _title; set => SetProperty(ref _title, value); }
		public virtual VisualElement TitleView { get => _titleView; set => SetProperty(ref _titleView, value); }
		public bool DynamicOverflowEnabled { get => _dynamicOverflowEnabled; set => SetProperty(ref _dynamicOverflowEnabled, value); }
		public bool BackButtonVisible { get => _backButtonVisible; set => SetProperty(ref _backButtonVisible, value); }
		public virtual bool DrawerToggleVisible { get => _drawerToggleVisible; set => SetProperty(ref _drawerToggleVisible, value); }
		public bool IsVisible { get => _isVisible; set => SetProperty(ref _isVisible, value); }
		public IElementHandler Handler { get; set; }

		Maui.IElement _parent;

		public event PropertyChangedEventHandler PropertyChanged;

		public Maui.IElement Parent => _parent;

		private protected void SetProperty<T>(ref T backingStore, T value,
			[CallerMemberName] string propertyName = "")
		{
			if (EqualityComparer<T>.Default.Equals(backingStore, value))
				return;

			backingStore = value;
			Handler?.UpdateValue(propertyName);
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
