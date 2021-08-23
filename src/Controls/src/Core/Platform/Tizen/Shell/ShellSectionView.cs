using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using ElmSharp;
using Tizen.UIExtensions.ElmSharp;
using Tizen.UIExtensions.Shell;
using EBox = ElmSharp.Box;
using EColor = ElmSharp.Color;
using EToolbarItem = ElmSharp.ToolbarItem;
using EToolbarItemEventArgs = ElmSharp.ToolbarItemEventArgs;

namespace Microsoft.Maui.Controls.Platform
{
	public interface IShellSectionRenderer : IDisposable
	{
		EvasObject NativeView { get; }
	}

	public class ShellSectionView : IAppearanceObserver, IShellSectionRenderer
	{
		EBox _mainLayout = null;
		EBox _contentArea = null;
		Tabs _tabs = null;
		EvasObject _currentContent = null;
		Page _displayedPage;

		Dictionary<ShellContent, EvasObject> _contentCache = new Dictionary<ShellContent, EvasObject>();
		Dictionary<ShellContent, EToolbarItem> _contentToTabsItem = new Dictionary<ShellContent, EToolbarItem>();
		Dictionary<EToolbarItem, ShellContent> _itemToContent = new Dictionary<EToolbarItem, ShellContent>();
		List<EToolbarItem> _tabsItems = new List<EToolbarItem>();

		EColor _backgroundColor = ShellView.DefaultBackgroundColor;
		EColor _foregroundColor = ShellView.DefaultForegroundColor;

		bool _disposed = false;

		public ShellSectionView(ShellSection section, IMauiContext context)
		{
			ShellSection = section;
			MauiContext = context;
			ShellSection.PropertyChanged += OnSectionPropertyChanged;
			(ShellSection.Items as INotifyCollectionChanged).CollectionChanged += OnShellSectionCollectionChanged;

			_mainLayout = new EBox(NativeParent);
			_mainLayout.SetLayoutCallback(OnLayout);

			_contentArea = new EBox(NativeParent);
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

		protected EvasObject NativeParent
		{
			get => MauiContext?.Context?.BaseLayout;
		}

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

		~ShellSectionView()
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
			ToolbarBackgroundColor = backgroundColor.IsDefault() ? ShellView.DefaultBackgroundColor : backgroundColor.ToNativeEFL();
			ToolbarForegroundColor = foregroundColor.IsDefault() ? ShellView.DefaultForegroundColor : foregroundColor.ToNativeEFL();
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

		void InitializeTabs()
		{
			if (_tabs != null)
			{
				return;
			}
			_tabs = new Tabs(NativeParent);
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
			_tabs.Scrollable = ShellSection.Items.Count > 3 ? TabsType.Scrollable : TabsType.Fixed;
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

		void OnTabsSelected(object sender, EToolbarItemEventArgs e)
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
			return xpage.ToNative(MauiContext);
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
