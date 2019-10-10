using System;
using System.ComponentModel;

namespace Xamarin.Forms.Platform.iOS
{
	public class SelectableItemsViewRenderer : StructuredItemsViewRenderer
	{
		SelectableItemsView SelectableItemsView => (SelectableItemsView)Element;
		SelectableItemsViewController SelectableItemsViewController => (SelectableItemsViewController)ItemsViewController;

		protected override ItemsViewController CreateController(ItemsView itemsView, ItemsViewLayout layout)
		{
			return new SelectableItemsViewController(itemsView as SelectableItemsView, layout);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs changedProperty)
		{
			base.OnElementPropertyChanged(sender, changedProperty);

			if (changedProperty.IsOneOf(SelectableItemsView.SelectedItemProperty, SelectableItemsView.SelectedItemsProperty))
			{
				UpdateNativeSelection();
			}
			else if (changedProperty.Is(SelectableItemsView.SelectionModeProperty))
			{
				UpdateSelectionMode();
			}
		}

		protected override void SetUpNewElement(ItemsView newElement)
		{
			if (newElement != null && !(newElement is SelectableItemsView))
			{
				throw new ArgumentException($"{nameof(newElement)} must be of type {typeof(SelectableItemsView).Name}");
			}

			base.SetUpNewElement(newElement);

			if (newElement == null)
			{
				return;
			}

			UpdateSelectionMode();
			UpdateNativeSelection();
		}

		protected virtual void UpdateNativeSelection()
		{
			SelectableItemsViewController.UpdateNativeSelection();
		}

		protected virtual void UpdateSelectionMode()
		{
			SelectableItemsViewController.UpdateSelectionMode();
		}

		protected override void UpdateItemsSource()
		{
			base.UpdateItemsSource();
			UpdateNativeSelection();
		}
	}
}