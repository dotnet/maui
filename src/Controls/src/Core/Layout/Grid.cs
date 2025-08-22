#nullable disable
using System;
using System.Collections.Generic;
using Microsoft.Maui.Layouts;
using Microsoft.Maui.Primitives;

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

		/// <include file="../../../docs/Microsoft.Maui.Controls/Grid.xml" path="//Member[@MemberName='GetColumn'][1]/Docs/*" />
		public static int GetColumn(BindableObject bindable)
		{
			return (int)bindable.GetValue(ColumnProperty);
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/Grid.xml" path="//Member[@MemberName='GetColumnSpan'][1]/Docs/*" />
		public static int GetColumnSpan(BindableObject bindable)
		{
			return (int)bindable.GetValue(ColumnSpanProperty);
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/Grid.xml" path="//Member[@MemberName='GetRow'][1]/Docs/*" />
		public static int GetRow(BindableObject bindable)
		{
			return (int)bindable.GetValue(RowProperty);
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/Grid.xml" path="//Member[@MemberName='GetRowSpan'][1]/Docs/*" />
		public static int GetRowSpan(BindableObject bindable)
		{
			return (int)bindable.GetValue(RowSpanProperty);
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/Grid.xml" path="//Member[@MemberName='SetColumn'][1]/Docs/*" />
		public static void SetColumn(BindableObject bindable, int value)
		{
			bindable.SetValue(ColumnProperty, value);
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/Grid.xml" path="//Member[@MemberName='SetColumnSpan'][1]/Docs/*" />
		public static void SetColumnSpan(BindableObject bindable, int value)
		{
			bindable.SetValue(ColumnSpanProperty, value);
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/Grid.xml" path="//Member[@MemberName='SetRow'][1]/Docs/*" />
		public static void SetRow(BindableObject bindable, int value)
		{
			bindable.SetValue(RowProperty, value);
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/Grid.xml" path="//Member[@MemberName='SetRowSpan'][1]/Docs/*" />
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
		/// <remarks>RowDefinitions is an ordered set of</remarks>
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

		public int GetColumn(IView view)
		{
			return view switch
			{
				BindableObject bo => (int)bo.GetValue(ColumnProperty),
				_ => _viewInfo[view].Col,
			};
		}

		public int GetColumnSpan(IView view)
		{
			return view switch
			{
				BindableObject bo => (int)bo.GetValue(ColumnSpanProperty),
				_ => _viewInfo[view].ColSpan,
			};
		}

		public int GetRow(IView view)
		{
			return view switch
			{
				BindableObject bo => (int)bo.GetValue(RowProperty),
				_ => _viewInfo[view].Row,
			};
		}

		public int GetRowSpan(IView view)
		{
			return view switch
			{
				BindableObject bo => (int)bo.GetValue(RowSpanProperty),
				_ => _viewInfo[view].RowSpan,
			};
		}

		public void AddRowDefinition(RowDefinition gridRowDefinition)
		{
			RowDefinitions.Add(gridRowDefinition);
		}

		public void AddColumnDefinition(ColumnDefinition gridColumnDefinition)
		{
			ColumnDefinitions.Add(gridColumnDefinition);
		}

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

		protected override SizeConstraint ComputeConstraintForView(View view)
		{
			var result = SizeConstraint.None;

			if (view.VerticalOptions.Alignment == LayoutAlignment.Fill && ViewHasFixedHeightDefinition(view))
			{
				result |= SizeConstraint.VerticallyFixed;
			}

			if (view.HorizontalOptions.Alignment == LayoutAlignment.Fill && ViewHasFixedWidthDefinition(view))
			{
				result |= SizeConstraint.HorizontallyFixed;
			}

			return result;
		}

		bool ViewHasFixedHeightDefinition(BindableObject view)
		{
			var gridHasFixedHeight = (Constraint & SizeConstraint.VerticallyFixed) != 0;

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
			var gridHasFixedWidth = (Constraint & SizeConstraint.HorizontallyFixed) != 0;

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
