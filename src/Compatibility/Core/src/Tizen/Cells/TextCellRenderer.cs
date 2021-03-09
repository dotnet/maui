using System.Collections.Generic;
using ElmSharp;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen
{
	public class TextCellRenderer : CellRenderer
	{
		bool _groupMode = false;
		// TextCell.Detail property is not supported on TV profile due to UX limitation.
		public TextCellRenderer() : this(ThemeManager.GetTextCellRendererStyle()) { }

		protected TextCellRenderer(string style) : base(style)
		{
			MainPart = this.GetMainPart();
			DetailPart = this.GetDetailPart();
		}

		protected string MainPart { get; set; }
		protected string DetailPart { get; set; }

		public override void SetGroupMode(bool enable)
		{
			if (_groupMode == enable)
				return;

			_groupMode = enable;
			Class = null;
			Style = ThemeManager.GetTextCellGroupModeStyle(enable);
			DetailPart = this.GetDetailPart();
		}

		protected override Span OnGetText(Cell cell, string part)
		{
			var textCell = (TextCell)cell;
			if (part == MainPart)
			{
				return OnMainText(textCell);
			}
			if (part == DetailPart)
			{
				return OnDetailText(textCell);
			}
			return null;
		}

		protected virtual Span OnMainText(TextCell cell)
		{
			return new Span()
			{
				Text = cell.Text,
				TextColor = cell.TextColor,
				FontSize = -1
			};
		}

		protected virtual Span OnDetailText(TextCell cell)
		{
			return new Span()
			{
				Text = cell.Detail,
				TextColor = cell.DetailColor,
				FontSize = -1
			};
		}

		protected override bool OnCellPropertyChanged(Cell cell, string property, Dictionary<string, EvasObject> realizedView)
		{
			if (property == TextCell.TextProperty.PropertyName ||
				property == TextCell.TextColorProperty.PropertyName ||
				property == TextCell.DetailProperty.PropertyName ||
				property == TextCell.DetailColorProperty.PropertyName)
			{
				return true;
			}
			return base.OnCellPropertyChanged(cell, property, realizedView);
		}
	}
}
