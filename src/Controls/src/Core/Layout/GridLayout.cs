using System;
using System.Collections.Generic;
using Microsoft.Maui.Layouts;

namespace Microsoft.Maui.Controls
{
	[ContentProperty(nameof(Children))]
	public class GridLayout : Layout, IGridLayout
	{
		readonly Dictionary<IView, GridInfo> _viewInfo = new();

		public static readonly BindableProperty ColumnDefinitionsProperty = BindableProperty.Create("ColumnDefinitions",
			typeof(ColumnDefinitionCollection), typeof(GridLayout), null, validateValue: (bindable, value) => value != null,
			propertyChanged: UpdateSizeChangedHandlers, defaultValueCreator: bindable =>
			{
				var colDef = new ColumnDefinitionCollection();
				colDef.ItemSizeChanged += ((GridLayout)bindable).DefinitionsChanged;
				return colDef;
			});

		public static readonly BindableProperty RowDefinitionsProperty = BindableProperty.Create("RowDefinitions",
			typeof(RowDefinitionCollection), typeof(GridLayout), null, validateValue: (bindable, value) => value != null,
			propertyChanged: UpdateSizeChangedHandlers, defaultValueCreator: bindable =>
			{
				var rowDef = new RowDefinitionCollection();
				rowDef.ItemSizeChanged += ((GridLayout)bindable).DefinitionsChanged;
				return rowDef;
			});

		public static readonly BindableProperty RowSpacingProperty = BindableProperty.Create("RowSpacing", typeof(double),
			typeof(GridLayout), 0d, propertyChanged: Invalidate);

		public static readonly BindableProperty ColumnSpacingProperty = BindableProperty.Create("ColumnSpacing", typeof(double),
			typeof(GridLayout), 0d, propertyChanged: Invalidate);

		#region Row/Column/Span Attached Properties

		public static readonly BindableProperty RowProperty = BindableProperty.CreateAttached("Row",
			typeof(int), typeof(GridLayout), default(int), validateValue: (bindable, value) => (int)value >= 0,
			propertyChanged: Invalidate);

		public static readonly BindableProperty RowSpanProperty = BindableProperty.CreateAttached("RowSpan",
			typeof(int), typeof(GridLayout), 1, validateValue: (bindable, value) => (int)value >= 1,
			propertyChanged: Invalidate);

		public static readonly BindableProperty ColumnProperty = BindableProperty.CreateAttached("Column",
			typeof(int), typeof(GridLayout), default(int), validateValue: (bindable, value) => (int)value >= 0,
			propertyChanged: Invalidate);

		public static readonly BindableProperty ColumnSpanProperty = BindableProperty.CreateAttached("ColumnSpan",
			typeof(int), typeof(GridLayout), 1, validateValue: (bindable, value) => (int)value >= 1,
			propertyChanged: Invalidate);

		public static int GetColumn(BindableObject bindable)
		{
			return (int)bindable.GetValue(ColumnProperty);
		}

		public static int GetColumnSpan(BindableObject bindable)
		{
			return (int)bindable.GetValue(ColumnSpanProperty);
		}

		public static int GetRow(BindableObject bindable)
		{
			return (int)bindable.GetValue(RowProperty);
		}

		public static int GetRowSpan(BindableObject bindable)
		{
			return (int)bindable.GetValue(RowSpanProperty);
		}

		public static void SetColumn(BindableObject bindable, int value)
		{
			bindable.SetValue(ColumnProperty, value);
		}

		public static void SetColumnSpan(BindableObject bindable, int value)
		{
			bindable.SetValue(ColumnSpanProperty, value);
		}

		public static void SetRow(BindableObject bindable, int value)
		{
			bindable.SetValue(RowProperty, value);
		}

		public static void SetRowSpan(BindableObject bindable, int value)
		{
			bindable.SetValue(RowSpanProperty, value);
		}

		#endregion

		ReadOnlyCastingList<IGridRowDefinition, RowDefinition> _rowDefs;
		ReadOnlyCastingList<IGridColumnDefinition, ColumnDefinition> _colDefs;
		IReadOnlyList<IGridRowDefinition> IGridLayout.RowDefinitions => _rowDefs ??= new(RowDefinitions);
		IReadOnlyList<IGridColumnDefinition> IGridLayout.ColumnDefinitions => _colDefs ??= new(ColumnDefinitions);

		[System.ComponentModel.TypeConverter(typeof(ColumnDefinitionCollectionTypeConverter))]
		public ColumnDefinitionCollection ColumnDefinitions
		{
			get { return (ColumnDefinitionCollection)GetValue(ColumnDefinitionsProperty); }
			set { SetValue(ColumnDefinitionsProperty, value); }
		}

		[System.ComponentModel.TypeConverter(typeof(RowDefinitionCollectionTypeConverter))]
		public RowDefinitionCollection RowDefinitions
		{
			get { return (RowDefinitionCollection)GetValue(RowDefinitionsProperty); }
			set { SetValue(RowDefinitionsProperty, value); }
		}

		public double RowSpacing
		{
			get { return (double)GetValue(RowSpacingProperty); }
			set { SetValue(RowSpacingProperty, value); }
		}

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

		internal void Add(IView view, int left, int top)
		{
			if (view == null)
				throw new ArgumentNullException(nameof(view));
			if (left < 0)
				throw new ArgumentOutOfRangeException(nameof(left));
			if (top < 0)
				throw new ArgumentOutOfRangeException(nameof(top));

			Add(view, left, left + 1, top, top + 1);
		}

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
			var gridLayout = (GridLayout)bindable;

			if (oldValue is ColumnDefinitionCollection oldDefinition)
			{
				oldDefinition.ItemSizeChanged -= gridLayout.DefinitionsChanged;
			}

			if (newValue is ColumnDefinitionCollection newDefinition)
			{
				newDefinition.ItemSizeChanged += gridLayout.DefinitionsChanged;
			}

			gridLayout.DefinitionsChanged(bindable, EventArgs.Empty);
		}

		static void Invalidate(BindableObject bindable, object oldValue, object newValue)
		{
			if (bindable is Element element && element.Parent is GridLayout gridLayout)
			{
				gridLayout.InvalidateMeasure();
			}
		}

		void DefinitionsChanged(object sender, EventArgs args)
		{
			InvalidateMeasure();
		}

		protected override void InvalidateMeasure()
		{
			base.InvalidateMeasure();
			(this as IView)?.InvalidateMeasure();
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
