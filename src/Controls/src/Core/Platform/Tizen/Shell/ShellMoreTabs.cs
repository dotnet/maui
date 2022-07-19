using ElmSharp;
using EBox = ElmSharp.Box;
using EImage = ElmSharp.Image;
using TImage = Tizen.UIExtensions.ElmSharp.Image;
using TLabel = Tizen.UIExtensions.ElmSharp.Label;
using TThemeConstants = Tizen.UIExtensions.ElmSharp.ThemeConstants;

namespace Microsoft.Maui.Controls.Platform
{
	public class ShellMoreTabs : GenList
	{
		GenItemClass _defaultClass = null;

		public ShellMoreTabs(EvasObject parent) : base(parent)
		{
			SetAlignment(-1, -1);
			SetWeight(1, 1);
			NativeParent = parent;

			Homogeneous = true;
			SelectionMode = GenItemSelectionMode.Always;
			BackgroundColor = TThemeConstants.Shell.ColorClass.DefaultBackgroundColor;
			_defaultClass = new GenItemClass(TThemeConstants.GenItemClass.Styles.Full)
			{
				GetContentHandler = GetContent,
			};
		}

		protected EvasObject NativeParent { get; private set; }

		public void AddItem(ShellSection section)
		{
			Append(_defaultClass, section);
		}

		public int HeightRequest
		{
			get
			{
				var cellHeight = this.GetDefaultIconSize() * 2 + this.GetDefaultIconSize();
				return DPExtensions.ConvertToScaledPixel(cellHeight) * Count;
			}
		}

		EvasObject GetContent(object data, string part)
		{
			ShellSection section = data as ShellSection;

			var box = new EBox(NativeParent);
			box.Show();

			EImage icon = null;
			if (section.Icon != null)
			{
				icon = new TImage(NativeParent);
				icon.Show();
				box.PackEnd(icon);
			}

			var title = new TLabel(NativeParent)
			{
				Text = section.Title,
				FontSize = DPExtensions.ConvertToEflFontPoint(14),
				HorizontalTextAlignment = Tizen.UIExtensions.Common.TextAlignment.Start,
				VerticalTextAlignment = Tizen.UIExtensions.Common.TextAlignment.Center,
			};
			title.Show();
			box.PackEnd(title);
			int iconPadding = DPExtensions.ConvertToScaledPixel(this.GetDefaultIconPadding());
			int iconSize = DPExtensions.ConvertToScaledPixel(this.GetDefaultIconSize());
			int cellHeight = iconPadding * 2 + iconSize;
			box.SetLayoutCallback(() =>
			{
				var bound = box.Geometry;
				int leftMargin = iconPadding;

				if (icon != null)
				{
					var iconBound = bound;
					iconBound.X += iconPadding;
					iconBound.Y += iconPadding;
					iconBound.Width = iconSize;
					iconBound.Height = iconSize;
					icon.Geometry = iconBound;
					leftMargin = (2 * iconPadding + iconSize);
				}

				bound.X += leftMargin;
				bound.Width -= leftMargin;
				title.Geometry = bound;
			});

			box.MinimumHeight = cellHeight;
			return box;
		}
	}
}
