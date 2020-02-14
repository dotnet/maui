using System.ComponentModel;

namespace Xamarin.Forms.Platform.iOS
{
	public class StructuredItemsViewRenderer<TItemsView, TViewController> : ItemsViewRenderer<TItemsView, TViewController>
		where TItemsView : StructuredItemsView
		where TViewController : StructuredItemsViewController<TItemsView>
	{
		[Internals.Preserve(Conditional = true)]
		public StructuredItemsViewRenderer() { }

		protected override TViewController CreateController(TItemsView itemsView, ItemsViewLayout layout)
		{
			return new StructuredItemsViewController<TItemsView>(itemsView, layout) as TViewController;
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs changedProperty)
		{
			base.OnElementPropertyChanged(sender, changedProperty);

			if (changedProperty.IsOneOf(StructuredItemsView.HeaderProperty, StructuredItemsView.HeaderTemplateProperty))
			{
				UpdateHeaderView();
			}
			else if (changedProperty.IsOneOf(StructuredItemsView.FooterProperty, StructuredItemsView.FooterTemplateProperty))
			{
				UpdateFooterView();
			}
			else if (changedProperty.Is(StructuredItemsView.ItemsLayoutProperty))
			{
				UpdateLayout();
			}
			else if (changedProperty.Is(StructuredItemsView.ItemSizingStrategyProperty))
			{
				UpdateItemSizingStrategy();
			}
		}

		protected override void SetUpNewElement(TItemsView newElement)
		{
			base.SetUpNewElement(newElement);

			if (newElement == null)
			{
				return;
			}

			Controller.UpdateFooterView();
			Controller.UpdateHeaderView();
		}

		protected override ItemsViewLayout SelectLayout()
		{
			var itemSizingStrategy = ItemsView.ItemSizingStrategy;
			var itemsLayout = ItemsView.ItemsLayout;

			if (itemsLayout is GridItemsLayout gridItemsLayout)
			{
				return new GridViewLayout(gridItemsLayout, itemSizingStrategy);
			}

			if (itemsLayout is LinearItemsLayout listItemsLayout)
			{
				return new ListViewLayout(listItemsLayout, itemSizingStrategy);
			}

			// Fall back to vertical list
			return new ListViewLayout(new LinearItemsLayout(ItemsLayoutOrientation.Vertical), itemSizingStrategy);
		}

		protected virtual void UpdateHeaderView()
		{
			Controller.UpdateHeaderView();
		}

		protected virtual void UpdateFooterView()
		{
			Controller.UpdateFooterView();
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();
			Controller.UpdateLayoutMeasurements();
		}
	}
}