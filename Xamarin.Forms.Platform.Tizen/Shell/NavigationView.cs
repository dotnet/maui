using System;
using System.Collections.Generic;
using ElmSharp;
using EColor = ElmSharp.Color;
using EImage = ElmSharp.Image;
using NBox = Xamarin.Forms.Platform.Tizen.Native.Box;

namespace Xamarin.Forms.Platform.Tizen
{
	public class NavigationView : Background, INavigationView
	{
		NBox _box;
		EImage _bg;
		Aspect _bgImageAspect;
		ImageSource _bgImageSource;
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
			_box.LayoutUpdated += (s, e) =>
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
				base.BackgroundColor = effectiveColor;

				if(_bg == null)
				{
					_menu.BackgroundColor = effectiveColor;
				}
			}
		}

		public Aspect BackgroundImageAspect
		{
			get
			{
				return _bgImageAspect;
			}
			set
			{
				_bgImageAspect = value;
				if (_bg != null)
				{
					_bg.ApplyAspect(_bgImageAspect);
				}
			}
		}

		public ImageSource BackgroundImageSource
		{
			get
			{
				return _bgImageSource;
			}
			set
			{
				_bgImageSource = value;
				if (_bgImageSource != null)
				{
					if (_bg == null)
					{
						_bg = new EImage(this);
					}
					_menu.BackgroundColor = EColor.Transparent;
					SetPartContent("elm.swallow.background", _bg);
					_bg.ApplyAspect(_bgImageAspect);
					_ = _bg.LoadFromImageSourceAsync(_bgImageSource);
				}
				else
				{
					EColor effectiveColor = _backgroundColor.IsDefault ? _defaultBackgroundColor : _backgroundColor;
					_menu.BackgroundColor = effectiveColor;
					SetPartContent("elm.swallow.background", null);
				}
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
					_box.UnPack(_header);
					_header.Hide();
				}
				_header = value;

				if (_header != null)
				{
					_box.PackStart(_header);
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
			_box = new Native.Box(parent)
			{
				AlignmentX = -1,
				AlignmentY = -1,
				WeightX = 1,
				WeightY = 1
			};
			SetContent(_box);

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
			_box.PackEnd(_menu);

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
