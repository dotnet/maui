using System;
using System.Collections.Generic;
using ElmSharp;
using EColor = ElmSharp.Color;

namespace Xamarin.Forms.Platform.Tizen
{
	public class NavigationView : Native.Box
	{
		EvasObject _header;
		GenList _menu;
		EColor _backgroundColor;
		EColor _defaultBackgroundColor = EColor.White;

		IList<Group> _groups;
		GenItemClass _defaultClass;

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

		public event EventHandler<GenListItemEventArgs> MenuItemSelected;

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

		public IList<Group> Menu
		{
			get
			{
				return _groups;
			}
			set
			{
				_groups = value;
				UpdateMenu();
			}
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
				MenuItemSelected?.Invoke(this, e);
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
							var image = new ElmSharp.Image(parent)
							{
								MinimumWidth = Forms.ConvertToScaledPixel(24),
								MinimumHeight = Forms.ConvertToScaledPixel(24)
							};
							var result = image.Load(ResourcePath.GetPath(icon));
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

		public string Icon { get; set; }

		public Item(string title, string icon = null)
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
