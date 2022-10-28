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
using NCollectionView = Tizen.UIExtensions.NUI.CollectionView;
using NColor = Tizen.NUI.Color;
using NLayoutGroup = Tizen.NUI.LayoutGroup;
using NShadow = Tizen.NUI.Shadow;
using NView = Tizen.NUI.BaseComponents.View;

namespace Microsoft.Maui.Controls.Platform
{
	public class ShellItemView : NView
	{
		NView? _currentSectionStack;

		NCollectionView? _tabbedView;
		ItemTemplateAdaptor? _adaptor;

		bool _isTabBarVisible;

		IList<ShellSection>? _cachedGroups;

		Dictionary<ShellSection, NView> _shellSectionStackCache = new Dictionary<ShellSection, NView>();

		protected Shell Shell { get; private set; }
		protected ShellItem ShellItem { get; private set; }
		protected IMauiContext MauiContext { get; private set; }

		protected NColor DefaultBackgroundColor = NColor.White;
		protected NColor DefaultBackdropColor = new NColor(0.2f, 0.2f, 0.2f, 0.2f);
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

			Add(_currentSectionStack);
			(_currentSectionStack.Layout as NLayoutGroup)?.ChangeLayoutSiblingOrder(0);
		}

		public void UpdateTabbarTitleColor(GColor? color)
		{
		}

		public void UpdateTabbarBackgroundColor(GColor? color)
		{
			if (_tabbedView != null)
				_tabbedView.BackgroundColor = color?.ToNUIColor();
		}

		protected virtual ItemTemplateAdaptor CreateItemAdaptor(IEnumerable items)
		{
			return new ShellSectionItemAdaptor(ShellItem, items);
		}

		void OnTabItemSelected(object? sender, CollectionViewSelectionChangedEventArgs e)
		{
			if (e.SelectedItems == null || e.SelectedItems.Count == 0)
				return;

			var selected = e.SelectedItems[0];

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
				MakeSimplePopup()?.Open();
			} 
		}

		Popup? MakeSimplePopup()
		{
			Popup popup = new Popup
			{
				Layout = new LinearLayout
				{
					VerticalAlignment = VerticalAlignment.Bottom
				},
				BackgroundColor = DefaultBackdropColor,
			};

			var items = ShellItem.Items.ToList().GetRange(MaxBottomItems - 1, ShellItem.Items.Count - MaxBottomItems + 1);
			var itemsView = new NCollectionView
			{
				WidthSpecification = LayoutParamPolicies.MatchParent,
				LayoutManager = new LinearLayoutManager(false),
				SelectionMode = CollectionViewSelectionMode.SingleAlways,
				SizeHeight = 50d.ToScaledPixel() * items.Count,
			};

			var adaptor = new ShellFlyoutItemAdaptor(Shell, items, false);
			adaptor.SelectionChanged += (s, e) =>
			{
				popup.Close();
				OnTabItemSelected(s, e);
			};

			itemsView.Adaptor = adaptor;
			itemsView.ScrollView.HideScrollbar = true;
			itemsView.BoxShadow = new NShadow(10d.ToPixel(), DefaultBackdropColor);

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
			if (ShellItem.Items.Count <= 1)
				return;

			if (_tabbedView != null && !IsItemChanged(ShellItem.Items))
				return;

			if (_tabbedView == null)
			{
				_tabbedView = new NCollectionView
				{
					SizeHeight = 80d.ToScaledPixel(),
					WidthSpecification = LayoutParamPolicies.MatchParent,
					SelectionMode = CollectionViewSelectionMode.SingleAlways,
					BackgroundColor = DefaultBackgroundColor
				};
				_tabbedView.ScrollView.HideScrollbar = true;
				_tabbedView.ScrollView.ScrollEnabled = false;
				Add(_tabbedView);
			}

			if (_adaptor != null)
				_adaptor.SelectionChanged -= OnTabItemSelected;

			if (ShellItem.Items.Count <= MaxBottomItems)
			{
				_adaptor = CreateItemAdaptor(ShellItem.Items);
			}
			else
			{
				var items = ShellItem.Items.ToList<object>().GetRange(0, MaxBottomItems - 1);
				items.Add(new MoreItem());
				_adaptor = CreateItemAdaptor(items);
			}

			_tabbedView.LayoutManager = new GridLayoutManager(false, ShellItem.Items.Count > MaxBottomItems ? MaxBottomItems : ShellItem.Items.Count);
			_tabbedView.Adaptor = _adaptor;
			_adaptor.SelectionChanged += OnTabItemSelected;
			
			_cachedGroups = ShellItem.Items.ToList();
		}

		void HideTabBar()
		{
			if (_tabbedView != null)
				Remove(_tabbedView);
		}

		class MoreItem
		{
			const string PathMoreVert = "M12 8c1.1 0 2-.9 2-2s-.9-2-2-2-2 .9-2 2 .9 2 2 2zm0 2c-1.1 0-2 .9-2 2s.9 2 2 2 2-.9 2-2-.9-2-2-2zm0 6c-1.1 0-2 .9-2 2s.9 2 2 2 2-.9 2-2-.9-2-2-2z";

			public string Title { get; set; } = "More";

			public string? IconPath { get; set; } = PathMoreVert;
		}
	}
}
