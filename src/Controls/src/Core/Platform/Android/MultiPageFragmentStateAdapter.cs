using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AndroidX.Fragment.App;
using AndroidX.ViewPager2.Adapter;

namespace Microsoft.Maui.Controls.Platform
{
	internal class MultiPageFragmentStateAdapter<[DynamicallyAccessedMembers(BindableProperty.DeclaringTypeMembers | BindableProperty.ReturnTypeMembers)] T> : FragmentStateAdapter where T : Page
	{
		MultiPage<T> _page;
		readonly IMauiContext _context;
		bool _disposed;
		bool _tearingDown;
		List<AdapterItemKey> keys = new List<AdapterItemKey>();

		public MultiPageFragmentStateAdapter(
			MultiPage<T> page, FragmentManager fragmentManager, IMauiContext context)
			: base(fragmentManager, context.GetActivity().Lifecycle)
		{
			_page = page;
			_context = context;
		}

		protected override void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				_disposed = true;
				if (disposing)
				{
					_page = null!;
				}
			}

			base.Dispose(disposing);
		}

		public override int ItemCount => CountOverride;

		public int CountOverride { get; set; }

		public override Fragment CreateFragment(int position)
		{
			// Guard against a pending RecyclerView layout pass calling CreateFragment after
			// BeginTeardown() reported zero items but before that update has been processed,
			// which would otherwise dereference a disposed/null _page.
			if (_tearingDown || _page is null)
			{
				return new Fragment();
			}

			return FragmentContainer.CreateInstance(GetItemIdByPosition(position), _context);
		}

		public override long GetItemId(int position)
		{
			if (_tearingDown || _page is null)
			{
				// -1 matches AndroidX RecyclerView.NO_ID.
				return -1L;
			}

			return GetItemIdByPosition(position).ItemId;
		}

		public override bool ContainsItem(long itemId)
		{
			if (_tearingDown)
			{
				return false;
			}

			return GetItemByItemId(itemId) != null;
		}

		internal void BeginTeardown()
		{
			_tearingDown = true;
			CountOverride = 0;
			keys.Clear();
			NotifyDataSetChanged();
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
