using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reflection;
using ElmSharp;
using Microsoft.Extensions.DependencyInjection;
using Tizen.UIExtensions.ElmSharp;
using Tizen.UIExtensions.Shell;
using EBox = ElmSharp.Box;
using EColor = ElmSharp.Color;
using EToolbarItem = ElmSharp.ToolbarItem;
using EToolbarItemEventArgs = ElmSharp.ToolbarItemEventArgs;
using TImage = Tizen.UIExtensions.ElmSharp.Image;
using TThemeConstants = Tizen.UIExtensions.Shell.ThemeConstants;

namespace Microsoft.Maui.Controls.Platform
{
	public class ShellItemView : IAppearanceObserver, IDisposable
	{
		Tabs _tabs = null;
		EBox _mainLayout = null;
		EBox _contentHolder = null;
		Panel _moreItemsDrawer = null;
		ShellMoreTabs _moreItemsList = null;
		EToolbarItem _moreTabItem = null;
		ShellSectionStack _currentStack = null;

		Dictionary<EToolbarItem, ShellSection> _sectionsTable = new Dictionary<EToolbarItem, ShellSection>();
		Dictionary<ShellSection, EToolbarItem> _tabItemsTable = new Dictionary<ShellSection, EToolbarItem>();
		Dictionary<ShellSection, ShellSectionStack> _shellSectionStackCache = new Dictionary<ShellSection, ShellSectionStack>();
		List<EToolbarItem> _tabsItems = new List<EToolbarItem>();

		bool _disposed = false;
		Color _tabBarBackgroudColor = ShellView.DefaultBackgroundColor;
		Color _tabBarTitleColor = ShellView.DefaultTitleColor;

		const string _dotsIcon = TThemeConstants.Shell.Resources.DotsIcon;

		public ShellItemView(ShellItem item, IMauiContext context)
		{
			ShellItem = item;
			MauiContext = context;

			Initialize();

			ShellItem.PropertyChanged += OnShellItemPropertyChanged;
			if (ShellItem.Items is INotifyCollectionChanged notifyCollectionChanged)
			{
				notifyCollectionChanged.CollectionChanged += OnShellItemsCollectionChanged;
			}
			ShellController.AddAppearanceObserver(this, ShellItem);

			UpdateTabsItems();
			UpdateCurrentItem(ShellItem.CurrentItem);
		}

		~ShellItemView()
		{
			Dispose(false);
		}

		protected IMauiContext MauiContext { get; private set; }

		protected EvasObject NativeParent
		{
			get => MauiContext?.Context?.BaseLayout;
		}

		public EvasObject NativeView
		{
			get
			{
				return _mainLayout;
			}
		}

		public EColor TabBarBackgroundColor
		{
			get
			{
				return _tabBarBackgroudColor;
			}
			set
			{
				_tabBarBackgroudColor = value;
				UpdateTabsBackgroudColor(_tabBarBackgroudColor);
			}
		}

		public EColor TabBarTitleColor
		{
			get => _tabBarTitleColor;
			set
			{
				_tabBarTitleColor = value;
				UpdateTabBarTitleColor(value);
			}
		}

		ShellItem ShellItem { get; }
		IShellController ShellController => Shell.Current;
		bool HasMoreItems => _moreItemsDrawer != null;
		bool HasTabs => _tabs != null;

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (disposing)
			{
				ShellController.RemoveAppearanceObserver(this);
				if (ShellItem != null)
				{
					ShellItem.PropertyChanged -= OnShellItemPropertyChanged;
					if (ShellItem.Items is INotifyCollectionChanged notifyCollectionChanged)
					{
						notifyCollectionChanged.CollectionChanged -= OnShellItemsCollectionChanged;
					}

					foreach (var stack in _shellSectionStackCache.Values)
					{
						stack.Dispose();
					}

					DestroyMoreItems();
					DeinitializeTabs();

					_sectionsTable.Clear();
					_tabItemsTable.Clear();
					_shellSectionStackCache.Clear();
					_tabsItems.Clear();
				}
				NativeView.Unrealize();
			}
			_disposed = true;
		}

		protected virtual void UpdateTabsItems()
		{
			ResetTabs();
			if (ShellItem.Items.Count > 1)
			{
				InitializeTabs();
				foreach (ShellSection section in ShellItem.Items)
				{
					AddTabsItem(section);
				}
			}
			else
			{
				DeinitializeTabs();
			}
		}

		protected virtual ShellSectionStack CreateShellSectionStack(ShellSection section)
		{
			return new ShellSectionStack(section, MauiContext);
		}

		bool _disableMoreItemOpen;

		void UpdateCurrentItem(ShellSection section)
		{
			UpdateCurrentShellSection(section);

			if (HasTabs)
			{
				if (_tabItemsTable.ContainsKey(section))
				{
					_tabItemsTable[section].IsSelected = true;
				}
				else if (HasMoreItems)
				{
					_disableMoreItemOpen = true;
					_moreTabItem.IsSelected = true;
					_disableMoreItemOpen = false;
				}

				if (HasMoreItems)
				{
					_moreItemsDrawer.IsOpen = false;
				}
			}
		}

		void UpdateCurrentItemFromUI(ShellSection section)
		{
			if (ShellItem.CurrentItem != section)
			{
				ShellItem.SetValueFromRenderer(ShellItem.CurrentItemProperty, section);
			}
			if (HasMoreItems)
			{
				_moreItemsDrawer.IsOpen = false;
			}
		}

		void Initialize()
		{
			_mainLayout = new EBox(NativeParent);
			_mainLayout.SetLayoutCallback(OnLayout);
			_mainLayout.Show();
			_contentHolder = new EBox(NativeParent);
			_contentHolder.Show();
			_mainLayout.PackEnd(_contentHolder);
		}

		void InitializeTabs()
		{
			if (_tabs != null)
				return;

			_tabs = new Tabs(NativeParent);
			_tabs.Show();
			_tabs.BackgroundColor = _tabBarBackgroudColor;
			_tabs.Scrollable = TabsType.Fixed;
			_tabs.Selected += OnTabsSelected;
			_mainLayout.PackEnd(_tabs);
		}

		void DeinitializeTabs()
		{
			if (_tabs == null)
				return;

			_mainLayout.UnPack(_tabs);
			_tabs.Selected -= OnTabsSelected;
			_tabs.Unrealize();
			_tabs = null;
		}

		void CreateMoreItems()
		{
			if (_moreItemsDrawer != null)
				return;

			_moreItemsList = new ShellMoreTabs(NativeParent);
			_moreItemsList.Show();
			_moreItemsList.ItemSelected += OnMoreItemSelected;

			_moreItemsDrawer = new Panel(NativeParent);
			_moreItemsDrawer.Show();

			_moreItemsDrawer.SetScrollable(true);
			_moreItemsDrawer.SetScrollableArea(1.0);
			_moreItemsDrawer.Direction = PanelDirection.Bottom;
			_moreItemsDrawer.IsOpen = false;
			_moreItemsDrawer.SetContent(_moreItemsList, true);
			_mainLayout.PackEnd(_moreItemsDrawer);
		}

		void DestroyMoreItems()
		{
			if (_moreItemsDrawer == null)
				return;

			_mainLayout.UnPack(_moreItemsDrawer);

			_moreItemsList.Unrealize();
			_moreItemsDrawer.Unrealize();

			_moreItemsList = null;
			_moreItemsDrawer = null;
		}

		void OnMoreItemSelected(object sender, GenListItemEventArgs e)
		{
			ShellSection section = e.Item.Data as ShellSection;
			UpdateCurrentItemFromUI(section);
		}

		void OnShellItemPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(ShellItem.CurrentItem))
			{
				UpdateCurrentItem(ShellItem.CurrentItem);
			}
		}

		void IAppearanceObserver.OnAppearanceChanged(ShellAppearance appearance)
		{
			var tabBarBackgroudColor = (appearance as IShellAppearanceElement)?.EffectiveTabBarBackgroundColor;
			var tabBarTitleColor = (appearance as IShellAppearanceElement)?.EffectiveTabBarTitleColor;
			TabBarBackgroundColor = tabBarBackgroudColor.IsDefault() ? ShellView.DefaultBackgroundColor : tabBarBackgroudColor.ToNativeEFL();
			TabBarTitleColor = tabBarTitleColor.IsDefault() ? ShellView.DefaultTitleColor : tabBarTitleColor.ToNativeEFL();
		}

		void UpdateTabsBackgroudColor(EColor color)
		{
			foreach (EToolbarItem item in _tabsItems)
			{
				item.SetBackgroundColor(color);
			}
		}

		void UpdateTabBarTitleColor(EColor color)
		{
			foreach (EToolbarItem item in _tabsItems)
			{
				item.SetTextColor(color);
			}
		}

		void ResetTabs()
		{
			if (!HasTabs)
				return;

			foreach (var item in _tabsItems)
			{
				item.Delete();
			}
			_tabsItems.Clear();
			DestroyMoreItems();
			_moreTabItem = null;
		}

		void OnShellItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			UpdateTabsItems();
		}

		void AddTabsItem(ShellSection section)
		{
			if (_tabsItems.Count < 5)
			{
				var item = AppendTabsItem(section.Title, section.Icon);
				_sectionsTable.Add(item, section);
				_tabItemsTable.Add(section, item);
				_tabsItems.Add(item);
			}
			else if (!HasMoreItems)
			{
				CreateMoreItems();

				var last = _tabsItems.Last();
				var lastSection = _sectionsTable[last];

				_tabsItems.Remove(last);
				_sectionsTable.Remove(last);
				_tabItemsTable.Remove(lastSection);
				last.Delete();

				//The source of icon resources is https://materialdesignicons.com/
				var assembly = typeof(ShellItemView).GetTypeInfo().Assembly;
				var assemblyName = assembly.GetName().Name;
				_moreTabItem = AppendTabsItem("More", ImageSource.FromResource(assemblyName + "." + _dotsIcon, assembly));
				_tabsItems.Add(_moreTabItem);

				_moreItemsList.AddItem(lastSection);
				_moreItemsList.AddItem(section);
			}
			else
			{
				_moreItemsList.AddItem(section);
			}
		}

		void UpdateCurrentShellSection(ShellSection section)
		{
			if (_currentStack != null)
			{
				_currentStack.Hide();
				_contentHolder.UnPack(_currentStack);
			}
			_currentStack = null;

			if (section == null)
			{
				return;
			}

			ShellSectionStack native;
			if (_shellSectionStackCache.ContainsKey(section))
			{
				native = _shellSectionStackCache[section];
			}
			else
			{
				native = CreateShellSectionStack(section);
				_shellSectionStackCache[section] = native;
			}
			_currentStack = native;
			_currentStack.Show();
			_contentHolder.PackEnd(_currentStack);
		}

		void OnTabsSelected(object sender, EToolbarItemEventArgs e)
		{
			if (_tabs.SelectedItem == null)
				return;

			if (HasMoreItems && e.Item == _moreTabItem)
			{
				if (!_disableMoreItemOpen)
				{
					_moreItemsDrawer.IsOpen = !_moreItemsDrawer.IsOpen;
				}
			}
			else
			{
				UpdateCurrentItemFromUI(_sectionsTable[_tabs.SelectedItem]);
			}
		}

		void OnLayout()
		{
			if (NativeView.Geometry.Height == 0 || NativeView.Geometry.Width == 0)
				return;

			int tabsHeight = 0;
			var bound = _mainLayout.Geometry;
			if (HasTabs)
			{
				tabsHeight = _tabs.MinimumHeight;
				var tabsBound = bound;
				tabsBound.Y += (bound.Height - tabsHeight);
				tabsBound.Height = tabsHeight;
				_tabs.Geometry = tabsBound;
				if (HasMoreItems)
				{
					int moreItemListHeight = _moreItemsList.HeightRequest;
					moreItemListHeight = Math.Min(moreItemListHeight, bound.Height - tabsHeight);
					var moreItemDrawerBound = bound;
					moreItemDrawerBound.Y += (bound.Height - tabsHeight - moreItemListHeight);
					moreItemDrawerBound.Height = moreItemListHeight;
					_moreItemsDrawer.Geometry = moreItemDrawerBound;
				}
			}
			bound.Height -= tabsHeight;
			_contentHolder.Geometry = bound;
		}

		EToolbarItem AppendTabsItem(string text, ImageSource iconSource)
		{
			var item = _tabs.Append(text);
			if (iconSource != null)
			{
				TImage image = new TImage(NativeParent);
				var provider = MauiContext.Services.GetRequiredService<IImageSourceServiceProvider>();
				var service = provider.GetRequiredImageSourceService(iconSource);

				_ = service.LoadImageAsync(iconSource, image);

				item.SetIconPart(image);
			}
			item.SetBackgroundColor(_tabBarBackgroudColor);
			item.SetUnderlineColor(EColor.Transparent);
			item.SetTextColor(_tabBarTitleColor);

			return item;
		}
	}
}
