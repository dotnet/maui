using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.Specialized;
using ElmSharp;
using EToolbarItem = ElmSharp.ToolbarItem;
using EColor = ElmSharp.Color;
using System.Maui.Platform.Tizen.Native;

namespace System.Maui.Platform.Tizen
{
	public class ShellSectionRenderer : IAppearanceObserver
	{
		Native.Box _box = null;
		IShellTabs _tabs = null;
		Native.Page _currentContent = null;
		ShellSection _section = null;

		Dictionary<ShellContent, Native.Page> _contentToPage = new Dictionary<ShellContent, Native.Page>();
		Dictionary<ShellContent, EToolbarItem> _contentToItem = new Dictionary<ShellContent, EToolbarItem>();
		Dictionary<EToolbarItem, ShellContent> _itemToContent = new Dictionary<EToolbarItem, ShellContent>();
		LinkedList<EToolbarItem> _toolbarItemList = new LinkedList<EToolbarItem>();

		EColor _backgroundColor = ShellRenderer.DefaultBackgroundColor.ToNative();
		EColor _foregroundCollor = ShellRenderer.DefaultForegroundColor.ToNative();

		bool _disposed = false;

		public ShellSectionRenderer(ShellSection section)
		{
			_section = section;
			_section.PropertyChanged += OnSectionPropertyChanged;
			(_section.Items as INotifyCollectionChanged).CollectionChanged += OnShellSectionCollectionChanged;

			_box = new Native.Box(System.Maui.Maui.NativeParent);
			_box.LayoutUpdated += OnLayoutUpdated;

			_tabs = CreateToolbar();
			_tabs.TargetView.Show();
			Control.PackEnd(_tabs as EvasObject);
			InitializeTabs();

			ResetToolbarItem();
			UpdateCurrentShellContent(_section.CurrentItem);

			((IShellController)_section.Parent.Parent).AddAppearanceObserver(this, _section);
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
				return _backgroundColor;
			}
			set
			{
				_backgroundColor = value;
				UpdateToolbarBackgroudColor(_backgroundColor);
			}
		}

		public EColor ForegroundColor
		{
			get
			{
				return _foregroundCollor;
			}
			set
			{
				_foregroundCollor = value;
				UpdateToolbarForegroundColor(_foregroundCollor);
			}
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (disposing)
			{
				if (_section != null)
				{
					Control.LayoutUpdated -= OnLayoutUpdated;
					((IShellController)_section.Parent.Parent).RemoveAppearanceObserver(this);
					_section.PropertyChanged -= OnSectionPropertyChanged;
					_section = null;

					foreach (var pair in _contentToPage)
					{
						var content = pair.Value as Native.Page;
						content.Unrealize();
					}

					if (_tabs != null)
					{
						_tabs.Selected -= OnTabsSelected;
					}
					_contentToPage.Clear();
					_contentToItem.Clear();
					_itemToContent.Clear();
					_toolbarItemList.Clear();
				}
				Control.Unrealize();
			}
			_disposed = true;
		}

		protected virtual IShellTabs CreateToolbar()
		{
			return new ShellTabs(System.Maui.Maui.NativeParent);
		}

		void InitializeTabs()
		{
			_tabs.BackgroundColor = _backgroundColor;
			_tabs.Type = ShellTabsType.Fixed;
			_tabs.Selected += OnTabsSelected;
		}

		void OnSectionPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "CurrentItem")
			{
				UpdateCurrentItem(_section.CurrentItem);
			}
		}

		void UpdateCurrentItem(ShellContent content)
		{
			UpdateCurrentShellContent(content);
			if (_contentToItem.ContainsKey(content))
			{
				_contentToItem[content].IsSelected = true;
			}
			UpdateLayout();
		}

		void IAppearanceObserver.OnAppearanceChanged(ShellAppearance appearance)
		{
			if (appearance == null)
				return;

			if (!appearance.BackgroundColor.IsDefault)
			{
				BackgroundColor = appearance.BackgroundColor.ToNative();
			}
			else
			{
				BackgroundColor = ShellRenderer.DefaultBackgroundColor.ToNative();
			}

			if (!appearance.ForegroundColor.IsDefault)
			{
				ForegroundColor = appearance.ForegroundColor.ToNative();
			}
			else
			{
				ForegroundColor = ShellRenderer.DefaultForegroundColor.ToNative();
			}
		}

		void UpdateToolbarBackgroudColor(EColor color)
		{
			foreach (EToolbarItem item in _toolbarItemList)
			{
				item.SetPartColor("bg", color);
			}
		}

		void UpdateToolbarForegroundColor(EColor color)
		{
			foreach (EToolbarItem item in _toolbarItemList)
			{
				if (item != _tabs.SelectedItem)
				{
					item.SetPartColor("underline", EColor.Transparent);
				}
				else
				{
					item.SetPartColor("underline", color);
				}
			}
		}

		void ResetToolbarItem()
		{
			foreach (ShellContent content in _section.Items)
			{
				InsertToolbarItem(content);
			}
			if (_section.Items.Count > 3)
			{
				_tabs.Type = ShellTabsType.Scrollable;
			}
		}

		EToolbarItem InsertToolbarItem(ShellContent content)
		{
			EToolbarItem item = _tabs.Append(content.Title, null);
			item.SetPartColor("bg", _backgroundColor);

			_toolbarItemList.AddLast(item);
			_itemToContent.Add(item, content);
			_contentToItem.Add(content, item);

			return item;
		}

		void RemoveToolbarItem(ShellContent section)
		{
			if (_contentToItem.ContainsKey(section))
			{
				var del = _contentToItem[section];
				_toolbarItemList.Remove(del);
				_itemToContent.Remove(del);
				_contentToItem.Remove(section);
				del.Delete();
			}
		}

		void OnShellSectionCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
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

		void AddToolbarItems(NotifyCollectionChangedEventArgs e)
		{
			foreach (var item in e.NewItems)
			{
				InsertToolbarItem(item as ShellContent);
			}
			UpdateLayout();
		}

		void RemoveToolbarItems(NotifyCollectionChangedEventArgs e)
		{
			foreach (var item in e.OldItems)
			{
				RemoveToolbarItem(item as ShellContent);
			}
			UpdateLayout();
		}

		void OnTabsSelected(object sender, ToolbarItemEventArgs e)
		{
			if (_tabs.SelectedItem == null)
			{
				return;
			}

			ShellContent content = _itemToContent[_tabs.SelectedItem];
			if (_section.CurrentItem != content)
			{
				_section.SetValueFromRenderer(ShellSection.CurrentItemProperty, content);
			}
		}

		void UpdateCurrentShellContent(ShellContent content)
		{
			_currentContent?.Hide();

			if (content == null)
			{
				_currentContent = null;
				return;
			}

			Native.Page native = null;
			if (_contentToPage.ContainsKey(content))
			{
				native = _contentToPage[content];
			}
			else
			{
				native = CreateShellContent(content);
				Control.PackEnd(native);
				_contentToPage.Add(content, native);
			}
			_currentContent = native;
			_currentContent.Show();
			return;
		}

		Native.Page CreateShellContent(ShellContent content)
		{
			Page xpage = ((IShellContentController)content).GetOrCreateContent();
			Native.Page page = Platform.GetOrCreateRenderer(xpage).NativeView as Native.Page;
			page.BackgroundColor = (xpage.BackgroundColor != Color.Default ? xpage.BackgroundColor.ToNative() : EColor.White);
			return page;
		}

		void UpdateLayout()
		{
			OnLayoutUpdated(this, new LayoutEventArgs() { Geometry = Control.Geometry });
		}

		void OnLayoutUpdated(object sender, LayoutEventArgs e)
		{
			int toolbarHeight = 0;
			if (_section.Items.Count <= 1)
			{
				toolbarHeight = 0;
			}
			else
			{
				toolbarHeight = _tabs.TargetView.MinimumHeight;
			}
			_tabs.TargetView.Move(e.Geometry.X, e.Geometry.Y);
			_tabs.TargetView.Resize(e.Geometry.Width, toolbarHeight);
			_currentContent?.Move(e.Geometry.X, e.Geometry.Y + toolbarHeight);
			_currentContent?.Resize(e.Geometry.Width, e.Geometry.Height - toolbarHeight);
		}
	}
}
