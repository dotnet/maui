using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AndroidX.Fragment.App;
using AndroidX.ViewPager2.Adapter;

namespace Microsoft.Maui.Controls.Platform
{
	internal class MultiPageFragmentStateAdapter<[DynamicallyAccessedMembers(BindableProperty.DeclaringTypeMembers
#if NET8_0 // IL2091
	| BindableProperty.ReturnTypeMembers
#endif
	)] T> : FragmentStateAdapter where T : Page
	{
		MultiPage<T> _page;
		readonly IMauiContext _context;
		List<AdapterItemKey> keys = new List<AdapterItemKey>();

		public MultiPageFragmentStateAdapter(
			MultiPage<T> page, FragmentManager fragmentManager, IMauiContext context)
			: base(fragmentManager, context.GetActivity().Lifecycle)
		{
			_page = page;
			_context = context;
		}

		public override int ItemCount => CountOverride;

		public int CountOverride { get; set; }

		public override Fragment CreateFragment(int position)
		{
			var fragment = FragmentContainer.CreateInstance(GetItemIdByPosition(position), _context);
			return fragment;
		}

		public override long GetItemId(int position)
		{
			return GetItemIdByPosition(position).ItemId;
		}

		public override bool ContainsItem(long itemId)
		{
			return GetItemByItemId(itemId) != null;
		}

		AdapterItemKey GetItemIdByPosition(int position)
		{
			CheckItemKeys();
			var page = _page.Children[position];
			for (var i = 0; i < keys.Count; i++)
			{
				var item = keys[i];
				if (item.Page == page)
				{
					return item;
				}
			}

			var itemKey = new AdapterItemKey(page, (ik) => keys.Remove(ik));
			keys.Add(itemKey);

			return itemKey;
		}

		AdapterItemKey? GetItemByItemId(long itemId)
		{
			CheckItemKeys();
			for (var i = 0; i < keys.Count; i++)
			{
				var item = keys[i];
				if (item.ItemId == itemId)
				{
					return item;
				}
			}

			return null;
		}

		void CheckItemKeys()
		{
			for (var i = keys.Count - 1; i >= 0; i--)
			{
				var item = keys[i];

				if (!_page.Children.Contains(item.Page))
				{
					// Disconnect will remove the ItemKey from the keys list
					item.Disconnect();
				}
			}
		}
	}
}
