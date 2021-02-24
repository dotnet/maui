using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls.StyleSheets;

namespace Microsoft.Maui.Controls
{
	internal class MenuShellItem : ShellItem, IMenuItemController, IStyleSelectable
	{
		internal MenuShellItem(MenuItem menuItem)
		{
			MenuItem = menuItem;
			MenuItem.Parent = this;
			Shell.SetFlyoutItemIsVisible(this, Shell.GetFlyoutItemIsVisible(menuItem));
			SetBinding(TitleProperty, new Binding(nameof(MenuItem.Text), BindingMode.OneWay, source: menuItem));
			SetBinding(IconProperty, new Binding(nameof(MenuItem.IconImageSource), BindingMode.OneWay, source: menuItem));
			SetBinding(FlyoutIconProperty, new Binding(nameof(MenuItem.IconImageSource), BindingMode.OneWay, source: menuItem));
			SetBinding(AutomationIdProperty, new Binding(nameof(MenuItem.AutomationId), BindingMode.OneWay, source: menuItem));

			MenuItem.PropertyChanged += OnMenuItemPropertyChanged;
		}

		IList<string> IStyleSelectable.Classes => ((IStyleSelectable)MenuItem).Classes;

		public string Text => Title;

		void OnMenuItemPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == Shell.MenuItemTemplateProperty.PropertyName)
				Shell.SetMenuItemTemplate(this, Shell.GetMenuItemTemplate(MenuItem));
			else if (e.PropertyName == TitleProperty.PropertyName)
				OnPropertyChanged(MenuItem.TextProperty.PropertyName);
			else if (e.PropertyName == FlyoutItem.IsVisibleProperty.PropertyName)
				Shell.SetFlyoutItemIsVisible(this, Shell.GetFlyoutItemIsVisible(MenuItem));
		}

		protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			base.OnPropertyChanged(propertyName);
			if (propertyName == nameof(Title))
				OnPropertyChanged(nameof(Text));
		}

		public MenuItem MenuItem { get; }
		bool IMenuItemController.IsEnabled { get => MenuItem.IsEnabled; set => MenuItem.IsEnabled = value; }

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
