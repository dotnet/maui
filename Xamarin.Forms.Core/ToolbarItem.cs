using System;

namespace Xamarin.Forms
{
	public class ToolbarItem : MenuItem
	{
		static readonly BindableProperty OrderProperty = BindableProperty.Create("Order", typeof(ToolbarItemOrder), typeof(ToolbarItem), ToolbarItemOrder.Default, validateValue: (bo, o) =>
		{
			var order = (ToolbarItemOrder)o;
			return order == ToolbarItemOrder.Default || order == ToolbarItemOrder.Primary || order == ToolbarItemOrder.Secondary;
		});

		static readonly BindableProperty PriorityProperty = BindableProperty.Create("Priority", typeof(int), typeof(ToolbarItem), 0);

		public ToolbarItem()
		{
		}

		public ToolbarItem(string name, string icon, Action activated, ToolbarItemOrder order = ToolbarItemOrder.Default, int priority = 0)
		{
			if (activated == null)
				throw new ArgumentNullException("activated");

			Text = name;
			Icon = icon;
			Clicked += (s, e) => activated();
			Order = order;
			Priority = priority;
		}

		[Obsolete("Now that ToolbarItem is based on MenuItem, .Text has replaced .Name")]
		public string Name
		{
			get { return Text; }
			set { Text = value; }
		}

		public ToolbarItemOrder Order
		{
			get { return (ToolbarItemOrder)GetValue(OrderProperty); }
			set { SetValue(OrderProperty, value); }
		}

		public int Priority
		{
			get { return (int)GetValue(PriorityProperty); }
			set { SetValue(PriorityProperty, value); }
		}

		[Obsolete("Activated has been replaced by the more consistent 'Clicked'")]
		public event EventHandler Activated
		{
			add { Clicked += value; }
			remove { Clicked -= value; }
		}
	}
}