using System;
using System.Collections;
using System.Collections.Generic;
using ElmSharp;
using EColor = ElmSharp.Color;

namespace Xamarin.Forms.Platform.Tizen
{
	public class NavigationView : Native.Box, INavigationView
	{
		EvasObject _header;
		GenList _menu;
		GenItemClass _defaultClass;
		EColor _backgroundColor;
		EColor _defaultBackgroundColor = EColor.White;

		List<Group> _groups;
		IDictionary<Item, Element> _flyoutMenu = new Dictionary<Item, Element>();

		public NavigationView(EvasObject parent) : base(parent)
		{
			Initialize(parent);
			LayoutUpdated += (s, e) =>
			{
				UpdateChildGeometry();
			};
			base.BackgroundColor = _defaultBackgroundColor;
			_menu.BackgroundColor = _defaultBackgroundColor;
		}

		public event EventHandler<SelectedItemChangedEventArgs> SelectedItemChanged;

		public override EColor BackgroundColor
		{
			get
			{
				return _backgroundColor;
			}
			set
			{
				_backgroundColor = value;

				EColor effectiveColor = _backgroundColor.IsDefault ? _defaultBackgroundColor : _backgroundColor;
				_menu.BackgroundColor = effectiveColor;
				base.BackgroundColor = effectiveColor;
			}
		}

		public EvasObject Header
		{
			get
			{
				return _header;
			}
			set
			{
				if (_header != null)
				{
					UnPack(_header);
					_header.Hide();
				}
				_header = value;

				if (_header != null)
				{
					PackStart(_header);
					if (!_header.IsVisible)
					{
						_header.Show();
					}
				}

				UpdateChildGeometry();
			}
		}

		public void BuildMenu(List<List<Element>> flyoutGroups)
		{
			var groups = new List<Group>();
			_flyoutMenu.Clear();

			for (int i = 0; i < flyoutGroups.Count; i++)
			{
				var flyoutGroup = flyoutGroups[i];
				var items = new List<Item>();
				for (int j = 0; j < flyoutGroup.Count; j++)
				{
					string title = null;
					ImageSource icon = null;
					if (flyoutGroup[j] is BaseShellItem shellItem)
					{
						title = shellItem.Title;

						if (shellItem.FlyoutIcon is FileImageSource flyoutIcon)
						{
							icon = flyoutIcon;
						}
					}
					else if (flyoutGroup[j] is MenuItem menuItem)
					{
						title = menuItem.Text;
						if (menuItem.IconImageSource != null)
						{
							icon = menuItem.IconImageSource;
						}
					}
					Item item = new Item(title, icon);
					items.Add(item);

					_flyoutMenu.Add(item, flyoutGroup[j]);
				}
				var group = new Group(items);
				groups.Add(group);
			}
			_groups = groups;
			UpdateMenu();
		}

		void Initialize(EvasObject parent)
		{
			_menu = new GenList(parent)
			{
				BackgroundColor = EColor.Transparent,
				Style = "solid/default",
			};

			_menu.ItemSelected += (s, e) =>
			{
				_flyoutMenu.TryGetValue(e.Item.Data as Item, out Element element);

				SelectedItemChanged?.Invoke(this, new SelectedItemChangedEventArgs(element, -1));
			};

			_menu.Show();
			PackEnd(_menu);

			_defaultClass = new GenItemClass("double_label")
			{
				GetTextHandler = (obj, part) =>
				{
					if (part == "elm.text")
					{
						return ((Item)obj).Title;
					}
					else
					{
						return null;
					}
				},
				GetContentHandler = (obj, part) =>
				{
					if (part == "elm.swallow.icon")
					{
						var icon = ((Item)obj).Icon;
						if (icon != null)
						{
							var image = new Native.Image(parent)
							{
								MinimumWidth = Forms.ConvertToScaledPixel(24),
								MinimumHeight = Forms.ConvertToScaledPixel(24)
							};
							var result = image.LoadFromImageSourceAsync(icon);
							return image;
						}
						else
						{
							return null;
						}
					}
					else
					{
						return null;
					}
				}
			};
		}

		void UpdateMenu()
		{
			_menu.Clear();

			if (_groups != null)
			{
				for (int i = 0; i < _groups.Count; i++)
				{
					for (int j = 0; j < _groups[i].Items.Count; j++)
					{
						var item = _menu.Append(_defaultClass, _groups[i].Items[j]);
						if (j != 0)
						{
							item.SetPartColor("bottomline", EColor.Transparent);
						}

						item.SetPartColor("bg", EColor.Transparent);
					}
				}
			}
		}

		void UpdateChildGeometry()
		{
			int headerHeight = 0;
			if (_header != null)
			{
				headerHeight = _header.MinimumHeight;
				_header.Geometry = new Rect(Geometry.X, Geometry.Y, Geometry.Width, headerHeight);
			}
			_menu.Geometry = new Rect(Geometry.X, Geometry.Y + headerHeight, Geometry.Width, Geometry.Height - headerHeight);
		}
	}

	public class Item
	{
		public string Title { get; set; }

		public ImageSource Icon { get; set; }

		public Item(string title, ImageSource icon = null)
		{
			Title = title;
			Icon = icon;
		}
	}

	public class Group
	{
		public string Title { get; set; }

		public List<Item> Items { get; set; }

		public Group(List<Item> items, string title = null)
		{
			Items = items;
			Title = title;
		}
	}
}
