using System;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.Android
{
	internal class TemplatedItemViewHolder : SelectableViewHolder
	{
		readonly ItemContentView _itemContentView;
		readonly DataTemplate _template;
		DataTemplate _selectedTemplate;

		public View View { get; private set; }

		public TemplatedItemViewHolder(ItemContentView itemContentView, DataTemplate template) : base(itemContentView)
		{
			_itemContentView = itemContentView;
			_template = template;
		}

		protected override void OnSelectedChanged()
		{
			base.OnSelectedChanged();

			if (View == null)
			{
				return;
			}

			VisualStateManager.GoToState(View, IsSelected 
				? VisualStateManager.CommonStates.Selected 
				: VisualStateManager.CommonStates.Normal);
		}

		public void Recycle(ItemsView itemsView)
		{
			View.BindingContext = null;
			itemsView.RemoveLogicalChild(View);
		}

		public void Bind(object itemBindingContext, ItemsView itemsView)
		{
			var template = _template.SelectDataTemplate(itemBindingContext, itemsView);

			if(template != _selectedTemplate)
			{
				_itemContentView.Recycle();
				View = (View)template.CreateContent();
				_itemContentView.RealizeContent(View);
				_selectedTemplate = template;
			}

			// Set the binding context before we add it as a child of the ItemsView; otherwise, it will
			// inherit the ItemsView's binding context
			View.BindingContext = itemBindingContext;

			itemsView.AddLogicalChild(View);
		}
	}
}