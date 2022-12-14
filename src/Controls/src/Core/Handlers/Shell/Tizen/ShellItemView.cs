#nullable enable

using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Microsoft.Maui.Controls.Handlers.Items;
using Tizen.NUI;
using Tizen.NUI.BaseComponents;
using Tizen.UIExtensions.NUI;
using GColor = Microsoft.Maui.Graphics.Color;
using GColors = Microsoft.Maui.Graphics.Colors;
using NCollectionView = Tizen.UIExtensions.NUI.CollectionView;
using NLayoutGroup = Tizen.NUI.LayoutGroup;
using NShadow = Tizen.NUI.Shadow;
using NView = Tizen.NUI.BaseComponents.View;

namespace Microsoft.Maui.Controls.Platform
{
	public class ShellItemView : NView
	{
		public readonly GColor DefaultTabBarBackgroundColor = new GColor(1f, 1f, 1f, 1f);
		public readonly GColor DefaultBackdropColor = new GColor(0.2f, 0.2f, 0.2f, 0.2f);

		NView? _currentSectionStack;

		NCollectionView? _bottomTabBar;
		ItemTemplateAdaptor? _adaptor;
		bool _isTabBarVisible;
		int _lastSelected = 0;

		IList<ShellSection>? _cachedGroups;

		Dictionary<ShellSection, NView> _shellSectionStackCache = new Dictionary<ShellSection, NView>();

		protected Shell Shell { get; private set; }
		protected ShellItem ShellItem { get; private set; }
		protected IMauiContext MauiContext { get; private set; }
		protected IShellItemController ShellItemController => (ShellItem as IShellItemController)!;

		protected int MaxBottomItems = 5;

		public ShellItemView(ShellItem item, IMauiContext context) : base()
		{
			ShellItem = item;
			MauiContext = context;
			Shell = (Shell)item.Parent;

			_isTabBarVisible = true;

			HeightSpecification = LayoutParamPolicies.MatchParent;
			WidthSpecification = LayoutParamPolicies.MatchParent;

			if (ShellItem.Items is INotifyCollectionChanged notifyCollectionChanged)
			{
				notifyCollectionChanged.CollectionChanged += OnShellItemsCollectionChanged;
			}

			Layout = new LinearLayout
			{
				LinearOrientation = LinearLayout.Orientation.Vertical,
			};
			UpdateTabBar(true);
		}

		public void UpdateTabBar(bool isVisible)
		{
			if (isVisible)
				ShowTabBar();
			else
				HideTabBar();

			_isTabBarVisible = isVisible;
		}

		public void UpdateCurrentItem(ShellSection section)
		{
			if (_currentSectionStack != null)
			{
				Remove(_currentSectionStack);
				_currentSectionStack = null;
			}

			if (_shellSectionStackCache.ContainsKey(section))
			{
				_currentSectionStack = _shellSectionStackCache[section];
			}
			else
			{
				_currentSectionStack = section.ToPlatform(MauiContext);
				_shellSectionStackCache[section] = _currentSectionStack;
			}

			var selectedIdx = _bottomTabBar?.Adaptor?.GetItemIndex(section) ?? 0;
			_lastSelected = selectedIdx < 0 ? MaxBottomItems - 1 : selectedIdx;
			_bottomTabBar?.RequestItemSelect(_lastSelected);

			Add(_currentSectionStack);
			(_currentSectionStack.Layout as NLayoutGroup)?.ChangeLayoutSiblingOrder(0);
		}

		public void UpdateBottomTabBarColors(GColor? backgroundColor, GColor? titleColor, GColor? unselectedColor)
		{
			if (_bottomTabBar != null)
			{
				backgroundColor = ((backgroundColor != null) && backgroundColor.IsNotDefault()) ? backgroundColor : DefaultTabBarBackgroundColor;
				titleColor = ((titleColor != null) && titleColor.IsNotDefault()) ? titleColor : backgroundColor?.GetAccentColor();
				unselectedColor = ((unselectedColor != null) && unselectedColor.IsNotDefault()) ? unselectedColor : titleColor?.MultiplyAlpha(0.5f);

				_bottomTabBar.BackgroundColor = backgroundColor?.ToNUIColor();
				(_bottomTabBar.Adaptor as ShellSectionItemAdaptor)?.UpdateItemsColor(titleColor, unselectedColor);
			}
		}

		protected virtual ItemTemplateAdaptor CreateItemAdaptor(IEnumerable items)
		{
			return new ShellSectionItemAdaptor(ShellItem, items);
		}

		void OnTabItemSelected(object? sender, CollectionViewSelectionChangedEventArgs e)
		{
			if (e.SelectedItems == null || e.SelectedItems.Count == 0 || _bottomTabBar == null)
				return;

			var selected = e.SelectedItems[0];
			var selectedIdx = _bottomTabBar.Adaptor?.GetItemIndex(selected) ?? 0;

			if (selectedIdx == _lastSelected)
				return;

			_lastSelected = selectedIdx;
			if (selected is ShellSection shellSection)
			{
				Shell.CurrentItem = shellSection;
			}
			else if (selected is ShellContent shellContent)
			{
				Shell.CurrentItem = shellContent;
			}
			else
			{
				MakeSimplePopup().Open();
			}
		}

		Popup MakeSimplePopup()
		{
			Popup popup = new Popup
			{
				Layout = new LinearLayout
				{
					VerticalAlignment = VerticalAlignment.Bottom
				},
				BackgroundColor = DefaultBackdropColor.ToNUIColor(),
			};

			var items = ShellItemController.GetItems().ToList().GetRange(MaxBottomItems - 1, ShellItem.Items.Count - MaxBottomItems + 1);
			var itemsView = new NCollectionView
			{
				WidthSpecification = LayoutParamPolicies.MatchParent,
				LayoutManager = new LinearLayoutManager(false),
				SelectionMode = CollectionViewSelectionMode.SingleAlways,
				SizeHeight = 50d.ToScaledPixel() * items.Count,
			};

			var adaptor = new MoreItemAdaptor(ShellItem, items);
			adaptor.SelectionChanged += (s, e) =>
			{
				popup.Close();
				OnTabItemSelected(s, e);
			};

			itemsView.Adaptor = adaptor;
			itemsView.ScrollView.HideScrollbar = true;
			itemsView.BoxShadow = new NShadow(10d.ToPixel(), DefaultBackdropColor.ToNUIColor());

			popup.OutsideClicked += (s, e) =>
			{
				popup.Close();
			};

			popup.Content = itemsView;

			return popup;
		}


		void OnShellItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
		{
			UpdateTabBar(_isTabBarVisible);
		}

		bool IsItemChanged(IList<ShellSection> groups)
		{
			if (_cachedGroups == null)
				return true;

			if (_cachedGroups.Count != groups.Count)
				return true;

			for (int i = 0; i < groups.Count; i++)
			{
				if (_cachedGroups[i] != groups[i])
				{
					return true;
				}
			}

			return false;
		}

		void ShowTabBar()
		{
			if (!ShellItemController.ShowTabs)
				return;

			if (_bottomTabBar != null && !IsItemChanged(ShellItem.Items))
				return;

			if (_bottomTabBar == null)
			{
				_bottomTabBar = new NCollectionView
				{
					SizeHeight = 80d.ToScaledPixel(),
					WidthSpecification = LayoutParamPolicies.MatchParent,
					SelectionMode = CollectionViewSelectionMode.SingleAlways,
					BackgroundColor = DefaultTabBarBackgroundColor.ToNUIColor(),
				};
				_bottomTabBar.ScrollView.HideScrollbar = true;
				_bottomTabBar.ScrollView.ScrollEnabled = false;
				Add(_bottomTabBar);
			}

			if (_adaptor != null)
				_adaptor.SelectionChanged -= OnTabItemSelected;

			var items = ShellItemController.GetItems().ToList<object>();
			if (items.Count > MaxBottomItems)
			{
				items = items.GetRange(0, MaxBottomItems - 1);
				items.Add(new MoreItem());
			}

			_bottomTabBar.LayoutManager = new GridLayoutManager(false, items.Count > MaxBottomItems ? MaxBottomItems : items.Count);
			_bottomTabBar.Adaptor = _adaptor = CreateItemAdaptor(items);
			_adaptor.SelectionChanged += OnTabItemSelected;

			_bottomTabBar.RequestItemSelect(_lastSelected);

			_cachedGroups = ShellItem.Items;
		}

		void HideTabBar()
		{
			if (_bottomTabBar != null)
				Remove(_bottomTabBar);
		}

		class MoreItem
		{
			const string PathMoreVert = "M12 8c1.1 0 2-.9 2-2s-.9-2-2-2-2 .9-2 2 .9 2 2 2zm0 2c-1.1 0-2 .9-2 2s.9 2 2 2 2-.9 2-2-.9-2-2-2zm0 6c-1.1 0-2 .9-2 2s.9 2 2 2 2-.9 2-2-.9-2-2-2z";

			public string Title { get; set; } = "More";

			public string? IconPath { get; set; } = PathMoreVert;
		}

		class MoreItemAdaptor : ItemTemplateAdaptor
		{
			public MoreItemAdaptor(Element element, IEnumerable items) : base(element, items, GetTemplate()) { }

			protected override bool IsSelectable => true;

			static DataTemplate GetTemplate()
			{
				return new DataTemplate(() =>
				{
					return new ShellFlyoutItemView();
				});
			}
		}
	}
}
