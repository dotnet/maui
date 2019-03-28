using System;
using Android.Content;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.Android
{
	internal class TemplatedItemViewHolder : SelectableViewHolder
	{
		private readonly ItemContentView _itemContentView;
		private readonly DataTemplate _template;

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
			itemsView.RemoveLogicalChild(View);
			_itemContentView.Recycle();
		}

		public void Bind(object itemBindingContext, ItemsView itemsView)
		{
			var template = _template.SelectDataTemplate(itemBindingContext, itemsView);

			View = (View)template.CreateContent();
			_itemContentView.RealizeContent(View);

			// Set the binding context before we add it as a child of the ItemsView; otherwise, it will
			// inherit the ItemsView's binding context
			View.BindingContext = itemBindingContext;

			itemsView.AddLogicalChild(View);
		}
	}
}