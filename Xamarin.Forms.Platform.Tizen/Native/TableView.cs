using ElmSharp;

namespace Xamarin.Forms.Platform.Tizen.Native
{
	/// <summary>
	/// Extends the ListView class to provide TableView class implementation.
	/// </summary>
	public class TableView : ListView, ITableView
	{

		static readonly SectionCellRenderer _sectionCellRenderer = new SectionCellRenderer();
		/// <summary>
		/// Initializes a new instance of the TableView class.
		/// </summary>
		public TableView(EvasObject parent)
			: base(parent)
		{
		}

		/// <summary>
		/// Sets the root of the table.
		/// </summary>
		/// <param name="root">TableRoot, which is parent to one or more TableSections.</param>
		public void ApplyTableRoot(TableRoot root)
		{
			Clear();
			foreach (TableSection ts in root)
			{
				if (!string.IsNullOrEmpty(ts.Title))
					AddSectionTitle(ts.Title, ts.TextColor);
				AddSource(ts);
			}
		}

		protected override CellRenderer GetCellRenderer(Cell cell, bool isGroup = false)
		{
			if (cell.GetType() == typeof(SectionCell))
			{
				return _sectionCellRenderer;
			}
			return base.GetCellRenderer(cell, isGroup);
		}

		/// <summary>
		/// Sets the section title.
		/// </summary>
		void AddSectionTitle(string title, Color textColor)
		{
			Cell cell = new SectionCell()
			{
				Text = title,
				TextColor = textColor
			};
			AddCell(cell);
		}

		internal class SectionCellRenderer : TextCellRenderer
		{
			public SectionCellRenderer() : this(ThemeConstants.GenItemClass.Styles.GroupIndex)
			{
			}

			protected SectionCellRenderer(string style) : base(style) { }
		}
		class SectionCell : TextCell
		{
		}
	}
}

