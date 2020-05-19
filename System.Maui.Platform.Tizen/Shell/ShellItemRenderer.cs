using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using ElmSharp;
using EColor = ElmSharp.Color;
using EToolbarItem = ElmSharp.ToolbarItem;
using System.Maui.Platform.Tizen.Native;
using System.Collections.Specialized;
using System.Linq;

namespace System.Maui.Platform.Tizen
{
	public class ShellItemRenderer : IAppearanceObserver, IDisposable
	{
		IShellTabs _tabs = null;
		IFlyoutController _flyoutController = null;
		ShellItem _shellItem = null;

		Native.Box _box = null;
		Panel _drawer = null;
		ShellMoreToolbar _more = null;
		EToolbarItem _moreToolbarItem = null;
		ShellSectionNavigation _currentSection = null;

		Dictionary<EToolbarItem, ShellSection> _itemToSection = new Dictionary<EToolbarItem, ShellSection>();
		Dictionary<ShellSection, EToolbarItem> _sectionToitem = new Dictionary<ShellSection, EToolbarItem>();
		Dictionary<ShellSection, ShellSectionNavigation> _sectionToPage = new Dictionary<ShellSection, ShellSectionNavigation>();
		LinkedList<EToolbarItem> _toolbarItemList = new LinkedList<EToolbarItem>();

		bool _disposed = false;
		EColor _backgroudColor = ShellRenderer.DefaultBackgroundColor.ToNative();
		// The source of icon resources is https://materialdesignicons.com/
		const string _dotsIcon = "System.Maui.Platform.Tizen.Resource.dots_horizontal.png";

		public ShellItemRenderer(IFlyoutController flyoutController, ShellItem item)
		{
			_flyoutController = flyoutController;
			_shellItem = item;
			_shellItem.PropertyChanged += OnShellItemPropertyChanged;
			(_shellItem.Items as INotifyCollectionChanged).CollectionChanged += OnShellItemsCollectionChanged;

			_box = new Native.Box(System.Maui.Maui.NativeParent);
			_box.LayoutUpdated += OnLayoutUpdated;
			_box.Show();

			// Create Tabs
			_tabs = CreateTabs();
			_tabs.TargetView.Show();
			Control.PackEnd(_tabs as EvasObject);
			InitializeTabs();

			// Create More Tabs
			_more = CreateMoreToolbar();
			_more.Show();

			_drawer = CreateDrawer();
			_drawer.Show();
			Control.PackEnd(_drawer);
			InitialzeDrawer(_more);

			ResetToolbarItems();

			UpdateCurrentShellSection(_shellItem.CurrentItem);
			if (_drawer != null)
				_currentSection?.StackBelow(_drawer);

			((IShellController)_shellItem.Parent).AddAppearanceObserver(this, _shellItem);
		}

		~ShellItemRenderer()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public Native.Box Control
		{
			get
			{
				return _box;
			}
		}

		public EColor BackgroundColor
		{
			get
			{
				return _backgroudColor;
			}
			set
			{
				_backgroudColor = value;
				UpdateToolbarBackgroudColor(_backgroudColor);
			}
		}

		public void UpdateCurrentItem(ShellSection section)
		{
			UpdateCurrentShellSection(section);

			if (_drawer != null)
				_currentSection?.StackBelow(_drawer);

			if (_sectionToitem.ContainsKey(section))
			{
				_sectionToitem[section].IsSelected = true;
			}
			else if(section != null)
			{
				_drawer.IsOpen = false;
				var more = _toolbarItemList.Last() as EToolbarItem;
				more.IsSelected = true;
			}
			UpdateLayout();
		}

		public void SetCurrentItem(ShellSection section)
		{
			if (_shellItem.CurrentItem != section)
			{
				_shellItem.SetValueFromRenderer(ShellItem.CurrentItemProperty, section);
			}
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (disposing)
			{
				if (_shellItem != null)
				{
					Control.LayoutUpdated -= OnLayoutUpdated;
					((IShellController)_shellItem.Parent).RemoveAppearanceObserver(this);
					_shellItem.PropertyChanged -= OnShellItemPropertyChanged;
					(_shellItem.Items as INotifyCollectionChanged).CollectionChanged -= OnShellItemsCollectionChanged;
					_shellItem = null;

					foreach (var pair in _sectionToPage)
					{
						var navi = pair.Value as ShellSectionNavigation;
						navi.Dispose();
					}

					if (_tabs != null)
					{
						_tabs.Selected -= OnTabsSelected;
					}
					_itemToSection.Clear();
					_sectionToitem.Clear();
					_sectionToPage.Clear();
					_toolbarItemList.Clear();
				}
				Control.Unrealize();
			}
			_disposed = true;
		}

		protected virtual IShellTabs CreateTabs()
		{ 
			return new ShellTabs(System.Maui.Maui.NativeParent);
		}

		protected virtual ShellSectionNavigation CreateShellSectionNavigation(IFlyoutController flyoutController, ShellSection section)
		{
			return new ShellSectionNavigation(flyoutController, section);
		}

		void OnShellItemPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "CurrentItem")
			{
				UpdateCurrentItem(_shellItem.CurrentItem);
			}
		}

		void IAppearanceObserver.OnAppearanceChanged(ShellAppearance appearance)
		{
			if (appearance == null)
				return;

			_backgroudColor = appearance.BackgroundColor.ToNative();
			UpdateToolbarBackgroudColor(appearance.BackgroundColor.ToNative());
		}

		void UpdateToolbarBackgroudColor(ElmSharp.Color color)
		{
			foreach (EToolbarItem item in _toolbarItemList)
			{
				item.SetPartColor("bg", color);
			}
		}

		Panel CreateDrawer()
		{
			return new Panel(System.Maui.Maui.NativeParent);
		}

		void InitialzeDrawer(EvasObject content)
		{
			_drawer.SetScrollable(true);
			_drawer.SetScrollableArea(1.0);
			_drawer.Direction = PanelDirection.Bottom;
			_drawer.IsOpen = false;
			_drawer.SetContent(content, true);
		}

		ShellMoreToolbar CreateMoreToolbar()
		{
			return new ShellMoreToolbar(this);
		}

		void InitializeTabs()
		{
			_tabs.BackgroundColor = _backgroudColor;
			_tabs.Type = ShellTabsType.Fixed;
			_tabs.Selected += OnTabsSelected;
		}

		void ResetToolbarItems()
		{
			foreach (ShellSection section in _shellItem.Items)
			{
				InsertToolbarItem(section);
			}
		}

		void AddToolbarItems(NotifyCollectionChangedEventArgs e)
		{
			foreach (var item in e.NewItems)
			{
				InsertToolbarItem(item as ShellSection);
			}
			UpdateLayout();
		}

		void RemoveToolbarItems(NotifyCollectionChangedEventArgs e)
		{
			foreach (var item in e.OldItems)
			{
				RemoveToolbarItem(item as ShellSection);
			}
			UpdateLayout();
		}

		void OnShellItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					AddToolbarItems(e);
					break;

				case NotifyCollectionChangedAction.Remove:
					RemoveToolbarItems(e);
					break;

				default:
					break;
			}
		}

		string GetIconPath(ImageSource src)
		{
			if (src is FileImageSource fis)
			{
				return ResourcePath.GetPath(fis.File);
			}
			else
			{
				return null;
			}
		}

		void RemoveToolbarItem(ShellSection section)
		{
			if (_sectionToitem.ContainsKey(section))
			{
				var del = _sectionToitem[section];
				_toolbarItemList.Remove(del);
				_itemToSection.Remove(del);
				_sectionToitem.Remove(section);
				del.Delete();

				if (_moreToolbarItem != null)
				{
					ShellSection move = _more.RemoveFirst();
					InsertToolbarItem(move);
				}
			}
			else
			{
				_more.RemoveItem(section);
			}

			if (_more.Count == 1)
			{
				_toolbarItemList.Remove(_moreToolbarItem);
				_moreToolbarItem.Delete();
				_moreToolbarItem = null;

				ShellSection move = _more.RemoveFirst();
				InsertToolbarItem(move);
			}
		}

		void InsertToolbarItem(ShellSection section)
		{
			if (_toolbarItemList.Count < 5)
			{
				EToolbarItem item = null;
				if (_moreToolbarItem == null)
					item = _tabs.Append(section.Title, GetIconPath(section.Icon));
				else
					item = _tabs.InsertBefore(_moreToolbarItem, section.Title, GetIconPath(section.Icon));

				if (item != null)
				{
					item.SetPartColor("bg", _backgroudColor);
					item.SetPartColor("underline", EColor.Transparent);

					_toolbarItemList.AddLast(item);
					_itemToSection.Add(item, section);
					_sectionToitem.Add(section, item);
				}
			}
			else if (_moreToolbarItem == null && _toolbarItemList.Count == 5)
			{
				var last = _toolbarItemList.Last() as EToolbarItem;
				var lastSection = _itemToSection[last];

				_toolbarItemList.RemoveLast();
				_itemToSection.Remove(last);
				_sectionToitem.Remove(lastSection);
				last.Delete();

				_moreToolbarItem = CreateTabsItem("More");
				_toolbarItemList.AddLast(_moreToolbarItem);
				InitializeTabsItem(_moreToolbarItem, _dotsIcon);

				_more.AddItem(lastSection);
				_more.AddItem(section);
			}
			else
			{
				_more.AddItem(section);
			}
		}

		void InitializeTabsItem(EToolbarItem item, string resource)
		{
			//The source of icon resources is https://materialdesignicons.com/
			ImageSource src = ImageSource.FromResource(resource, typeof(ShellItemRenderer).GetTypeInfo().Assembly);
			Native.Image icon = new Native.Image(System.Maui.Maui.NativeParent);
			var task = icon.LoadFromImageSourceAsync(src);

			item.SetPartContent("elm.swallow.icon", icon);
			item.SetPartColor("bg", _backgroudColor);
			item.SetPartColor("underline", EColor.Transparent);
		}

		EToolbarItem CreateTabsItem(string text)
		{
			return _tabs.Append(text, null);
		}

		void UpdateCurrentShellSection(ShellSection section)
		{
			_currentSection?.Hide();

			if (section == null)
			{
				_currentSection = null;
				return;
			}

			ShellSectionNavigation native = null;
			if (_sectionToPage.ContainsKey(section))
			{
				native = _sectionToPage[section] as ShellSectionNavigation;
			}
			else
			{
				native = CreateShellSectionNavigation(_flyoutController, section);
				_sectionToPage[section] = native;
				Control.PackEnd(native);
			}
			_currentSection = native;
			_currentSection.Show();
			return;
		}

		void OnTabsSelected(object sender, ToolbarItemEventArgs e)
		{
			if (_tabs.SelectedItem == null)
				return;

			if (e.Item == _moreToolbarItem)
			{
				_drawer.IsOpen = true;
			}
			else
			{
				ShellSection section = _itemToSection[_tabs.SelectedItem];
				SetCurrentItem(section);
			}
		}

		void UpdateLayout()
		{
			OnLayoutUpdated(this, new LayoutEventArgs() { Geometry = Control.Geometry });
		}

		void OnLayoutUpdated(object sender, LayoutEventArgs e)
		{
			int toolbarHeight = _tabs.TargetView.MinimumHeight;
			if (_shellItem.Items.Count <= 1)
			{
				toolbarHeight = 0;
				_tabs?.TargetView.Hide();
				_drawer?.Hide();
			}

			_currentSection?.Move(e.Geometry.X, e.Geometry.Y);
			_currentSection?.Resize(e.Geometry.Width, e.Geometry.Height - toolbarHeight);
			if (_shellItem.Items.Count > 1)
			{
				_tabs.TargetView.Show();
				_tabs.TargetView.Move(e.Geometry.X, e.Geometry.Y + e.Geometry.Height - toolbarHeight);
				_tabs.TargetView.Resize(e.Geometry.Width, toolbarHeight);
				if (_drawer != null)
				{
					_drawer.Show();
					_drawer.Move(e.Geometry.X, e.Geometry.Y + e.Geometry.Height - _more.Height);
					_drawer.Resize(e.Geometry.Width, _more.Height);
				}
			}
		}
	}
}
