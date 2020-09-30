using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using ElmSharp;
using EBox = ElmSharp.Box;
using EColor = ElmSharp.Color;
using EToolbarItem = ElmSharp.ToolbarItem;

namespace Xamarin.Forms.Platform.Tizen
{
	public class ShellSectionRenderer : IAppearanceObserver, IDisposable
	{
		EBox _mainLayout = null;
		EBox _contentArea = null;
		IShellTabs _tabs = null;
		EvasObject _currentContent = null;
		Page _displayedPage;

		Dictionary<ShellContent, EvasObject> _contentCache = new Dictionary<ShellContent, EvasObject>();
		Dictionary<ShellContent, EToolbarItem> _contentToTabsItem = new Dictionary<ShellContent, EToolbarItem>();
		Dictionary<EToolbarItem, ShellContent> _itemToContent = new Dictionary<EToolbarItem, ShellContent>();
		List<EToolbarItem> _tabsItems = new List<EToolbarItem>();

		EColor _backgroundColor = ShellRenderer.DefaultBackgroundColor.ToNative();
		EColor _foregroundColor = ShellRenderer.DefaultForegroundColor.ToNative();

		bool _disposed = false;

		public ShellSectionRenderer(ShellSection section)
		{
			ShellSection = section;
			ShellSection.PropertyChanged += OnSectionPropertyChanged;
			(ShellSection.Items as INotifyCollectionChanged).CollectionChanged += OnShellSectionCollectionChanged;

			_mainLayout = new EBox(Forms.NativeParent);
			_mainLayout.SetLayoutCallback(OnLayout);

			_contentArea = new EBox(Forms.NativeParent);
			_contentArea.Show();
			_mainLayout.PackEnd(_contentArea);

			UpdateTabsItem();
			UpdateCurrentItem(ShellSection.CurrentItem);

			((IShellController)Shell.Current).AddAppearanceObserver(this, ShellSection);
			(ShellSection as IShellSectionController).AddDisplayedPageObserver(this, UpdateDisplayedPage);
		}

		bool HasTabs => _tabs != null;

		bool _tabBarIsVisible = true;

		bool TabBarIsVisible
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
						_tabs?.NativeView.Show();
					}
					else
					{
						_tabs?.NativeView.Hide();
					}
				}
			}
		}

		public ShellSection ShellSection { get; }

		public EvasObject NativeView
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

		~ShellSectionRenderer()
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
			var backgroundColor = (appearance as IShellAppearanceElement)?.EffectiveTabBarBackgroundColor ?? Color.Default;
			var foregroundColor = appearance?.ForegroundColor ?? Color.Default;
			ToolbarBackgroundColor = backgroundColor.IsDefault ? ShellRenderer.DefaultBackgroundColor.ToNative() : backgroundColor.ToNative();
			ToolbarForegroundColor = foregroundColor.IsDefault ? ShellRenderer.DefaultForegroundColor.ToNative() : foregroundColor.ToNative();
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

		void OnDisplayedPagePropertyChanged(object sender, PropertyChangedEventArgs e)
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

					foreach (var native in _contentCache.Values)
					{
						native.Unrealize();
					}
					_contentCache.Clear();
					_contentToTabsItem.Clear();
					_itemToContent.Clear();
				}
				NativeView.Unrealize();
			}
			_disposed = true;
		}

		protected virtual IShellTabs CreateToolbar()
		{
			return new ShellTabs(Forms.NativeParent);
		}

		void InitializeTabs()
		{
			if (_tabs != null)
			{
				return;
			}
			_tabs = CreateToolbar();
			_tabs.NativeView.Show();
			_tabs.BackgroundColor = _backgroundColor;
			_tabs.Scrollable = ShellTabsType.Fixed;
			_tabs.Selected += OnTabsSelected;
			_mainLayout.PackEnd(_tabs.NativeView);
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
			_tabs.NativeView.Unrealize();
			_tabs = null;
		}

		void OnSectionPropertyChanged(object sender, PropertyChangedEventArgs e)
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
			_tabs.Scrollable = ShellSection.Items.Count > 3 ? ShellTabsType.Scrollable : ShellTabsType.Fixed;
		}

		EToolbarItem InsertTabsItem(ShellContent content)
		{
			EToolbarItem item = _tabs.Append(content.Title, null);
			item.SetBackgroundColor(_backgroundColor);
			item.SetUnderlineColor(_foregroundColor);

			_tabsItems.Add(item);
			_itemToContent[item] = content;
			_contentToTabsItem[content] = item;
			return item;
		}

		void OnShellSectionCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			UpdateTabsItem();
		}

		void OnTabsSelected(object sender, ToolbarItemEventArgs e)
		{
			if (_tabs.SelectedItem == null)
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
				var native = CreateShellContent(content);
				native.SetAlignment(-1, -1);
				native.SetWeight(1, 1);
				_contentCache[content] = native;
			}
			_currentContent = _contentCache[content];
			_currentContent.Show();
			_contentArea.PackEnd(_currentContent);
		}

		EvasObject CreateShellContent(ShellContent content)
		{
			Page xpage = ((IShellContentController)content).GetOrCreateContent();
			return Platform.GetOrCreateRenderer(xpage).NativeView;
		}

		void OnLayout()
		{
			if (NativeView.Geometry.Width == 0 || NativeView.Geometry.Height == 0)
				return;
			var bound = NativeView.Geometry;

			int tabsHeight;
			if (HasTabs && TabBarIsVisible)
			{
				var tabsBound = bound;
				tabsHeight = _tabs.NativeView.MinimumHeight;
				tabsBound.Height = tabsHeight;
				_tabs.NativeView.Geometry = tabsBound;
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
