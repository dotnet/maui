#nullable enable

using System;
using System.Collections;
using System.Linq;
using Microsoft.Maui.Controls.Handlers.Items;
using Tizen.NUI;
using Tizen.NUI.BaseComponents;
using Tizen.UIExtensions.Common;
using Tizen.UIExtensions.NUI;
using NCollectionView = Tizen.UIExtensions.NUI.CollectionView;
using NItemSizingStrategy = Tizen.UIExtensions.NUI.ItemSizingStrategy;
using NView = Tizen.NUI.BaseComponents.View;

namespace Microsoft.Maui.Controls.Platform
{
	public class ShellSearchview : MauiSearchBar
	{
		IEnumerable? _items;

		NView _itemsViewContainer;
		NCollectionView? _itemsView;
		ItemTemplateAdaptor? _adaptor;

		Element _element;

		public event EventHandler<ShellSearchViewItemSelectedEventArgs>? ItemSelected;

		public DataTemplate? ItemTemplate { get; set; }

		public IEnumerable? ItemsSource
		{
			get => _items;
			set
			{
				_items = value;
				SetItems();
			}
		}

		public ShellSearchview(Element element) : base()
		{
			_element = element;

			LayoutUpdated += OnLayoutUpdated;

			_itemsViewContainer = new NView
			{
				WidthSpecification = LayoutParamPolicies.MatchParent,
				HeightSpecification = LayoutParamPolicies.WrapContent,
				BackgroundColor = DefaultBackgroundColor
			};

			Children.Add(_itemsViewContainer);
		}

		static NCollectionView CreateItemsView()
		{
			return new NCollectionView
			{
				WidthSpecification = LayoutParamPolicies.MatchParent,
				HeightSpecification = LayoutParamPolicies.MatchParent,
				LayoutManager = new LinearLayoutManager(false, NItemSizingStrategy.MeasureFirstItem),
				SelectionMode = CollectionViewSelectionMode.SingleAlways,
			};
		}

		void OnLayoutUpdated(object? sender, LayoutEventArgs e)
		{
			_itemsViewContainer.Position = new Position(0, Entry.Position.Y + Entry.SizeHeight - ((float)Radius * 2));
		}

		void SetItems()
		{
			if (!string.IsNullOrEmpty(Entry.Text))
				ShowItems();
			else
				HideItems();
		}

		void ShowItems()
		{
			if (_items == null)
				return;

			if (_itemsView == null)
			{
				_itemsView = CreateItemsView();
				_itemsViewContainer.Add(_itemsView);
			}

			if (_adaptor != null)
				_adaptor.SelectionChanged -= OnItemSelected;

			_itemsView.Adaptor = _adaptor = CreateItemTemplateAdaptor();
			_adaptor.SelectionChanged += OnItemSelected;

			ConfigureLayout();

			_itemsView.Show();
			_itemsViewContainer.Show();
		}

		void HideItems()
		{
			_itemsView?.Hide();
			_itemsViewContainer.Hide();
		}

		ItemTemplateAdaptor CreateItemTemplateAdaptor()
		{
			return new ShellSearchItemAdaptor(_element, _items!, ItemTemplate);
		}

		void ConfigureLayout()
		{
			int itemCount = _items!.Cast<object>().Count();
			var screenSize = Devices.DeviceDisplay.MainDisplayInfo.GetScaledScreenSize();
			var maximumItemListHeight = screenSize.Height / 2;
			var itemListHeight = ((ICollectionViewController)_itemsView!).GetItemSize().ToDP().Height * itemCount;
			var itemsViewHeight = Math.Min(itemListHeight, maximumItemListHeight);

			_itemsViewContainer.SizeHeight = (float)itemsViewHeight.ToScaledPixel();
		}

		void OnItemSelected(object? sender, CollectionViewSelectionChangedEventArgs e)
		{
			Entry.Text = String.Empty;
			HideItems();
			var selected = e.SelectedItems?.FirstOrDefault();
			ItemSelected?.Invoke(this, new ShellSearchViewItemSelectedEventArgs(selected));
		}
	}

	public class ShellSearchViewItemSelectedEventArgs : EventArgs
	{
		public object? SelectedItem { get; private set; }

		public ShellSearchViewItemSelectedEventArgs(object? obj)
		{
			SelectedItem = obj;
		}
	}
}
