using ElmSharp;
using System.Collections.Generic;

namespace Xamarin.Forms.Platform.Tizen
{
	public class TextCellRenderer : CellRenderer
	{
		// TextCell.Detail property is not supported on TV profile due to UX limitation.
		public TextCellRenderer() : this( Device.Idiom == TargetIdiom.Phone ? "double_label" : "default") { }

		protected TextCellRenderer(string style) : base(style)
		{
			MainPart = "elm.text";
			DetailPart = "elm.text.sub";
		}

		protected string MainPart { get; set; }
		protected string DetailPart { get; set; }

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
				ForegroundColor = cell.TextColor,
				FontSize = -1
			};
		}

		protected virtual Span OnDetailText(TextCell cell)
		{
			return new Span()
			{
				Text = cell.Detail,
				ForegroundColor = cell.DetailColor,
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

	internal class GroupCellTextRenderer : TextCellRenderer
	{

		public GroupCellTextRenderer() : this("group_index")
		{
			DetailPart = "elm.text.end";
		}
		protected GroupCellTextRenderer(string style) : base(style) { }
	}
}
