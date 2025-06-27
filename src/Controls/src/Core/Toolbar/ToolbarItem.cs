#nullable disable
using System;
using System.Collections.Generic;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/ToolbarItem.xml" path="Type[@FullName='Microsoft.Maui.Controls.ToolbarItem']/Docs/*" />
	public class ToolbarItem : MenuItem
	{
		static readonly BindableProperty OrderProperty = BindableProperty.Create(nameof(Order), typeof(ToolbarItemOrder), typeof(ToolbarItem), ToolbarItemOrder.Default, validateValue: (bo, o) =>
		{
			var order = (ToolbarItemOrder)o;
			return order == ToolbarItemOrder.Default || order == ToolbarItemOrder.Primary || order == ToolbarItemOrder.Secondary;
		});

		static readonly BindableProperty PriorityProperty = BindableProperty.Create(nameof(Priority), typeof(int), typeof(ToolbarItem), 0);

		/// <summary>Bindable property for <see cref="IsVisible"/>.</summary>
		public static readonly BindableProperty IsVisibleProperty = BindableProperty.Create(nameof(IsVisible), typeof(bool), typeof(ToolbarItem), true, propertyChanged: OnIsVisiblePropertyChanged);

		/// <summary>
		/// Internal bindable property to store the page that contains this toolbar item.
		/// This is needed because when IsVisible is false, the item is removed from the collection and Parent becomes null.
		/// </summary>
		static readonly BindableProperty ParentPageProperty = BindableProperty.CreateAttached("ParentPage", typeof(WeakReference<Page>), typeof(ToolbarItem), null);

		/// <include file="../../docs/Microsoft.Maui.Controls/ToolbarItem.xml" path="//Member[@MemberName='.ctor'][1]/Docs/*" />
		public ToolbarItem()
		{
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ToolbarItem.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public ToolbarItem(string name, string icon, Action activated, ToolbarItemOrder order = ToolbarItemOrder.Default, int priority = 0)
		{
			if (activated == null)
				throw new ArgumentNullException(nameof(activated));

			Text = name;
			IconImageSource = icon;
			Clicked += (s, e) => activated();
			Order = order;
			Priority = priority;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ToolbarItem.xml" path="//Member[@MemberName='Order']/Docs/*" />
		public ToolbarItemOrder Order
		{
			get { return (ToolbarItemOrder)GetValue(OrderProperty); }
			set { SetValue(OrderProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ToolbarItem.xml" path="//Member[@MemberName='Priority']/Docs/*" />
		public int Priority
		{
			get { return (int)GetValue(PriorityProperty); }
			set { SetValue(PriorityProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ToolbarItem.xml" path="//Member[@MemberName='IsVisible']/Docs/*" />
		public bool IsVisible
		{
			get { return (bool)GetValue(IsVisibleProperty); }
			set { SetValue(IsVisibleProperty, value); }
		}

		static void OnIsVisiblePropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			if (bindable is not ToolbarItem item)
				return;

			bool setValue = (bool)newValue;
			Page page = null;

			// First try to get the page from the current Parent
			if (item.Parent is Page parentPage)
			{
				page = parentPage;
				// Store a weak reference to the page for later use
				item.SetValue(ParentPageProperty, new WeakReference<Page>(page));
			}
			else
			{
				// If Parent is null (because item was removed from collection), try to get it from stored reference
				if (item.GetValue(ParentPageProperty) is WeakReference<Page> weakRef && weakRef.TryGetTarget(out var storedPage))
				{
					page = storedPage;
				}
			}

			if (page == null)
				return;

			var items = page.ToolbarItems;
			if (items == null)
				return;

			if (setValue && !items.Contains(item))
			{
				// Add item back to toolbar items, preserving original order based on Priority
				int index = items.Count;
				for (int i = 0; i < items.Count; i++)
				{
					if (items[i].Priority > item.Priority)
					{
						index = i;
						break;
					}
				}
				items.Insert(index, item);
			}
			else if (!setValue && items.Contains(item))
			{
				items.Remove(item);
			}
		}
	}
}
