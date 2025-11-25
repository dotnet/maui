#nullable disable
using System;
using System.Collections.Generic;
using Microsoft.Maui.Layouts;

namespace Microsoft.Maui.Controls
{
	/// <summary>A layout that arranges views in rows and columns.</summary>
	[ContentProperty(nameof(Children))]
	public class Grid : Layout, IGridLayout
	{
		static readonly ColumnDefinitionCollection DefaultColumnDefinitions = new(new ColumnDefinition { Width = GridLength.Star });
		static readonly RowDefinitionCollection DefaultRowDefinitions = new(new RowDefinition { Height = GridLength.Star });

		readonly Dictionary<IView, GridInfo> _viewInfo = new();

		/// <summary>Bindable property for <see cref="ColumnDefinitions"/>.</summary>
		public static readonly BindableProperty ColumnDefinitionsProperty = BindableProperty.Create(nameof(ColumnDefinitions),
			typeof(ColumnDefinitionCollection), typeof(Grid), null, validateValue: (bindable, value) => value != null,
			propertyChanged: UpdateSizeChangedHandlers, defaultValueCreator: bindable =>
			{
				var colDef = new ColumnDefinitionCollection();
				colDef.ItemSizeChanged += ((Grid)bindable).DefinitionsChanged;
				return colDef;
			});

		/// <summary>Bindable property for <see cref="RowDefinitions"/>.</summary>
		public static readonly BindableProperty RowDefinitionsProperty = BindableProperty.Create(nameof(RowDefinitions),
			typeof(RowDefinitionCollection), typeof(Grid), null, validateValue: (bindable, value) => value != null,
			propertyChanged: UpdateSizeChangedHandlers, defaultValueCreator: bindable =>
			{
				var rowDef = new RowDefinitionCollection();
				rowDef.ItemSizeChanged += ((Grid)bindable).DefinitionsChanged;
				return rowDef;
			});

		/// <summary>Bindable property for <see cref="RowSpacing"/>.</summary>
		public static readonly BindableProperty RowSpacingProperty = BindableProperty.Create(nameof(RowSpacing), typeof(double),
			typeof(Grid), 0d, propertyChanged: Invalidate);

		/// <summary>Bindable property for <see cref="ColumnSpacing"/>.</summary>
		public static readonly BindableProperty ColumnSpacingProperty = BindableProperty.Create(nameof(ColumnSpacing), typeof(double),
			typeof(Grid), 0d, propertyChanged: Invalidate);

		#region Row/Column/Span Attached Properties

		/// <summary>Bindable property for attached property <c>Row</c>.</summary>
		public static readonly BindableProperty RowProperty = BindableProperty.CreateAttached("Row",
			typeof(int), typeof(Grid), default(int), validateValue: (bindable, value) => (int)value >= 0,
			propertyChanged: Invalidate);

		/// <summary>Bindable property for attached property <c>RowSpan</c>.</summary>
		public static readonly BindableProperty RowSpanProperty = BindableProperty.CreateAttached("RowSpan",
			typeof(int), typeof(Grid), 1, validateValue: (bindable, value) => (int)value >= 1,
			propertyChanged: Invalidate);

		/// <summary>Bindable property for attached property <c>Column</c>.</summary>
		public static readonly BindableProperty ColumnProperty = BindableProperty.CreateAttached("Column",
			typeof(int), typeof(Grid), default(int), validateValue: (bindable, value) => (int)value >= 0,
			propertyChanged: Invalidate);

		/// <summary>Bindable property for attached property <c>ColumnSpan</c>.</summary>
		public static readonly BindableProperty ColumnSpanProperty = BindableProperty.CreateAttached("ColumnSpan",
			typeof(int), typeof(Grid), 1, validateValue: (bindable, value) => (int)value >= 1,
			propertyChanged: Invalidate);

		/// <summary>Gets the column of the <paramref name="bindable" /> child element.</summary>
		/// <param name="bindable">An element that belongs to the Grid layout.</param>
		/// <returns>The column that the child element is in.</returns>
		public static int GetColumn(BindableObject bindable)
		{
			return (int)bindable.GetValue(ColumnProperty);
		}

		/// <returns>The number of columns spanned by the element; defaults to 1.</returns>
		public static int GetColumnSpan(BindableObject bindable)
		{
			return (int)bindable.GetValue(ColumnSpanProperty);
		}

		/// <summary>Gets the row of the <paramref name="bindable" /> child element.</summary>
		/// <param name="bindable">An element that belongs to the Grid layout.</param>
		/// <returns>The row that the child element is in.</returns>
		public static int GetRow(BindableObject bindable)
		{
			return (int)bindable.GetValue(RowProperty);
		}

		/// <summary>Gets the row span of the <paramref name="bindable" /> child element.</summary>
		/// <param name="bindable">An element that belongs to the Grid layout.</param>
		/// <returns>The row span value of the given element.</returns>
		public static int GetRowSpan(BindableObject bindable)
		{
			return (int)bindable.GetValue(RowSpanProperty);
		}

		/// <summary>Changes the column in which a child element will be placed.</summary>
		/// <param name="bindable">A child element of this Grid to move to a different column.</param>
		/// <param name="value">The column in which to place the child element.</param>
		public static void SetColumn(BindableObject bindable, int value)
		{
			bindable.SetValue(ColumnProperty, value);
		}

		/// <summary>Changes the column span of the specified child element.</summary>
		/// <param name="bindable">A child element of this Grid on which to assign a new column span.</param>
		/// <param name="value">The new column span.</param>
		public static void SetColumnSpan(BindableObject bindable, int value)
		{
			bindable.SetValue(ColumnSpanProperty, value);
		}

		/// <summary>Changes the row in which a child element will be placed.</summary>
		/// <param name="bindable">A child element of this Grid to move to a different row.</param>
		/// <param name="value">The row in which to place the child element.</param>
		public static void SetRow(BindableObject bindable, int value)
		{
			bindable.SetValue(RowProperty, value);
		}

		/// <summary>Changes the row span of the specified child element.</summary>
		/// <param name="bindable">A child element of this Grid on which to assign a new row span.</param>
		/// <param name="value">The new row span.</param>
		public static void SetRowSpan(BindableObject bindable, int value)
		{
			bindable.SetValue(RowSpanProperty, value);
		}

		#endregion

		ReadOnlyCastingList<IGridRowDefinition, RowDefinition> _rowDefs;
		ReadOnlyCastingList<IGridColumnDefinition, ColumnDefinition> _colDefs;
		IReadOnlyList<IGridRowDefinition> IGridLayout.RowDefinitions => _rowDefs ??= new(RowDefinitions);
		IReadOnlyList<IGridColumnDefinition> IGridLayout.ColumnDefinitions => _colDefs ??= new(ColumnDefinitions);

		/// <summary>Provides the interface for the bound property that gets or sets the ordered collection of <see cref="Microsoft.Maui.Controls.ColumnDefinition"/> objects that control the layout of columns in the <see cref="Microsoft.Maui.Controls.Grid"/>.</summary>
		[System.ComponentModel.TypeConverter(typeof(ColumnDefinitionCollectionTypeConverter))]
		public ColumnDefinitionCollection ColumnDefinitions
		{
			get { return (ColumnDefinitionCollection)GetValue(ColumnDefinitionsProperty); }
			set { SetValue(ColumnDefinitionsProperty, value); }
		}

		/// <summary>Provides the interface for the bound property that gets or sets the collection of RowDefinition objects that control the heights of each row.</summary>
		/// <remarks>
		/// <see cref="RowDefinition.Height" /> is set to <see cref="Microsoft.Maui.GridLength.Star" />.
		/// </remarks>
		[System.ComponentModel.TypeConverter(typeof(RowDefinitionCollectionTypeConverter))]
		public RowDefinitionCollection RowDefinitions
		{
			get { return (RowDefinitionCollection)GetValue(RowDefinitionsProperty); }
			set { SetValue(RowDefinitionsProperty, value); }
		}

		/// <summary>Gets or sets the amount of space between rows in the Grid. This is a bindable property.</summary>
		public double RowSpacing
		{
			get { return (double)GetValue(RowSpacingProperty); }
			set { SetValue(RowSpacingProperty, value); }
		}

		/// <summary>Gets or sets the amount of space between columns in the Grid. This is a bindable property.</summary>
		public double ColumnSpacing
		{
			get { return (double)GetValue(ColumnSpacingProperty); }
			set { SetValue(ColumnSpacingProperty, value); }
		}

		/// <summary>
		/// Gets the zero-based column index for the provided <paramref name="view" />.
		/// </summary>
		/// <param name="view">The child view whose column index to retrieve. Can be a <see cref="BindableObject" /> or a virtual (non-bindable) view.</param>
		/// <returns>The zero-based column index; defaults to 0 for views that have not been positioned explicitly.</returns>
		/// <exception cref="KeyNotFoundException">Thrown if <paramref name="view" /> is a non-bindable view that has not been added to the grid.</exception>
		public int GetColumn(IView view)
		{
			return view switch
			{
				BindableObject bo => (int)bo.GetValue(ColumnProperty),
				_ => _viewInfo[view].Col,
			};
		}

		/// <summary>
		/// Gets the number of columns spanned by the provided <paramref name="view" />.
		/// </summary>
		/// <param name="view">The child view whose column span to retrieve.</param>
		/// <returns>The column span; defaults to 1.</returns>
		/// <exception cref="KeyNotFoundException">Thrown if <paramref name="view" /> is a non-bindable view that has not been added to the grid.</exception>
		public int GetColumnSpan(IView view)
		{
			return view switch
			{
				BindableObject bo => (int)bo.GetValue(ColumnSpanProperty),
				_ => _viewInfo[view].ColSpan,
			};
		}

		/// <summary>
		/// Gets the zero-based row index for the provided <paramref name="view" />.
		/// </summary>
		/// <param name="view">The child view whose row index to retrieve.</param>
		/// <returns>The zero-based row index; defaults to 0.</returns>
		/// <exception cref="KeyNotFoundException">Thrown if <paramref name="view" /> is a non-bindable view that has not been added to the grid.</exception>
		public int GetRow(IView view)
		{
			return view switch
			{
				BindableObject bo => (int)bo.GetValue(RowProperty),
				_ => _viewInfo[view].Row,
			};
		}

		/// <summary>
		/// Gets the number of rows spanned by the provided <paramref name="view" />.
		/// </summary>
		/// <param name="view">The child view whose row span to retrieve.</param>
		/// <returns>The row span; defaults to 1.</returns>
		/// <exception cref="KeyNotFoundException">Thrown if <paramref name="view" /> is a non-bindable view that has not been added to the grid.</exception>
		public int GetRowSpan(IView view)
		{
			return view switch
			{
				BindableObject bo => (int)bo.GetValue(RowSpanProperty),
				_ => _viewInfo[view].RowSpan,
			};
		}

		/// <summary>
		/// Adds a <see cref="RowDefinition" /> to the <see cref="RowDefinitions" /> collection.
		/// </summary>
		/// <param name="gridRowDefinition">The row definition to add.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="gridRowDefinition" /> is <c>null</c>.</exception>
		public void AddRowDefinition(RowDefinition gridRowDefinition)
		{
			RowDefinitions.Add(gridRowDefinition);
		}

		/// <summary>
		/// Adds a <see cref="ColumnDefinition" /> to the <see cref="ColumnDefinitions" /> collection.
		/// </summary>
		/// <param name="gridColumnDefinition">The column definition to add.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="gridColumnDefinition" /> is <c>null</c>.</exception>
		public void AddColumnDefinition(ColumnDefinition gridColumnDefinition)
		{
			ColumnDefinitions.Add(gridColumnDefinition);
		}

		/// <summary>
		/// Sets the zero-based row index for the specified <paramref name="view" />.
		/// </summary>
		/// <param name="view">The child view to position.</param>
		/// <param name="row">The zero-based row index (must be &gt;= 0).</param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="row" /> is negative.</exception>
		/// <exception cref="KeyNotFoundException">Thrown if <paramref name="view" /> is a non-bindable view that has not been added to the grid.</exception>
		public void SetRow(IView view, int row)
		{
			switch (view)
			{
				case BindableObject bo:
					bo.SetValue(RowProperty, row);
					break;
				default:
					_viewInfo[view].Row = row;
					InvalidateMeasure();
					break;
			}
		}

		/// <summary>
		/// Sets the number of rows spanned by the specified <paramref name="view" />.
		/// </summary>
		/// <param name="view">The child view to modify.</param>
		/// <param name="span">The span (must be &gt;= 1).</param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="span" /> is less than 1.</exception>
		/// <exception cref="KeyNotFoundException">Thrown if <paramref name="view" /> is a non-bindable view that has not been added to the grid.</exception>
		public void SetRowSpan(IView view, int span)
		{
			switch (view)
			{
				case BindableObject bo:
					bo.SetValue(RowSpanProperty, span);
					break;
				default:
					_viewInfo[view].RowSpan = span;
					InvalidateMeasure();
					break;
			}
		}

		/// <summary>
		/// Sets the zero-based column index for the specified <paramref name="view" />.
		/// </summary>
		/// <param name="view">The child view to position.</param>
		/// <param name="col">The zero-based column index (must be &gt;= 0).</param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="col" /> is negative.</exception>
		/// <exception cref="KeyNotFoundException">Thrown if <paramref name="view" /> is a non-bindable view that has not been added to the grid.</exception>
		public void SetColumn(IView view, int col)
		{
			switch (view)
			{
				case BindableObject bo:
					bo.SetValue(ColumnProperty, col);
					break;
				default:
					_viewInfo[view].Col = col;
					InvalidateMeasure();
					break;
			}
		}

		/// <summary>
		/// Sets the number of columns spanned by the specified <paramref name="view" />.
		/// </summary>
		/// <param name="view">The child view to modify.</param>
		/// <param name="span">The span (must be &gt;= 1).</param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="span" /> is less than 1.</exception>
		/// <exception cref="KeyNotFoundException">Thrown if <paramref name="view" /> is a non-bindable view that has not been added to the grid.</exception>
		public void SetColumnSpan(IView view, int span)
		{
			switch (view)
			{
				case BindableObject bo:
					bo.SetValue(ColumnSpanProperty, span);
					break;
				default:
					_viewInfo[view].ColSpan = span;
					InvalidateMeasure();
					break;
			}
		}

		protected override void OnAdd(int index, IView view)
		{
			if (view is not BindableObject)
			{
				_viewInfo[view] = new GridInfo();
			}

			base.OnAdd(index, view);
		}

		protected override void OnClear()
		{
			_viewInfo.Clear();
			base.OnClear();
		}

		protected override void OnRemove(int index, IView view)
		{
			_viewInfo.Remove(view);
			base.OnRemove(index, view);
		}

		protected override void OnInsert(int index, IView view)
		{
			if (view is not BindableObject)
			{
				_viewInfo[view] = new GridInfo();
			}

			base.OnInsert(index, view);
		}

		protected override void OnUpdate(int index, IView view, IView oldView)
		{
			_viewInfo.Remove(oldView);

			if (view is not BindableObject)
			{
				_viewInfo[view] = new GridInfo();
			}

			base.OnUpdate(index, view, oldView);
		}

		protected override ILayoutManager CreateLayoutManager() => new GridLayoutManager(this);

		static void UpdateSizeChangedHandlers(BindableObject bindable, object oldValue, object newValue)
		{
			var gridLayout = (Grid)bindable;

			if (oldValue is ColumnDefinitionCollection oldColDefs)
			{
				oldColDefs.ItemSizeChanged -= gridLayout.DefinitionsChanged;
			}
			else if (oldValue is RowDefinitionCollection oldRowDefs)
			{
				oldRowDefs.ItemSizeChanged -= gridLayout.DefinitionsChanged;
			}

			if (newValue is ColumnDefinitionCollection newColDefs)
			{
				newColDefs.ItemSizeChanged += gridLayout.DefinitionsChanged;
			}
			else if (newValue is RowDefinitionCollection newRowDefs)
			{
				newRowDefs.ItemSizeChanged += gridLayout.DefinitionsChanged;
			}

			gridLayout.DefinitionsChanged(bindable, EventArgs.Empty);
		}

		static void Invalidate(BindableObject bindable, object oldValue, object newValue)
		{
			if (bindable is Grid grid)
			{
				grid.InvalidateMeasure();
			}
			else if (bindable is Element element && element.Parent is Grid parentGrid)
			{
				parentGrid.InvalidateMeasure();
			}
		}

		void DefinitionsChanged(object sender, EventArgs args)
		{
			// Clear out the IGridLayout row/col defs; they'll be set up again next time they're accessed
			_rowDefs = null;
			_colDefs = null;

			UpdateRowColumnBindingContexts();

			InvalidateMeasure();
		}

		protected override void InvalidateMeasure()
		{
			base.InvalidateMeasure();
			(this as IView)?.InvalidateMeasure();
		}

		protected override void OnBindingContextChanged()
		{
			base.OnBindingContextChanged();
			UpdateRowColumnBindingContexts();
		}

		protected override LayoutConstraint ComputeConstraintForView(View view)
		{
			var result = LayoutConstraint.None;

			if (view.VerticalOptions.Alignment == LayoutAlignment.Fill && ViewHasFixedHeightDefinition(view))
			{
				result |= LayoutConstraint.VerticallyFixed;
			}

			if (view.HorizontalOptions.Alignment == LayoutAlignment.Fill && ViewHasFixedWidthDefinition(view))
			{
				result |= LayoutConstraint.HorizontallyFixed;
			}

			return result;
		}

		bool ViewHasFixedHeightDefinition(BindableObject view)
		{
			var gridHasFixedHeight = (Constraint & LayoutConstraint.VerticallyFixed) != 0;

			var row = GetRow(view);
			var rowSpan = GetRowSpan(view);
			var rowDefinitions = RowDefinitions;
			if (rowDefinitions?.Count is not > 0)
			{
				rowDefinitions = DefaultRowDefinitions;
			}

			for (int i = row; i < row + rowSpan && i < rowDefinitions.Count; i++)
			{
				GridLength height = rowDefinitions[i].Height;

				if (height.IsAuto)
				{
					return false;
				}

				if (!gridHasFixedHeight && height.IsStar)
				{
					return false;
				}
			}

			return true;
		}

		bool ViewHasFixedWidthDefinition(BindableObject view)
		{
			var gridHasFixedWidth = (Constraint & LayoutConstraint.HorizontallyFixed) != 0;

			var col = GetColumn(view);
			var colSpan = GetColumnSpan(view);
			var columnDefinitions = ColumnDefinitions;
			if (columnDefinitions?.Count is not > 0)
			{
				columnDefinitions = DefaultColumnDefinitions;
			}

			for (int i = col; i < col + colSpan && i < columnDefinitions.Count; i++)
			{
				GridLength width = columnDefinitions[i].Width;

				if (width.IsAuto)
				{
					return false;
				}

				if (!gridHasFixedWidth && width.IsStar)
				{
					return false;
				}
			}

			return true;
		}

		void UpdateRowColumnBindingContexts()
		{
			var bindingContext = BindingContext;

			RowDefinitionCollection rowDefs = RowDefinitions;
			for (var i = 0; i < rowDefs.Count; i++)
			{
				SetInheritedBindingContext(rowDefs[i], bindingContext);
			}

			ColumnDefinitionCollection colDefs = ColumnDefinitions;
			for (var i = 0; i < colDefs.Count; i++)
			{
				SetInheritedBindingContext(colDefs[i], bindingContext);
			}
		}

		class GridInfo
		{
			public int Row { get; set; }
			public int Col { get; set; }
			public int RowSpan { get; set; } = 1;
			public int ColSpan { get; set; } = 1;
		}
	}
}
