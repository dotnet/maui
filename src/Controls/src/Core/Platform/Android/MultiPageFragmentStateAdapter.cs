using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AndroidX.Fragment.App;
using AndroidX.ViewPager2.Adapter;
using AView = Android.Views.View;

namespace Microsoft.Maui.Controls.Platform
{
	internal class MultiPageFragmentStateAdapter<[DynamicallyAccessedMembers(BindableProperty.DeclaringTypeMembers)] T> : FragmentStateAdapter where T : Page
	{
		MultiPage<T> _page;
		readonly IMauiContext _context;
		List<ItemKey> keys = new List<ItemKey>();

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
			var fragment = FragmentContainer.CreateInstance(_page.Children[position], _context);
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

		ItemKey GetItemIdByPosition(int position)
		{
			CheckItemKeys();
			var page = _page.Children[position];
			for (var i = 0; i < keys.Count; i++)
			{
				ItemKey item = keys[i];
				if (item.Page == page)
				{
					return item;
				}
			}

			ItemKey itemKey = new ItemKey(page, (ik) => keys.Remove(ik));
			keys.Add(itemKey);

			return itemKey;
		}

		ItemKey? GetItemByItemId(long itemId)
		{
			CheckItemKeys();
			for (var i = 0; i < keys.Count; i++)
			{
				ItemKey item = keys[i];
				if (item.ItemId == itemId)
				{
					return item;
				}
			}

			return null;
		}

		void CheckItemKeys()
		{
			for (var i = 0; i < keys.Count; i++)
			{
				ItemKey item = keys[i];

				if (!_page.Children.Contains(item.Page))
				{
					// Disconnect will remove the ItemKey from the keys list
					item.Disconnect();
				}
			}
		}

		class ItemKey
		{
			Page? _page;
			Action<ItemKey>? _markInvalid;
			bool _stableView;
			object? _platformView;

			public ItemKey(Page page, Action<ItemKey> markInvalid)
			{
				_page = page;
				_markInvalid = markInvalid;
				_page.HandlerChanging += OnHandlerChanging;
				_page.HandlerChanged += OnHandlerChanged;
				ItemId = AView.GenerateViewId();
			}

			public bool Disconnected => _page != null;
			public Page? Page => _page;
			public long ItemId { get; }
			public void Disconnect()
			{
				if (_page == null)
					return;

				_markInvalid?.Invoke(this);

				if (_page != null)
				{
					_page.HandlerChanging -= OnHandlerChanging;
					_page.HandlerChanged -= OnHandlerChanged;
					_page = null;
				}

				_platformView = null;
			}

			public void SetToStableView()
			{
				_stableView = true;
			}

			void OnHandlerChanging(object? sender, HandlerChangingEventArgs e)
			{
				if (_stableView && _platformView != null)
					Disconnect();
			}

			void OnHandlerChanged(object? sender, EventArgs e)
			{
				if (_page == null || _platformView != null)
				{
					if (sender is Page page)
						page.HandlerChanged -= OnHandlerChanged;

					return;
				}

				_platformView = _page.Handler?.PlatformView;

				if (_platformView != null)
					_page.HandlerChanged -= OnHandlerChanged;
			}

		}
	}
}