using System.Collections;
using System.ComponentModel;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms
{

	public abstract class ItemsView<TVisual> : View, ITemplatedItemsView<TVisual> where TVisual : BindableObject
	{
		/*
		public static readonly BindableProperty InfiniteScrollingProperty =
			BindableProperty.Create<ItemsView, bool> (lv => lv.InfiniteScrolling, false);

		public bool InfiniteScrolling
		{
			get { return (bool) GetValue (InfiniteScrollingProperty); }
			set { SetValue (InfiniteScrollingProperty, value); }
		}*/

		public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create("ItemsSource", typeof(IEnumerable), typeof(ItemsView<TVisual>), null, propertyChanged: OnItemsSourceChanged);

		public static readonly BindableProperty ItemTemplateProperty = BindableProperty.Create("ItemTemplate", typeof(DataTemplate), typeof(ItemsView<TVisual>), null, validateValue: ValidateItemTemplate);

		internal ItemsView()
		{
			TemplatedItems = new TemplatedItemsList<ItemsView<TVisual>, TVisual>(this, ItemsSourceProperty, ItemTemplateProperty);
		}

		public IEnumerable ItemsSource
		{
			get { return (IEnumerable)GetValue(ItemsSourceProperty); }
			set { SetValue(ItemsSourceProperty, value); }
		}

		public DataTemplate ItemTemplate
		{
			get { return (DataTemplate)GetValue(ItemTemplateProperty); }
			set { SetValue(ItemTemplateProperty, value); }
		}

		/*public void UpdateNonNotifyingList()
		{
			this.templatedItems.ForceUpdate();
		}*/

		IListProxy ITemplatedItemsView<TVisual>.ListProxy
		{
			get { return TemplatedItems.ListProxy; }
		}

		ITemplatedItemsList<TVisual> ITemplatedItemsView<TVisual>.TemplatedItems { get { return TemplatedItems; } }

		[EditorBrowsable(EditorBrowsableState.Never)]
		public TemplatedItemsList<ItemsView<TVisual>, TVisual> TemplatedItems { get; }

		TVisual IItemsView<TVisual>.CreateDefault(object item)
		{
			return CreateDefault(item);
		}

		void IItemsView<TVisual>.SetupContent(TVisual content, int index)
		{
			SetupContent(content, index);
		}

		void IItemsView<TVisual>.UnhookContent(TVisual content)
		{
			UnhookContent(content);
		}

		protected abstract TVisual CreateDefault(object item);

		protected virtual void SetupContent(TVisual content, int index)
		{
		}

		protected virtual void UnhookContent(TVisual content)
		{
		}

		static void OnItemsSourceChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var element = newValue as Element;
			if (element == null)
				return;
			element.Parent = (Element)bindable;
		}

		static bool ValidateItemTemplate(BindableObject bindable, object value)
		{
			var listView = bindable as ListView;
			if (listView == null)
				return true;

			var isRetainStrategy = listView.CachingStrategy == ListViewCachingStrategy.RetainElement;
			var isDataTemplateSelector = listView.ItemTemplate is DataTemplateSelector;
			if (isRetainStrategy && isDataTemplateSelector)
				return false;

			return true;
		}
	}
}