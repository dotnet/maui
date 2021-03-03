using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using ElmSharp;
using ElmSharp.Wearable;
using EColor = ElmSharp.Color;
using ELayout = ElmSharp.Layout;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen.Watch
{
	public class NavigationView : ELayout
	{
		readonly int _defaultIconSize = ThemeConstants.Shell.Resources.Watch.DefaultNavigationViewIconSize;

		class Item : INotifyPropertyChanged
		{
			Element _source;
			public Element Source
			{
				get
				{
					return _source;
				}
				set
				{
					if (_source != null)
					{
						_source.PropertyChanged -= OnElementPropertyChanged;
					}
					_source = value;
					_source.PropertyChanged += OnElementPropertyChanged;
					UpdateContent();
				}
			}

			public string Text { get; set; }
			public string Icon { get; set; }

			public event PropertyChangedEventHandler PropertyChanged;

			void UpdateContent()
			{
				if (Source is BaseShellItem shellItem)
				{
					Text = shellItem.Title;
					Icon = (shellItem.Icon as FileImageSource)?.ToAbsPath();
				}
				else if (Source is MenuItem menuItem)
				{
					Text = menuItem.Text;
					Icon = (menuItem.IconImageSource as FileImageSource)?.ToAbsPath();
				}
				else
				{
					Text = null;
					Icon = null;
				}
				SendPropertyChanged();
			}

			void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
			{
				UpdateContent();
			}

			void SendPropertyChanged([CallerMemberName] string propertyName = "")
			{
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		Box _outterBox;
		ELayout _surfaceLayout;
		CircleSurface _surface;
		CircleGenList _naviMenu;

		GenItemClass _defaultClass;
		SmartEvent _draggedUpCallback;
		SmartEvent _draggedDownCallback;

		GenListItem _header;
		GenListItem _footer;

		List<List<Element>> _itemCache;
		List<GenListItem> _items = new List<GenListItem>();

		public NavigationView(EvasObject parent) : base(parent)
		{
			InitializeComponent();
		}

		public event EventHandler<SelectedItemChangedEventArgs> ItemSelected;

		public event EventHandler<DraggedEventArgs> Dragged;


		EColor _backgroundColor = ThemeConstants.Shell.ColorClass.Watch.DefaultNavigationViewBackgroundColor;
		public override EColor BackgroundColor
		{
			get => _backgroundColor;
			set
			{
				_backgroundColor = value.IsDefault ? ThemeConstants.Shell.ColorClass.Watch.DefaultNavigationViewBackgroundColor : value;
				UpdateBackgroundColor();
			}
		}

		EColor _foregroundColor = ThemeConstants.Shell.ColorClass.Watch.DefaultNavigationViewForegroundColor;
		public EColor ForegroundColor
		{
			get => _foregroundColor;
			set
			{
				_foregroundColor = value;
				UpdateForegroundColor();
			}
		}


		public void Build(List<List<Element>> items)
		{
			// Only update when items was changed
			if (!IsUpdated(items))
			{
				return;
			}
			_itemCache = items;

			ClearItemPropertyChangedHandler();
			_naviMenu.Clear();
			_items.Clear();
			// header
			_header = _naviMenu.Append(_defaultClass, new Item { Text = "" });

			// TODO. need to improve, need to support group
			foreach (var group in items)
			{
				foreach (var item in group)
				{
					var data = new Item
					{
						Source = item
					};
					if (item is BaseShellItem shellItem)
					{
						data.Text = shellItem.Title;
						data.Icon = (shellItem.Icon as FileImageSource)?.ToAbsPath();
					}
					else if (item is MenuItem menuItem)
					{
						data.Text = menuItem.Text;
						data.Icon = (menuItem.IconImageSource as FileImageSource)?.ToAbsPath();
					}
					var genitem = _naviMenu.Append(_defaultClass, data, GenListItemType.Normal);
					_items.Add(genitem);
					data.PropertyChanged += OnItemPropertyChanged;
				}
			}
			_footer = _naviMenu.Append(_defaultClass, new Item { Text = "" });
		}

		public void Activate()
		{
			(_naviMenu as IRotaryActionWidget)?.Activate();
		}
		public void Deactivate()
		{
			(_naviMenu as IRotaryActionWidget)?.Deactivate();
		}

		protected override IntPtr CreateHandle(EvasObject parent)
		{
			_outterBox = new Box(parent);
			return _outterBox.Handle;
		}

		void InitializeComponent()
		{
			_outterBox.SetLayoutCallback(OnLayout);

			_surfaceLayout = new ELayout(this);
			_surfaceLayout.Show();
			_surface = new CircleSurface(_surfaceLayout);

			_naviMenu = new CircleGenList(this, _surface)
			{
				Homogeneous = true,
				BackgroundColor = _backgroundColor
			};
			_naviMenu.Show();

			_draggedUpCallback = new SmartEvent(_naviMenu, "drag,start,up");
			_draggedUpCallback.On += (s, e) =>
			{
				if (_footer.TrackObject.IsVisible)
				{
					Dragged?.Invoke(this, new DraggedEventArgs(DraggedState.EdgeBottom));
				}
				else
				{
					Dragged?.Invoke(this, new DraggedEventArgs(DraggedState.Up));
				}
			};

			_draggedDownCallback = new SmartEvent(_naviMenu, "drag,start,down");
			_draggedDownCallback.On += (s, e) =>
			{
				if (_header.TrackObject.IsVisible)
				{
					Dragged?.Invoke(this, new DraggedEventArgs(DraggedState.EdgeTop));
				}
				else
				{
					Dragged?.Invoke(this, new DraggedEventArgs(DraggedState.Down));
				}
			};

			_outterBox.PackEnd(_naviMenu);
			_outterBox.PackEnd(_surfaceLayout);

			_surfaceLayout.StackAbove(_naviMenu);

			_defaultClass = new GenItemClass(ThemeConstants.GenItemClass.Styles.Watch.Text1Icon1)
			{
				GetTextHandler = (obj, part) =>
				{
					if (part == ThemeConstants.GenItemClass.Parts.Text)
					{
						var text = (obj as Item).Text;
						if (_foregroundColor != EColor.Default)
							return $"<span color='{_foregroundColor.ToHex()}'>{text}</span>";
						else
							return text;
					}
					return null;
				},
				GetContentHandler = (obj, part) =>
				{
					if (part == ThemeConstants.GenItemClass.Parts.Watch.Icon && obj is Item menuItem && !string.IsNullOrEmpty(menuItem.Icon))
					{
						var icon = new ElmSharp.Image(Microsoft.Maui.Controls.Compatibility.Forms.NativeParent)
						{
							AlignmentX = -1,
							AlignmentY = -1,
							WeightX = 1.0,
							WeightY = 1.0,
							MinimumWidth = _defaultIconSize,
							MinimumHeight = _defaultIconSize,
						};
						icon.Show();
						icon.Load(menuItem.Icon);
						return icon;
					}
					return null;
				}
			};

			_naviMenu.ItemSelected += OnItemSelected;

		}

		void OnItemSelected(object sender, GenListItemEventArgs e)
		{
			ItemSelected?.Invoke(this, new SelectedItemChangedEventArgs((e.Item.Data as Item).Source, -1));
		}

		void OnLayout()
		{
			_surfaceLayout.Geometry = Geometry;
			_naviMenu.Geometry = Geometry;
		}

		void UpdateBackgroundColor()
		{
			_naviMenu.BackgroundColor = _backgroundColor;
		}

		void UpdateForegroundColor()
		{
			foreach (var item in _items)
			{
				item.Update();
			}
		}

		bool IsUpdated(List<List<Element>> items)
		{
			if (_itemCache == null)
				return true;

			if (_itemCache.Count != items.Count)
				return true;

			for (int i = 0; i < items.Count; i++)
			{
				if (_itemCache[i].Count != items[i].Count)
					return true;

				for (int j = 0; j < items[i].Count; j++)
				{
					if (_itemCache[i][j] != items[i][j])
						return true;
				}
			}
			return false;
		}

		void ClearItemPropertyChangedHandler()
		{
			foreach (var item in _items)
			{
				(item.Data as Item).PropertyChanged -= OnItemPropertyChanged;
			}
		}

		void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			var item = _items.Where((d) => d.Data == sender).FirstOrDefault();
			item?.Update();
		}

	}
	public enum DraggedState
	{
		EdgeTop,
		Up,
		Down,
		EdgeBottom,
	}

	public class DraggedEventArgs
	{
		public DraggedState State { get; private set; }

		public DraggedEventArgs(DraggedState state)
		{
			State = state;
		}
	}

	static class FileImageSourceEX
	{
		public static string ToAbsPath(this FileImageSource source)
		{
			return ResourcePath.GetPath(source.File);
		}
	}

	static class ColorEX
	{
		public static string ToHex(this EColor c)
		{
			return string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", c.R, c.G, c.B, c.A);
		}
	}
}
