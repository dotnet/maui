using System;
using System.Collections.Generic;
using Microsoft.Maui.Layouts;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../docs/Microsoft.Maui.Controls/Grid.xml" path="Type[@FullName='Microsoft.Maui.Controls.Grid']/Docs/*" />
	[ContentProperty(nameof(Children))]
	public class Grid : Layout, IGridLayout
	{
		readonly Dictionary<IView, GridInfo> _viewInfo = new();

		/// <include file="../../../docs/Microsoft.Maui.Controls/Grid.xml" path="//Member[@MemberName='ColumnDefinitionsProperty']/Docs/*" />
		public static readonly BindableProperty ColumnDefinitionsProperty = BindableProperty.Create("ColumnDefinitions",
			typeof(ColumnDefinitionCollection), typeof(Grid), null, validateValue: (bindable, value) => value != null,
			propertyChanged: UpdateSizeChangedHandlers, defaultValueCreator: bindable =>
			{
				var colDef = new ColumnDefinitionCollection();
				colDef.ItemSizeChanged += ((Grid)bindable).DefinitionsChanged;
				return colDef;
			});

		/// <include file="../../../docs/Microsoft.Maui.Controls/Grid.xml" path="//Member[@MemberName='RowDefinitionsProperty']/Docs/*" />
		public static readonly BindableProperty RowDefinitionsProperty = BindableProperty.Create("RowDefinitions",
			typeof(RowDefinitionCollection), typeof(Grid), null, validateValue: (bindable, value) => value != null,
			propertyChanged: UpdateSizeChangedHandlers, defaultValueCreator: bindable =>
			{
				var rowDef = new RowDefinitionCollection();
				rowDef.ItemSizeChanged += ((Grid)bindable).DefinitionsChanged;
				return rowDef;
			});

		/// <include file="../../../docs/Microsoft.Maui.Controls/Grid.xml" path="//Member[@MemberName='RowSpacingProperty']/Docs/*" />
		public static readonly BindableProperty RowSpacingProperty = BindableProperty.Create("RowSpacing", typeof(double),
			typeof(Grid), 0d, propertyChanged: Invalidate);

		/// <include file="../../../docs/Microsoft.Maui.Controls/Grid.xml" path="//Member[@MemberName='ColumnSpacingProperty']/Docs/*" />
		public static readonly BindableProperty ColumnSpacingProperty = BindableProperty.Create("ColumnSpacing", typeof(double),
			typeof(Grid), 0d, propertyChanged: Invalidate);

		#region Row/Column/Span Attached Properties

		/// <include file="../../../docs/Microsoft.Maui.Controls/Grid.xml" path="//Member[@MemberName='RowProperty']/Docs/*" />
		public static readonly BindableProperty RowProperty = BindableProperty.CreateAttached("Row",
			typeof(int), typeof(Grid), default(int), validateValue: (bindable, value) => (int)value >= 0,
			propertyChanged: Invalidate);

		/// <include file="../../../docs/Microsoft.Maui.Controls/Grid.xml" path="//Member[@MemberName='RowSpanProperty']/Docs/*" />
		public static readonly BindableProperty RowSpanProperty = BindableProperty.CreateAttached("RowSpan",
			typeof(int), typeof(Grid), 1, validateValue: (bindable, value) => (int)value >= 1,
			propertyChanged: Invalidate);

		/// <include file="../../../docs/Microsoft.Maui.Controls/Grid.xml" path="//Member[@MemberName='ColumnProperty']/Docs/*" />
		public static readonly BindableProperty ColumnProperty = BindableProperty.CreateAttached("Column",
			typeof(int), typeof(Grid), default(int), validateValue: (bindable, value) => (int)value >= 0,
			propertyChanged: Invalidate);

		/// <include file="../../../docs/Microsoft.Maui.Controls/Grid.xml" path="//Member[@MemberName='ColumnSpanProperty']/Docs/*" />
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

		/// <include file="../../../docs/Microsoft.Maui.Controls/Grid.xml" path="//Member[@MemberName='ColumnDefinitions']/Docs/*" />
		[System.ComponentModel.TypeConverter(typeof(ColumnDefinitionCollectionTypeConverter))]
		public ColumnDefinitionCollection ColumnDefinitions
		{
			get { return (ColumnDefinitionCollection)GetValue(ColumnDefinitionsProperty); }
			set { SetValue(ColumnDefinitionsProperty, value); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/Grid.xml" path="//Member[@MemberName='RowDefinitions']/Docs/*" />
		[System.ComponentModel.TypeConverter(typeof(RowDefinitionCollectionTypeConverter))]
		public RowDefinitionCollection RowDefinitions
		{
			get { return (RowDefinitionCollection)GetValue(RowDefinitionsProperty); }
			set { SetValue(RowDefinitionsProperty, value); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/Grid.xml" path="//Member[@MemberName='RowSpacing']/Docs/*" />
		public double RowSpacing
		{
			get { return (double)GetValue(RowSpacingProperty); }
			set { SetValue(RowSpacingProperty, value); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/Grid.xml" path="//Member[@MemberName='ColumnSpacing']/Docs/*" />
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

		// These extra internal add methods are here to keep some other old stuff working until we re-add
		// the Grid convenience methods
		internal void Add(IView view, int left, int right, int top, int bottom)
		{
			if (view == null)
				throw new ArgumentNullException(nameof(view));
			if (left < 0)
				throw new ArgumentOutOfRangeException(nameof(left));
			if (top < 0)
				throw new ArgumentOutOfRangeException(nameof(top));
			if (left >= right)
				throw new ArgumentOutOfRangeException(nameof(right));
			if (top >= bottom)
				throw new ArgumentOutOfRangeException(nameof(bottom));

			SetRow(view, top);
			SetRowSpan(view, bottom - top);
			SetColumn(view, left);
			SetColumnSpan(view, right - left);

			Add(view);
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
