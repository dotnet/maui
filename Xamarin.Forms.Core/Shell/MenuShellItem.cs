using System.Runtime.CompilerServices;

namespace Xamarin.Forms
{
	internal class MenuShellItem : ShellItem, IMenuItemController
	{
		internal MenuShellItem(MenuItem menuItem)
		{
			MenuItem = menuItem;

			SetBinding(TitleProperty, new Binding("Text", BindingMode.OneWay, source: menuItem));
			SetBinding(IconProperty, new Binding("Icon", BindingMode.OneWay, source: menuItem));
			SetBinding(FlyoutIconProperty, new Binding("Icon", BindingMode.OneWay, source: menuItem));

			Shell.SetMenuItemTemplate(this, Shell.GetMenuItemTemplate(MenuItem));
			MenuItem.PropertyChanged += OnMenuItemPropertyChanged;
		}

		public string Text => Title;

		void OnMenuItemPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == Shell.MenuItemTemplateProperty.PropertyName)
				Shell.SetMenuItemTemplate(this, Shell.GetMenuItemTemplate(MenuItem));
			else if (e.PropertyName == TitleProperty.PropertyName)
				OnPropertyChanged(MenuItem.TextProperty.PropertyName);
		}

		public MenuItem MenuItem { get; }
		bool IMenuItemController.IsEnabled { get => MenuItem.IsEnabled; set => MenuItem.IsEnabled = value; }

		string IMenuItemController.IsEnabledPropertyName => MenuItem.IsEnabledProperty.PropertyName;

		void IMenuItemController.Activate()
		{
			(MenuItem as IMenuItemController).Activate();
		}

		protected override void OnBindingContextChanged()
		{
			base.OnBindingContextChanged();
			SetInheritedBindingContext(MenuItem, BindingContext);
		}
	}
}
