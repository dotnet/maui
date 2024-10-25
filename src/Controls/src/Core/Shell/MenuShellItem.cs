#nullable disable
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls.Internals;
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

			this.SetBinding(TitleProperty, static (MenuItem item) => item.Text, BindingMode.OneWay, source: menuItem);
			this.SetBinding(IconProperty, static (MenuItem item) => item.IconImageSource, BindingMode.OneWay, source: menuItem);
			this.SetBinding(FlyoutIconProperty, static (MenuItem item) => item.IconImageSource, BindingMode.OneWay, source: menuItem);
			this.SetBinding(AutomationIdProperty, static (MenuItem item) => item.AutomationId, BindingMode.OneWay, source: menuItem);

			MenuItem.PropertyChanged += OnMenuItemPropertyChanged;
		}

		IList<string> IStyleSelectable.Classes => ((IStyleSelectable)MenuItem).Classes;

		public string Text => Title;

		void OnMenuItemPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == Shell.MenuItemTemplateProperty.PropertyName)
				Shell.SetMenuItemTemplate(this, Shell.GetMenuItemTemplate(MenuItem));
			else if (e.PropertyName == TitleProperty.PropertyName)
				OnPropertyChanged(MenuItem.TextProperty.PropertyName);
			else if (e.PropertyName == Shell.FlyoutItemIsVisibleProperty.PropertyName)
				Shell.SetFlyoutItemIsVisible(this, Shell.GetFlyoutItemIsVisible(MenuItem));
		}

		protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			base.OnPropertyChanged(propertyName);
			if (propertyName == nameof(Title))
				OnPropertyChanged(nameof(Text));
			else if (propertyName == Shell.FlyoutItemIsVisibleProperty.PropertyName && MenuItem != null)
				Shell.SetFlyoutItemIsVisible(MenuItem, Shell.GetFlyoutItemIsVisible(this));
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
