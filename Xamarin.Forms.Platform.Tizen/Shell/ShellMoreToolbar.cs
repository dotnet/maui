using System.Collections.Generic;
using ElmSharp;
using Xamarin.Forms.Platform.Tizen.Native;
using System.Linq;

namespace Xamarin.Forms.Platform.Tizen
{
	public class ShellMoreToolbar : GenList
	{
		ShellItemRenderer _shellItemRenderer = null;

		GenItemClass _defaultClass = null;
		Dictionary<ShellSection, GenListItem> _sectionToItem = new Dictionary<ShellSection, GenListItem>();
		LinkedList<ShellSection> _shellSectionList = new LinkedList<ShellSection>();

		const int _cellHeight = 100;
		const int _iconPadding = 30;
		const int _iconSize = 60;

		public ShellMoreToolbar(ShellItemRenderer renderer) : base(Forms.NativeParent)
		{
			_shellItemRenderer = renderer;

			Homogeneous = true;
			AlignmentX = -1;
			AlignmentY = -1;
			WeightX = 1;
			WeightY = 1;
			BackgroundColor = ShellRenderer.DefaultBackgroundColor.ToNative();
			ItemSelected += OnItemSelected;
			_defaultClass = new GenItemClass("full")
			{
				GetContentHandler = GetContent,
			};
		}

		public void AddItem(ShellSection section)
		{
			GenListItem item = Append(_defaultClass, section);
			if (item != null)
			{
				_sectionToItem[section] = item;
				_shellSectionList.AddLast(section);
			}
		}

		public void RemoveItem(ShellSection section)
		{
			if (_sectionToItem.ContainsKey(section))
			{
				GenListItem del = _sectionToItem[section];
				_sectionToItem.Remove(section);
				_shellSectionList.Remove(section);
				del.Delete();
			}
		}

		public ShellSection RemoveFirst()
		{
			ShellSection del = _shellSectionList.First();
			RemoveItem(del);
			return del;
		}

		public int Height
		{
			get
			{
				int height = _cellHeight * Count;
				int maxHeight = _shellItemRenderer.Control.Geometry.Height;
				return height <= maxHeight ? height : maxHeight;
			}
		}

		EvasObject GetContent(object data, string part)
		{
			ShellSection section = data as ShellSection;

			var box = new Native.Box(Forms.NativeParent);
			box.Show();

			var icon = new Native.Image(Forms.NativeParent)
			{
				MinimumWidth = Forms.ConvertToScaledPixel(44),
				MinimumHeight = Forms.ConvertToScaledPixel(27)
			};
			var task = icon.LoadFromImageSourceAsync(section.Icon);
			icon.Show();

			var title = new Native.Label(Forms.NativeParent)
			{
				Text = section.Title,
				FontSize = Forms.ConvertToEflFontPoint(14),
				HorizontalTextAlignment = Native.TextAlignment.Start,
				VerticalTextAlignment = Native.TextAlignment.Center
			};
			title.Show();

			box.PackEnd(icon);
			box.PackEnd(title);
			box.LayoutUpdated += (object sender, LayoutEventArgs e) =>
			{
				icon.Move(e.Geometry.X + _iconPadding, e.Geometry.Y + _iconPadding);
				icon.Resize(_iconSize, _iconSize);

				title.Move(e.Geometry.X + 2 * _iconPadding + _iconSize, e.Geometry.Y);
				title.Resize(e.Geometry.Width - (2 * _iconPadding + _iconSize), e.Geometry.Height);
			};
			box.MinimumHeight = _cellHeight;
			return box;
		}

		void OnItemSelected(object sender, GenListItemEventArgs e)
		{
			ShellSection section = e.Item.Data as ShellSection;
			_shellItemRenderer.SetCurrentItem(section);
		}
	}
}
