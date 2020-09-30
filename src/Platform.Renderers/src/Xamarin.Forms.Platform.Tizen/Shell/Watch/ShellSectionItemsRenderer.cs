using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using ElmSharp;
using ElmSharp.Wearable;
using ERect = ElmSharp.Rect;

namespace Xamarin.Forms.Platform.Tizen.Watch
{
	public class ShellSectionItemsRenderer : IShellItemRenderer
	{
		const int ItemMaxCount = 20;
		const int OddMiddleItem = 10;
		const int EvenMiddleItem = 11;

		Box _mainBox;
		Index _indexIndicator;
		Scroller _scroller;
		Box _innerContainer;
		List<ItemHolder> _items = new List<ItemHolder>();

		int _currentIndex = -1;
		ERect _lastLayoutBound;
		int _updateByCode;
		bool _isScrolling;

		public ShellSectionItemsRenderer(ShellSection shellSection)
		{
			ShellSection = shellSection;
			ShellSection.PropertyChanged += OnSectionPropertyChanged;
			(ShellSection.Items as INotifyCollectionChanged).CollectionChanged += OnItemsChanged;
			InitializeComponent();
			UpdateItems();
		}

		public ShellSection ShellSection { get; protected set; }

		public BaseShellItem Item => ShellSection;

		public EvasObject NativeView => _mainBox;

		public void Dispose()
		{
			Dispose(true);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				_mainBox?.Unrealize();
				(ShellSection.Items as INotifyCollectionChanged).CollectionChanged -= OnItemsChanged;
				ShellSection.PropertyChanged -= OnSectionPropertyChanged;
			}
		}

		void InitializeComponent()
		{
			_mainBox = new Box(Forms.NativeParent)
			{
				AlignmentX = -1,
				AlignmentY = -1,
				WeightX = 1,
				WeightY = 1,
			};
			_mainBox.Show();
			_mainBox.SetLayoutCallback(OnLayout);

			_indexIndicator = new Index(_mainBox)
			{
				IsHorizontal = true,
				AutoHide = false,
			}.SetStyledIndex();
			_indexIndicator.Show();

			_scroller = new Scroller(_mainBox);
			_scroller.Scrolled += OnScrolled;
			_scroller.PageScrolled += OnScrollStop;

			//PageScrolled event is not invoked when a user scrolls beyond the end using bezel
			var scrollAnimationStop = new SmartEvent(_scroller, ThemeConstants.Scroller.Signals.StopScrollAnimation);
			scrollAnimationStop.On += OnScrollStop;

			_scroller.Focused += OnFocused;
			_scroller.Unfocused += OnUnfocused;

			// Disables the visibility of the scrollbar in both directions:
			_scroller.HorizontalScrollBarVisiblePolicy = ScrollBarVisiblePolicy.Invisible;
			_scroller.VerticalScrollBarVisiblePolicy = ScrollBarVisiblePolicy.Invisible;

			// Sets the limit of scroll to one page maximum:
			_scroller.HorizontalPageScrollLimit = 1;
			_scroller.SetPageSize(1.0, 1.0);
			_scroller.SetAlignment(-1, -1);
			_scroller.SetWeight(1.0, 1.0);
			_scroller.Show();

			_innerContainer = new Box(_mainBox);
			_innerContainer.SetLayoutCallback(OnInnerLayoutUpdate);
			_innerContainer.SetAlignment(-1, -1);
			_innerContainer.SetWeight(1.0, 1.0);
			_innerContainer.Show();
			_scroller.SetContent(_innerContainer);

			_mainBox.PackEnd(_indexIndicator);
			_mainBox.PackEnd(_scroller);
			_indexIndicator.StackAbove(_scroller);
		}

		void UpdateItems()
		{
			_items.Clear();
			_indexIndicator.Clear();
			_innerContainer.UnPackAll();
			_lastLayoutBound = default(ERect);

			foreach (var item in ShellSection.Items)
			{
				var indexItem = _indexIndicator.Append(null);
				indexItem.SetIndexItemStyle(ShellSection.Items.Count, _items.Count, EvenMiddleItem, OddMiddleItem);
				_items.Add(new ItemHolder
				{
					IsRealized = false,
					IndexItem = indexItem,
					Item = item
				});
			}
			_indexIndicator.Update(0);
			UpdateCurrentPage(ShellSection.Items.IndexOf(ShellSection.CurrentItem));
		}

		void RealizeItem(int index)
		{
			if (index < 0 || _items.Count <= index)
				return;

			if (!_items[index].IsRealized)
				RealizeItem(_items[index]);
		}

		void RealizeItem(ItemHolder item)
		{
			var renderer = ShellRendererFactory.Default.CreateItemRenderer(item.Item);
			renderer.NativeView.Show();

			item.NativeView = renderer.NativeView;
			item.IsRealized = true;
			_innerContainer.PackEnd(item.NativeView);
			item.NativeView.StackBelow(_indexIndicator);
			item.NativeView.Geometry = item.Bound;
		}

		void UpdateCurrentPage(int index)
		{
			RealizeItem(index - 1);
			RealizeItem(index);
			RealizeItem(index + 1);

			UpdateCurrentIndex(index);
			UpdateFocusPolicy();
		}

		void UpdateFocusPolicy()
		{
			foreach (var item in _items)
			{
				if (item.IsRealized)
				{
					if (item.NativeView is ElmSharp.Widget widget)
					{
						widget.AllowTreeFocus = (_items[_currentIndex] == item);
					}
				}
			}
		}

		void UpdateCurrentIndex(int index)
		{
			if (index >= 0 && index < _items.Count)
			{
				_currentIndex = index;
				_items[index].IndexItem.Select(true);
			}
		}

		void OnItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			UpdateItems();
		}

		void OnFocused(object sender, EventArgs e)
		{
			RotaryEventManager.Rotated += OnRotated;
			var item = _items[_currentIndex].NativeView as Widget;
			item?.SetFocus(true);
		}

		void OnUnfocused(object sender, EventArgs e)
		{
			RotaryEventManager.Rotated -= OnRotated;
		}

		void OnScrollStop(object sender, EventArgs e)
		{
			if (_updateByCode > 0)
				return;

			UpdateCurrentPage(_currentIndex);
			var currentItem = ShellSection.Items[_currentIndex];
			ShellSection.SetValueFromRenderer(ShellSection.CurrentItemProperty, currentItem);
			_isScrolling = false;

			var item = _items[_currentIndex].NativeView as Widget;
			item?.SetFocus(true);
		}

		protected void OnRotated(RotaryEventArgs args)
		{
			OnRotated(args.IsClockwise);
		}

		protected virtual bool OnRotated(bool isClockwise)
		{
			if (Forms.RotaryFocusObject != null)
				return false;

			MoveNextPage(isClockwise);
			return true;
		}

		protected void MoveNextPage(bool isClockwise)
		{
			var index = _currentIndex;
			_isScrolling = true;

			if (isClockwise)
			{
				RealizeItem(index + 2);
				_scroller.ScrollTo(index + 1, 0, true);
			}
			else
			{
				RealizeItem(index - 2);
				_scroller.ScrollTo(index - 1, 0, true);
			}
		}

		void OnScrolled(object sender, EventArgs e)
		{
			_isScrolling = true;
			if (_currentIndex != _scroller.HorizontalPageIndex)
			{
				UpdateCurrentIndex(_scroller.HorizontalPageIndex);
			}
		}

		void OnSectionPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(ShellSection.CurrentItem))
			{
				var newIndex = ShellSection.Items.IndexOf(ShellSection.CurrentItem);
				if (_currentIndex != newIndex)
				{
					UpdateCurrentPage(newIndex);
					_updateByCode++;
					_scroller.ScrollTo(newIndex, 0, false);
					_updateByCode--;
				}
			}
		}

		void OnLayout()
		{
			_indexIndicator.Geometry = _mainBox.Geometry;
			_scroller.Geometry = _mainBox.Geometry;
		}

		void OnInnerLayoutUpdate()
		{
			if (_lastLayoutBound == _innerContainer.Geometry)
			{
				return;
			}
			_lastLayoutBound = _innerContainer.Geometry;

			var layoutBound = _innerContainer.Geometry.Size;
			int baseX = _innerContainer.Geometry.X;

			ERect bound = _scroller.Geometry;
			int index = 0;
			foreach (var item in _items)
			{
				bound.X = baseX + index * bound.Width;
				item.Bound = bound;
				if (item.IsRealized)
				{
					item.NativeView.Geometry = bound;
				}
				index++;
			}
			_innerContainer.MinimumWidth = _items.Count * bound.Width;


			if (_items.Count > _currentIndex && _currentIndex >= 0 && !_isScrolling)
			{
				_updateByCode++;
				_scroller.ScrollTo(_currentIndex, 0, false);
				_updateByCode--;
			}
		}

		class ItemHolder
		{
			public bool IsRealized { get; set; }
			public ERect Bound { get; set; }
			public EvasObject NativeView { get; set; }
			public IndexItem IndexItem { get; set; }
			public ShellContent Item { get; set; }
		}
	}
}