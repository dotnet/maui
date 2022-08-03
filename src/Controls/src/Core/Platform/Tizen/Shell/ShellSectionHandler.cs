#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using ElmSharp;
using Tizen.UIExtensions.ElmSharp;
using EBox = ElmSharp.Box;
using EColor = ElmSharp.Color;
using EToolbarItem = ElmSharp.ToolbarItem;
using EToolbarItemEventArgs = ElmSharp.ToolbarItemEventArgs;
using TThemeConstants = Tizen.UIExtensions.ElmSharp.ThemeConstants;

namespace Microsoft.Maui.Controls.Platform
{
	public interface IShellSectionHandler : IDisposable
	{
		EvasObject PlatformView { get; }
	}

	public class ShellSectionHandler : IAppearanceObserver, IShellSectionHandler
	{
		EBox _mainLayout;
		EBox _contentArea;
		Tabs? _tabs = null;
		EvasObject? _currentContent = null;
		Page? _displayedPage;

		Dictionary<ShellContent, EvasObject> _contentCache = new Dictionary<ShellContent, EvasObject>();
		Dictionary<ShellContent, EToolbarItem> _contentToTabsItem = new Dictionary<ShellContent, EToolbarItem>();
		Dictionary<EToolbarItem, ShellContent> _itemToContent = new Dictionary<EToolbarItem, ShellContent>();
		List<EToolbarItem> _tabsItems = new List<EToolbarItem>();

		EColor _backgroundColor = TThemeConstants.Shell.ColorClass.DefaultBackgroundColor;
		EColor _foregroundColor = TThemeConstants.Shell.ColorClass.DefaultForegroundColor;

		bool _disposed = false;

		public ShellSectionHandler(ShellSection section, IMauiContext context)
		{
			ShellSection = section;
			MauiContext = context;
			ShellSection.PropertyChanged += OnSectionPropertyChanged;
			if (ShellSection.Items is INotifyCollectionChanged collection)
			{
				collection.CollectionChanged += OnShellSectionCollectionChanged;
			}

			_mainLayout = new EBox(PlatformParent);
			_mainLayout.SetLayoutCallback(OnLayout);

			_contentArea = new EBox(PlatformParent);
			_contentArea.Show();
			_mainLayout.PackEnd(_contentArea);

			UpdateTabsItem();
			UpdateCurrentItem(ShellSection.CurrentItem);

			((IShellController)Shell.Current).AddAppearanceObserver(this, ShellSection);
			(ShellSection as IShellSectionController).AddDisplayedPageObserver(this, UpdateDisplayedPage);
		}

		bool HasTabs => _tabs != null;

		bool _tabBarIsVisible = true;

		protected IMauiContext MauiContext { get; private set; }

		protected EvasObject PlatformParent => MauiContext.GetPlatformParent();

		protected virtual bool TabBarIsVisible
		{
			get => _tabBarIsVisible;
			set
			{
				if (_tabBarIsVisible != value)
				{
					_tabBarIsVisible = value;
					_mainLayout.MarkChanged();

					if (value)
					{
						_tabs?.Show();
					}
					else
					{
						_tabs?.Hide();
					}
				}
			}
		}

		public ShellSection ShellSection { get; }

		public EvasObject PlatformView
		{
			get
			{
				return _mainLayout;
			}
		}

		public EColor ToolbarBackgroundColor
		{
			get
			{
				return _backgroundColor;
			}
			set
			{
				_backgroundColor = value;
				UpdateToolbarBackgroudColor(_backgroundColor);
			}
		}

		public EColor ToolbarForegroundColor
		{
			get
			{
				return _foregroundColor;
			}
			set
			{
				_foregroundColor = value;
				UpdateToolbarForegroundColor(_foregroundColor);
			}
		}

		~ShellSectionHandler()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		void IAppearanceObserver.OnAppearanceChanged(ShellAppearance appearance)
		{
			var backgroundColor = (appearance as IShellAppearanceElement)?.EffectiveTabBarBackgroundColor;
			var foregroundColor = appearance?.ForegroundColor;

			ToolbarBackgroundColor = backgroundColor.IsDefault() ? TThemeConstants.Shell.ColorClass.DefaultBackgroundColor : (backgroundColor?.ToPlatformEFL()).GetValueOrDefault();
			ToolbarForegroundColor = foregroundColor.IsDefault() ? TThemeConstants.Shell.ColorClass.DefaultForegroundColor : (foregroundColor?.ToPlatformEFL()).GetValueOrDefault();
		}

		void UpdateDisplayedPage(Page page)
		{
			if (_displayedPage != null)
			{
				_displayedPage.PropertyChanged -= OnDisplayedPagePropertyChanged;
			}

			if (page == null)
			{
				TabBarIsVisible = true;
				return;
			}
			_displayedPage = page;
			_displayedPage.PropertyChanged += OnDisplayedPagePropertyChanged;
			TabBarIsVisible = Shell.GetTabBarIsVisible(page);
		}

		void OnDisplayedPagePropertyChanged(object? sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == Shell.TabBarIsVisibleProperty.PropertyName)
			{
				TabBarIsVisible = Shell.GetTabBarIsVisible(_displayedPage);
			}
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (disposing)
			{
				((IShellController)Shell.Current).RemoveAppearanceObserver(this);
				if (ShellSection != null)
				{
					(ShellSection as IShellSectionController).RemoveDisplayedPageObserver(this);
					ShellSection.PropertyChanged -= OnSectionPropertyChanged;
					DeinitializeTabs();

					foreach (var platformView in _contentCache.Values)
					{
						platformView.Unrealize();
					}
					_contentCache.Clear();
					_contentToTabsItem.Clear();
					_itemToContent.Clear();
				}
				PlatformView.Unrealize();
			}
			_disposed = true;
		}

		void InitializeTabs()
		{
			if (_tabs != null)
			{
				return;
			}
			_tabs = new Tabs(PlatformParent);
			_tabs.Show();
			_tabs.BackgroundColor = _backgroundColor;
			_tabs.Scrollable = TabsType.Fixed;
			_tabs.Selected += OnTabsSelected;
			_mainLayout.PackEnd(_tabs);
		}

		void ClearTabsItem()
		{
			if (!HasTabs)
				return;

			foreach (var item in _tabsItems)
			{
				item.Delete();
			}
			_tabsItems.Clear();
			_contentToTabsItem.Clear();
			_itemToContent.Clear();
		}

		void DeinitializeTabs()
		{
			if (_tabs == null)
			{
				return;
			}
			ClearTabsItem();
			_tabs.Unrealize();
			_tabs = null;
		}

		void OnSectionPropertyChanged(object? sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "CurrentItem")
			{
				UpdateCurrentItem(ShellSection.CurrentItem);
			}
		}

		void UpdateCurrentItem(ShellContent content)
		{
			UpdateCurrentShellContent(content);
			if (_contentToTabsItem.ContainsKey(content))
			{
				_contentToTabsItem[content].IsSelected = true;
			}
		}

		void UpdateToolbarBackgroudColor(EColor color)
		{
			foreach (EToolbarItem item in _tabsItems)
			{
				item.SetBackgroundColor(color);
			}
		}

		void UpdateToolbarForegroundColor(EColor color)
		{
			foreach (EToolbarItem item in _tabsItems)
			{
				item.SetUnderlineColor(color);
			}
		}

		void UpdateTabsItem()
		{
			if (ShellSection.Items.Count <= 1)
			{
				DeinitializeTabs();
				return;
			}

			InitializeTabs();
			ClearTabsItem();
			foreach (ShellContent content in ShellSection.Items)
			{
				InsertTabsItem(content);
			}

			if (_tabs != null)
				_tabs.Scrollable = ShellSection.Items.Count > 3 ? TabsType.Scrollable : TabsType.Fixed;
		}

		EToolbarItem? InsertTabsItem(ShellContent content)
		{
			EToolbarItem? item = _tabs?.Append(content.Title, null);
			item?.SetBackgroundColor(_backgroundColor);
			item?.SetUnderlineColor(_foregroundColor);

			if (item != null)
			{
				_tabsItems.Add(item);
				_itemToContent[item] = content;
				_contentToTabsItem[content] = item;
			}

			return item;
		}

		void OnShellSectionCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
		{
			UpdateTabsItem();
		}

		void OnTabsSelected(object? sender, EToolbarItemEventArgs e)
		{
			if (_tabs?.SelectedItem == null)
			{
				return;
			}

			ShellContent content = _itemToContent[_tabs.SelectedItem];
			if (ShellSection.CurrentItem != content)
			{
				ShellSection.SetValueFromRenderer(ShellSection.CurrentItemProperty, content);
			}
		}

		void UpdateCurrentShellContent(ShellContent content)
		{
			if (_currentContent != null)
			{
				_currentContent.Hide();
				_contentArea.UnPack(_currentContent);
				_currentContent = null;
			}

			if (content == null)
			{
				return;
			}

			if (!_contentCache.ContainsKey(content))
			{
				var platformView = CreateShellContent(content);
				platformView.SetAlignment(-1, -1);
				platformView.SetWeight(1, 1);
				_contentCache[content] = platformView;
			}
			_currentContent = _contentCache[content];
			_currentContent.Show();
			_contentArea.PackEnd(_currentContent);
		}

		EvasObject CreateShellContent(ShellContent content)
		{
			Page xpage = ((IShellContentController)content).GetOrCreateContent();
			return xpage.ToPlatform(MauiContext);
		}

		void OnLayout()
		{
			if (PlatformView.Geometry.Width == 0 || PlatformView.Geometry.Height == 0)
				return;
			var bound = PlatformView.Geometry;

			int tabsHeight;
			if (_tabs != null && TabBarIsVisible)
			{
				var tabsBound = bound;
				tabsHeight = _tabs.MinimumHeight;
				tabsBound.Height = tabsHeight;
				_tabs.Geometry = tabsBound;
			}
			else
			{
				tabsHeight = 0;
			}

			var contentBound = bound;
			contentBound.Y += tabsHeight;
			contentBound.Height -= tabsHeight;
			_contentArea.Geometry = contentBound;
		}
	}
}
