#nullable enable

using System.Collections.Generic;
using System.Collections.Specialized;
using Microsoft.Maui.Controls.Handlers.Items;
using Tizen.NUI;
using Tizen.NUI.BaseComponents;
using Tizen.UIExtensions.NUI;
using GColor = Microsoft.Maui.Graphics.Color;
using NCollectionView = Tizen.UIExtensions.NUI.CollectionView;
using NColor = Tizen.NUI.Color;
using NLayoutGroup = Tizen.NUI.LayoutGroup;
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

		protected virtual ItemTemplateAdaptor CreateItemAdaptor()
		{
			return new ShellItemTemplateAdaptor(ShellItem, ShellItem.Items);
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
					LayoutManager = new LinearLayoutManager(true),
					SelectionMode = CollectionViewSelectionMode.SingleAlways,
					BackgroundColor = DefaultBackgroundColor
				};
				_tabbedView.ScrollView.HideScrollbar = true;
				Add(_tabbedView);
			}

			if (_adaptor != null)
				_adaptor.SelectionChanged -= OnTabItemSelected;

			_tabbedView.Adaptor = _adaptor = CreateItemAdaptor();
			_adaptor.SelectionChanged += OnTabItemSelected;
			_cachedGroups = ShellItem.Items;
		}

		void HideTabBar()
		{
			if (_tabbedView != null)
				Remove(_tabbedView);
		}
	}
}
