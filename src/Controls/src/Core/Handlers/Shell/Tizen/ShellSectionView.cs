#nullable enable

using System.Collections.Generic;
using Microsoft.Maui.Controls.Handlers.Items;
using Microsoft.Maui.Platform;
using Tizen.NUI;
using Tizen.NUI.BaseComponents;
using Tizen.UIExtensions.NUI;
using GColor = Microsoft.Maui.Graphics.Color;
using NCollectionView = Tizen.UIExtensions.NUI.CollectionView;
using NItemSizingStrategy = Tizen.UIExtensions.NUI.ItemSizingStrategy;
using NLayoutGroup = Tizen.NUI.LayoutGroup;
using NView = Tizen.NUI.BaseComponents.View;

namespace Microsoft.Maui.Controls.Platform
{
	public class ShellSectionView : NView
	{
		public readonly GColor DefaultTabBarBackgroundColor = GColor.FromArgb("#2196f3");

		NCollectionView? _topTabBar;
		ItemTemplateAdaptor? _adaptor;

		ShellContent? _currentContent;
		NView? _currentView;

		int _lastSelected = 0;
		IList<ShellContent>? _cachedContents;

		GColor? _backgroundColor;

		Dictionary<IView, NView> _pageMap = new Dictionary<IView, NView>();
		Dictionary<IView, IViewHandler?> _handlerMap = new Dictionary<IView, IViewHandler?>();

		protected ShellSection ShellSection { get; private set; }
		protected IMauiContext? MauiContext { get; set; }

		public ShellSectionView(ShellSection section, IMauiContext context)
		{
			ShellSection = section;
			MauiContext = context;

			HeightSpecification = LayoutParamPolicies.MatchParent;
			WidthSpecification = LayoutParamPolicies.MatchParent;

			Layout = new LinearLayout
			{
				LinearOrientation = LinearLayout.Orientation.Vertical,
			};

			UpdateCurrentItem();

			ShellSection.PropertyChanged += (s, e) =>
			{
				if (e.PropertyName == ShellSection.CurrentItemProperty.PropertyName)
				{
					UpdateCurrentItem();
				}
			};
		}

		public void UpdateTopTabBarColors(GColor? foregroundColor, GColor? backgroundColor, GColor? titleColor, GColor? unselectedColor)
		{
			_backgroundColor = backgroundColor;

			if (_topTabBar != null)
			{
				backgroundColor = ((backgroundColor != null) && backgroundColor.IsNotDefault()) ? backgroundColor : DefaultTabBarBackgroundColor;
				foregroundColor = ((foregroundColor != null) && foregroundColor.IsNotDefault()) ? foregroundColor : backgroundColor?.GetAccentColor();
				titleColor = ((titleColor != null) && titleColor.IsNotDefault()) ? titleColor : backgroundColor?.GetAccentColor();
				unselectedColor = ((unselectedColor != null) && unselectedColor.IsNotDefault()) ? unselectedColor : titleColor?.MultiplyAlpha(0.5f);

				_topTabBar.BackgroundColor = backgroundColor?.ToNUIColor();
				(_topTabBar.Adaptor as ShellContentItemAdaptor)?.UpdateItemsColor(foregroundColor, titleColor, unselectedColor);
			}
		}

		protected void CreateTabBar()
		{
			if (ShellSection.Items.Count <= 1 || !IsItemChanged())
				return;

			if (_topTabBar == null)
			{
				_topTabBar = new NCollectionView
				{
					SizeHeight = 40d.ToScaledPixel(),
					WidthSpecification = LayoutParamPolicies.MatchParent,
					LayoutManager = new LinearLayoutManager(true, NItemSizingStrategy.MeasureAllItems, 0),
					SelectionMode = CollectionViewSelectionMode.SingleAlways,
				};
				_topTabBar.ScrollView.HideScrollbar = true;

			}

			if (_adaptor != null)
				_adaptor.SelectionChanged -= OnTabItemSelected;

			_topTabBar.Adaptor = _adaptor = CreateItemAdaptor();
			_adaptor.SelectionChanged += OnTabItemSelected;

			_cachedContents = ShellSection.Items;

			Add(_topTabBar);
			(_topTabBar.Layout as NLayoutGroup)?.ChangeLayoutSiblingOrder(0);
		}

		protected virtual ItemTemplateAdaptor CreateItemAdaptor()
		{
			return new ShellContentItemAdaptor(ShellSection, ShellSection.Items);
		}

		void OnTabItemSelected(object? sender, CollectionViewSelectionChangedEventArgs e)
		{
			if (e.SelectedItems == null || e.SelectedItems.Count == 0 || _topTabBar == null)
				return;

			var selected = e.SelectedItems[0];
			var selectedIdx = _topTabBar.Adaptor?.GetItemIndex(selected) ?? 0;

			if (selectedIdx == _lastSelected)
				return;

			_lastSelected = selectedIdx;
			ShellSection.CurrentItem = (ShellContent)selected;
		}

		void UpdateCurrentItem()
		{
			CreateTabBar();
			_topTabBar?.RequestItemSelect(_lastSelected);
			UpdateContent(ShellSection.CurrentItem);
		}

		void UpdateContent(ShellContent shellContent)
		{
			if (_currentContent == shellContent)
				return;

			if (_currentView != null)
				Remove(_currentView);

			_currentView = GetShellContentView(shellContent);
			Add(_currentView);

			_currentContent = shellContent;
		}

		bool IsItemChanged()
		{
			var contents = ShellSection.Items;
			if (_cachedContents == null)
				return true;

			if (_cachedContents.Count != contents.Count)
				return true;

			for (int i = 0; i < contents.Count; i++)
			{
				if (_cachedContents[i] != contents[i])
				{
					return true;
				}
			}

			return false;
		}

		NView GetShellContentView(ShellContent shellContent)
		{
			var page = ((IShellContentController)shellContent).GetOrCreateContent();

			if (_pageMap.ContainsKey(page))
			{
				return _pageMap[page];
			}
			else
			{
				var content = page.ToPlatform(MauiContext!);
				_pageMap[page] = content;
				_handlerMap[page] = page.Handler;
				return content;
			}
		}
	}
}