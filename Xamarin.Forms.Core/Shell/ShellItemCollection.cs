using System.Collections;
using System.Collections.Specialized;

namespace Xamarin.Forms
{
	internal sealed class ShellItemCollection : ShellElementCollection<ShellItem>
	{
		public ShellItemCollection() : base() { }

		public override void Add(ShellItem item)
		{
			/*
			 * This is purely for the case where a user is only specifying Tabs at the highest level
			 * <shell>
			 * <tab></tab>
			 * <tab></tab>
			 * </shell>
			 * */
			if (Routing.IsImplicit(item) &&
				item is TabBar
				)
			{
				int i = Count - 1;
				if (i >= 0 && this[i] is TabBar && Routing.IsImplicit(this[i]))
				{
					(this[i] as ShellItem).Items.Add(item.Items[0]);
					return;
				}
			}

			Inner.Add(item);
		}
	}
}