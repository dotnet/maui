#nullable enable

using System.Collections.Generic;
using Microsoft.Maui.Controls.Handlers.Items;
using Tizen.NUI;
using Tizen.NUI.BaseComponents;
using Tizen.UIExtensions.NUI;
using NCollectionView = Tizen.UIExtensions.NUI.CollectionView;
using NColor = Tizen.NUI.Color;
using NItemSizingStrategy = Tizen.UIExtensions.NUI.ItemSizingStrategy;
using NLayoutGroup = Tizen.NUI.LayoutGroup;
using NView = Tizen.NUI.BaseComponents.View;

namespace Microsoft.Maui.Controls.Platform
{
	public class ShellSectionView : NView
	{
		NCollectionView? _tabs;
		ItemTemplateAdaptor? _adaptor;

		ShellContent? _currentContent;
		NView? _currentView;

		int _lastSelected = 0;
		IList<ShellContent>? _cachedContents;

		Dictionary<IView, NView> _pageMap = new Dictionary<IView, NView>();
		Dictionary<IView, IViewHandler?> _handlerMap = new Dictionary<IView, IViewHandler?>();

		protected ShellSection ShellSection { get; private set; }
		protected IMauiContext? MauiContext { get; set; }

		protected NColor DefaultBackgroundColor = NColor.White;

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

		protected void CreateTabBar()
		{
			if (ShellSection.Items.Count <= 1 || !IsItemChanged())
				return;

			if (_tabs == null)
			{
				_tabs = new NCollectionView
				{
					SizeHeight = 40d.ToScaledPixel(),
					WidthSpecification = LayoutParamPolicies.MatchParent,
					LayoutManager = new LinearLayoutManager(true, NItemSizingStrategy.MeasureAllItems),
					SelectionMode = CollectionViewSelectionMode.SingleAlways,
					BackgroundColor = DefaultBackgroundColor
				};
				_tabs.ScrollView.HideScrollbar = true;
			}

			if (_adaptor != null)
				_adaptor.SelectionChanged -= OnTabItemSelected;

			_tabs.Adaptor = _adaptor = CreateItemAdaptor();
			_adaptor.SelectionChanged += OnTabItemSelected;

			_cachedContents = ShellSection.Items;

			Add(_tabs);
			(_tabs.Layout as NLayoutGroup)?.ChangeLayoutSiblingOrder(0);
		}

		protected virtual ItemTemplateAdaptor CreateItemAdaptor()
		{
			return new ShellItemTemplateAdaptor(ShellSection, ShellSection.Items);
		}

		void OnTabItemSelected(object? sender, CollectionViewSelectionChangedEventArgs e)
		{
			if (e.SelectedItems == null || e.SelectedItems.Count == 0 || _tabs == null)
				return;

			var selected = e.SelectedItems[0];
			var selectedIdx = _tabs.Adaptor?.GetItemIndex(selected) ?? 0;

			if (selectedIdx == _lastSelected)
				return;

			_lastSelected = selectedIdx;
			ShellSection.CurrentItem = (ShellContent)selected;
		}

		void UpdateCurrentItem()
		{
			CreateTabBar();
			_tabs?.RequestItemSelect(_lastSelected);
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