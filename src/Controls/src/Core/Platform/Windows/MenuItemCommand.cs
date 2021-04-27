using System;
using System.ComponentModel;
using System.Windows.Input;

namespace Microsoft.Maui.Controls.Platform
{
	internal class MenuItemCommand : ICommand
	{
		readonly MenuItem _menuItem;

		public MenuItemCommand(MenuItem item)
		{
			_menuItem = item;
			_menuItem.PropertyChanged += OnElementPropertyChanged;
		}

		public virtual bool CanExecute(object parameter)
		{
			return _menuItem.IsEnabled;
		}

		public event EventHandler CanExecuteChanged;

		public void Execute(object parameter)
		{
			((IMenuItemController)_menuItem).Activate();
		}

		void OnCanExecuteChanged()
		{
			EventHandler changed = CanExecuteChanged;
			if (changed != null)
				changed(this, EventArgs.Empty);
		}

		void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == VisualElement.IsEnabledProperty.PropertyName)
				OnCanExecuteChanged();
		}
	}
}