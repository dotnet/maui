#nullable disable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Microsoft.Maui.Controls.Internals;


namespace Microsoft.Maui.Controls.Compatibility
{
	[ContentProperty(nameof(Children))]
	[Obsolete("Use Microsoft.Maui.Controls.Grid instead. For more information, see https://learn.microsoft.com/dotnet/maui/migration/layouts")]
	public partial class Grid : Layout<View>, IGridController, IElementConfiguration<Grid>, IGridLayout
	{
		/// <summary>Bindable property for attached property <c>Row</c>.</summary>
		public static readonly BindableProperty RowProperty = BindableProperty.CreateAttached("Row", typeof(int), typeof(Grid), default(int), validateValue: (bindable, value) => (int)value >= 0);

		/// <summary>Bindable property for attached property <c>RowSpan</c>.</summary>
		public static readonly BindableProperty RowSpanProperty = BindableProperty.CreateAttached("RowSpan", typeof(int), typeof(Grid), 1, validateValue: (bindable, value) => (int)value >= 1);

		/// <summary>Bindable property for attached property <c>Column</c>.</summary>
		public static readonly BindableProperty ColumnProperty = BindableProperty.CreateAttached("Column", typeof(int), typeof(Grid), default(int), validateValue: (bindable, value) => (int)value >= 0);

		/// <summary>Bindable property for attached property <c>ColumnSpan</c>.</summary>
		public static readonly BindableProperty ColumnSpanProperty = BindableProperty.CreateAttached("ColumnSpan", typeof(int), typeof(Grid), 1, validateValue: (bindable, value) => (int)value >= 1);

		/// <summary>Bindable property for <see cref="RowSpacing"/>.</summary>
		public static readonly BindableProperty RowSpacingProperty = BindableProperty.Create(nameof(RowSpacing), typeof(double), typeof(Grid), 6d,
			propertyChanged: (bindable, oldValue, newValue) => ((Grid)bindable).InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged));

		/// <summary>Bindable property for <see cref="ColumnSpacing"/>.</summary>
		public static readonly BindableProperty ColumnSpacingProperty = BindableProperty.Create(nameof(ColumnSpacing), typeof(double), typeof(Grid), 6d,
			propertyChanged: (bindable, oldValue, newValue) => ((Grid)bindable).InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged));

		/// <summary>Bindable property for <see cref="ColumnDefinitions"/>.</summary>
		public static readonly BindableProperty ColumnDefinitionsProperty = BindableProperty.Create(nameof(ColumnDefinitions), typeof(ColumnDefinitionCollection), typeof(Grid), null,
			validateValue: (bindable, value) => value != null, propertyChanged: (bindable, oldvalue, newvalue) =>
			{
				if (oldvalue != null)
					((ColumnDefinitionCollection)oldvalue).ItemSizeChanged -= ((Grid)bindable).OnDefinitionChanged;
				if (newvalue != null)
					((ColumnDefinitionCollection)newvalue).ItemSizeChanged += ((Grid)bindable).OnDefinitionChanged;
				((Grid)bindable).OnDefinitionChanged(bindable, EventArgs.Empty);
			}, defaultValueCreator: bindable =>
			{
				var colDef = new ColumnDefinitionCollection();
				colDef.ItemSizeChanged += ((Grid)bindable).OnDefinitionChanged;
				return colDef;
			});

		/// <summary>Bindable property for <see cref="RowDefinitions"/>.</summary>
		public static readonly BindableProperty RowDefinitionsProperty = BindableProperty.Create(nameof(RowDefinitions), typeof(RowDefinitionCollection), typeof(Grid), null,
			validateValue: (bindable, value) => value != null, propertyChanged: (bindable, oldvalue, newvalue) =>
			{
				if (oldvalue != null)
					((RowDefinitionCollection)oldvalue).ItemSizeChanged -= ((Grid)bindable).OnDefinitionChanged;
				if (newvalue != null)
					((RowDefinitionCollection)newvalue).ItemSizeChanged += ((Grid)bindable).OnDefinitionChanged;
				((Grid)bindable).OnDefinitionChanged(bindable, EventArgs.Empty);
			}, defaultValueCreator: bindable =>
			{
				var rowDef = new RowDefinitionCollection();
				rowDef.ItemSizeChanged += ((Grid)bindable).OnDefinitionChanged;
				return rowDef;
			});

		readonly GridElementCollection _children;
		readonly Lazy<PlatformConfigurationRegistry<Grid>> _platformConfigurationRegistry;

		public Grid()
		{
			Hosting.CompatibilityCheck.CheckForCompatibility();
			_children = new GridElementCollection(InternalChildren, this) { Parent = this };
			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<Grid>>(() =>
				new PlatformConfigurationRegistry<Grid>(this));
		}

		/// <inheritdoc/>
		public IPlatformElementConfiguration<T, Grid> On<T>() where T : IConfigPlatform
		{
			return _platformConfigurationRegistry.Value.On<T>();
		}

		public new IGridList<View> Children
		{
			get { return _children; }
		}

		[System.ComponentModel.TypeConverter(typeof(ColumnDefinitionCollectionTypeConverter))]
		public ColumnDefinitionCollection ColumnDefinitions
		{
			get { return (ColumnDefinitionCollection)GetValue(ColumnDefinitionsProperty); }
			set { SetValue(ColumnDefinitionsProperty, value); }
		}

		public double ColumnSpacing
		{
			get { return (double)GetValue(ColumnSpacingProperty); }
			set { SetValue(ColumnSpacingProperty, value); }
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

		protected override void OnAdded(View view)
		{
			base.OnAdded(view);
			view.PropertyChanged += OnItemPropertyChanged;
		}

		protected override void OnBindingContextChanged()
		{
			UpdateInheritedBindingContexts();
			base.OnBindingContextChanged();
		}

		protected override void OnRemoved(View view)
		{
			base.OnRemoved(view);
			view.PropertyChanged -= OnItemPropertyChanged;
		}

		internal override void ComputeConstraintForView(View view)
		{
			LayoutOptions vOptions = view.VerticalOptions;
			LayoutOptions hOptions = view.HorizontalOptions;

			var result = LayoutConstraint.None;

			// grab a snapshot of this grid's structure for computing the constraints
			var structure = new GridStructure(this);

			if (vOptions.Alignment == LayoutAlignment.Fill)
			{
				int row = GetRow(view);
				int rowSpan = GetRowSpan(view);
				List<RowDefinition> rowDefinitions = structure.Rows;

				var canFix = true;

				for (int i = row; i < row + rowSpan && i < rowDefinitions.Count; i++)
				{
					GridLength height = rowDefinitions[i].Height;
					if (height.IsAuto)
					{
						canFix = false;
						break;
					}
					if ((Constraint & LayoutConstraint.VerticallyFixed) == 0 && height.IsStar)
					{
						canFix = false;
						break;
					}
				}

				if (canFix)
					result |= LayoutConstraint.VerticallyFixed;
			}

			if (hOptions.Alignment == LayoutAlignment.Fill)
			{
				int col = GetColumn(view);
				int colSpan = GetColumnSpan(view);
				List<ColumnDefinition> columnDefinitions = structure.Columns;

				var canFix = true;

				for (int i = col; i < col + colSpan && i < columnDefinitions.Count; i++)
				{
					GridLength width = columnDefinitions[i].Width;
					if (width.IsAuto)
					{
						canFix = false;
						break;
					}
					if ((Constraint & LayoutConstraint.HorizontallyFixed) == 0 && width.IsStar)
					{
						canFix = false;
						break;
					}
				}

				if (canFix)
					result |= LayoutConstraint.HorizontallyFixed;
			}

			view.ComputedConstraint = result;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void InvalidateMeasureInernalNonVirtual(InvalidationTrigger trigger)
		{
			InvalidateMeasureInternal(trigger);
		}

		void OnDefinitionChanged(object sender, EventArgs args)
		{
			ComputeConstrainsForChildren();
			UpdateInheritedBindingContexts();
			InvalidateLayout();
		}

		void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == ColumnProperty.PropertyName || e.PropertyName == ColumnSpanProperty.PropertyName || e.PropertyName == RowProperty.PropertyName ||
				e.PropertyName == RowSpanProperty.PropertyName)
			{
				var child = sender as View;
				if (child != null)
				{
					ComputeConstraintForView(child);
				}

				InvalidateLayout();
			}
		}

		void UpdateInheritedBindingContexts()
		{
			object bindingContext = BindingContext;
			RowDefinitionCollection rowDefs = RowDefinitions;
			if (rowDefs != null)
			{
				for (var i = 0; i < rowDefs.Count; i++)
				{
					RowDefinition rowdef = rowDefs[i];
					SetInheritedBindingContext(rowdef, bindingContext);
				}
			}

			ColumnDefinitionCollection colDefs = ColumnDefinitions;
			if (colDefs != null)
			{
				for (var i = 0; i < colDefs.Count; i++)
				{
					ColumnDefinition coldef = colDefs[i];
					SetInheritedBindingContext(coldef, bindingContext);
				}
			}
		}

		public interface IGridList<T> : IList<T> where T : View
		{
			void Add(View view, int left, int top);
			void Add(View view, int left, int right, int top, int bottom);
			void AddHorizontal(IEnumerable<View> views);
			void AddHorizontal(View view);
			void AddVertical(IEnumerable<View> views);
			void AddVertical(View view);
		}

		class GridElementCollection : ElementCollection<View>, IGridList<View>
		{
			public GridElementCollection(ObservableCollection<Element> inner, Grid parent) : base(inner)
			{
				Parent = parent;
			}

			internal Grid Parent { get; set; }

			public void Add(View view, int left, int top)
			{
				if (left < 0)
					throw new ArgumentOutOfRangeException(nameof(left));
				if (top < 0)
					throw new ArgumentOutOfRangeException(nameof(top));
				Add(view, left, left + 1, top, top + 1);
			}

			public void Add(View view, int left, int right, int top, int bottom)
			{
				if (left < 0)
					throw new ArgumentOutOfRangeException(nameof(left));
				if (top < 0)
					throw new ArgumentOutOfRangeException(nameof(top));
				if (left >= right)
					throw new ArgumentOutOfRangeException(nameof(right));
				if (top >= bottom)
					throw new ArgumentOutOfRangeException(nameof(bottom));
				if (view == null)
					throw new ArgumentNullException(nameof(view));

				SetRow(view, top);
				SetRowSpan(view, bottom - top);
				SetColumn(view, left);
				SetColumnSpan(view, right - left);

				Add(view);
			}

			public void AddHorizontal(IEnumerable<View> views)
			{
				if (views == null)
					throw new ArgumentNullException(nameof(views));

				views.ForEach(AddHorizontal);
			}

			public void AddHorizontal(View view)
			{
				if (view == null)
					throw new ArgumentNullException(nameof(view));

				var rows = RowCount();
				var columns = ColumnCount();

				// if no rows, create a row
				if (rows == 0)
					rows++;

				Add(view, columns, columns + 1, 0, rows);
			}

			public void AddVertical(IEnumerable<View> views)
			{
				if (views == null)
					throw new ArgumentNullException(nameof(views));

				views.ForEach(AddVertical);
			}

			public void AddVertical(View view)
			{
				if (view == null)
					throw new ArgumentNullException(nameof(view));

				var rows = RowCount();
				var columns = ColumnCount();

				// if no columns, create a column
				if (columns == 0)
					columns++;

				Add(view, 0, columns, rows, rows + 1);
			}

			private int RowCount() => Math.Max(
				this.Max<View, int?>(w => GetRow(w) + GetRowSpan(w)) ?? 0,
				Parent.RowDefinitions.Count
			);

			private int ColumnCount() => Math.Max(
				this.Max<View, int?>(w => GetColumn(w) + GetColumnSpan(w)) ?? 0,
				Parent.ColumnDefinitions.Count
			);
		}

		int IGridLayout.GetRow(IView view)
		{
			if (view is BindableObject bo)
			{
				return GetRow(bo);
			}

			throw new InvalidEnumArgumentException($"{nameof(view)} must be a BindableObject");
		}

		int IGridLayout.GetColumn(IView view)
		{
			if (view is BindableObject bo)
			{
				return GetColumn(bo);
			}

			throw new InvalidEnumArgumentException($"{nameof(view)} must be a BindableObject");
		}

		int IGridLayout.GetRowSpan(IView view)
		{
			if (view is BindableObject bo)
			{
				return GetRowSpan(bo);
			}

			throw new InvalidEnumArgumentException($"{nameof(view)} must be a BindableObject");
		}

		int IGridLayout.GetColumnSpan(IView view)
		{
			if (view is BindableObject bo)
			{
				return GetColumnSpan(bo);
			}

			throw new InvalidEnumArgumentException($"{nameof(view)} must be a BindableObject");
		}

		IReadOnlyList<IGridRowDefinition> IGridLayout.RowDefinitions => RowDefinitions.ToList();
		IReadOnlyList<IGridColumnDefinition> IGridLayout.ColumnDefinitions => ColumnDefinitions.ToList();
	}
}