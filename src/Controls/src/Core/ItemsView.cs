#nullable disable
using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls
{
	/// <summary>A <see cref="Microsoft.Maui.Controls.View"/> that serves as a base class for views that contain a templated list of items.</summary>
	public abstract class ItemsView<[DynamicallyAccessedMembers(BindableProperty.DeclaringTypeMembers | BindableProperty.ReturnTypeMembers)] TVisual> : View, ITemplatedItemsView<TVisual> where TVisual : BindableObject
	{
		/*
		/// <summary>Bindable property for <see cref="InfiniteScrolling"/>.</summary>
		public static readonly BindableProperty InfiniteScrollingProperty =
			BindableProperty.Create<ItemsView, bool> (lv => lv.InfiniteScrolling, false);

		public bool InfiniteScrolling
		{
			get { return (bool) GetValue (InfiniteScrollingProperty); }
			set { SetValue (InfiniteScrollingProperty, value); }
		}*/

		/// <summary>Bindable property for <see cref="ItemsSource"/>.</summary>
		public static readonly BindableProperty ItemsSourceProperty =
			BindableProperty.Create(nameof(ItemsSource), typeof(IEnumerable), typeof(ItemsView<TVisual>), null,
									propertyChanged: OnItemsSourceChanged);

		/// <summary>Bindable property for <see cref="ItemTemplate"/>.</summary>
		public static readonly BindableProperty ItemTemplateProperty =
			BindableProperty.Create(nameof(ItemTemplate), typeof(DataTemplate), typeof(ItemsView<TVisual>), null,
									validateValue: (b, v) => ((ItemsView<TVisual>)b).ValidateItemTemplate((DataTemplate)v));

		internal ItemsView()
			=> TemplatedItems = new TemplatedItemsList<ItemsView<TVisual>, TVisual>(this, ItemsSourceProperty, ItemTemplateProperty);

		/// <include file="../../docs/Microsoft.Maui.Controls/ItemsView.xml" path="//Member[@MemberName='ItemsSource']/Docs/*" />
		public IEnumerable ItemsSource
		{
			get => (IEnumerable)GetValue(ItemsSourceProperty);
			set => SetValue(ItemsSourceProperty, value);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ItemsView.xml" path="//Member[@MemberName='ItemTemplate']/Docs/*" />
		public DataTemplate ItemTemplate
		{
			get => (DataTemplate)GetValue(ItemTemplateProperty);
			set => SetValue(ItemTemplateProperty, value);
		}

		/*public void UpdateNonNotifyingList()
		{
			this.templatedItems.ForceUpdate();
		}*/

		IListProxy ITemplatedItemsView<TVisual>.ListProxy => TemplatedItems.ListProxy;

		ITemplatedItemsList<TVisual> ITemplatedItemsView<TVisual>.TemplatedItems => TemplatedItems;

		[EditorBrowsable(EditorBrowsableState.Never)]
		public TemplatedItemsList<ItemsView<TVisual>, TVisual> TemplatedItems { get; }

		TVisual IItemsView<TVisual>.CreateDefault(object item) => CreateDefault(item);

		void IItemsView<TVisual>.SetupContent(TVisual content, int index) => SetupContent(content, index);

		void IItemsView<TVisual>.UnhookContent(TVisual content) => UnhookContent(content);

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

		protected virtual bool ValidateItemTemplate(DataTemplate template) => true;
	}
}
