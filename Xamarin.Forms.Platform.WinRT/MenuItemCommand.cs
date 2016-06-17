using System;
using System.ComponentModel;
using System.Windows.Input;

#if WINDOWS_UWP

namespace Xamarin.Forms.Platform.UWP
#else

namespace Xamarin.Forms.Platform.WinRT
#endif
{
	internal class MenuItemCommand : ICommand
	{
		readonly MenuItem _menuItem;
		IMenuItemController Controller => _menuItem;

		public MenuItemCommand(MenuItem item)
		{
			_menuItem = item;
			_menuItem.PropertyChanged += OnElementPropertyChanged;
		}

		public virtual bool CanExecute(object parameter)
		{
			return Controller.IsEnabled;
		}

		public event EventHandler CanExecuteChanged;

		public void Execute(object parameter)
		{
			Controller.Activate();
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