#nullable disable
using System;

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
	}
}
