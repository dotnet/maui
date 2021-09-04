using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	internal class GroupTemplateContext
	{
		public ItemTemplateContext HeaderItemTemplateContext { get; }
		public ItemTemplateContext FooterItemTemplateContext { get; }
		public object Items { get; }

		public GroupTemplateContext(ItemTemplateContext headerItemTemplateContext, 
			ItemTemplateContext footerItemTemplateContext, object items)
		{
			HeaderItemTemplateContext = headerItemTemplateContext;
			FooterItemTemplateContext = footerItemTemplateContext;

			if (footerItemTemplateContext == null)
			{
				Items = items;
			}
			else
			{
				// UWP ListViewBase does not support group footers. So we're going to fake the footer by adding an 
				// extra item to the ItemsSource so the footer shows up at the end of the group. 

				if (items is IList itemsList)
				{
					// If it's already an IList, we want to make sure to keep it that way
					itemsList.Add(footerItemTemplateContext);
					Items = itemsList;
					return;
				}

				// If the group items are not an IList, then we'll have to append the footer the hard way

				var listPlusFooter = new List<object>();

				foreach (var item in (items as IEnumerable))
				{
					listPlusFooter.Add(item);
				}

				listPlusFooter.Add(footerItemTemplateContext);

				Items = listPlusFooter;
			}
		}
	}
}