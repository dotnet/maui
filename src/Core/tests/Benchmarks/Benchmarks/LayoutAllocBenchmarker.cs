#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;
using Microsoft.Maui.Primitives;
using Flex = Microsoft.Maui.Layouts.Flex;

namespace Microsoft.Maui.Handlers.Benchmarks
{
	/// <summary>
	/// Measures true layout-code allocations using lightweight fake objects (no NSubstitute overhead).
	/// </summary>
	[MemoryDiagnoser]
	public class LayoutAllocBenchmarker
	{
		const double ConstraintWidth = 640;
		const double ConstraintHeight = 480;
		const int GridColumnCount = 4;

		GridLayoutManager _gridManager = null!;
		VerticalStackLayoutManager _vstackManager = null!;
		HorizontalStackLayoutManager _hstackManager = null!;

		[Params(12, 60)]
		public int ChildCount { get; set; }

		[Params(false, true)]
		public bool UseSpans { get; set; }

		[GlobalSetup]
		public void Setup()
		{
			var rowCount = Math.Max(2, (int)Math.Ceiling((double)ChildCount / GridColumnCount));

			var rows = new FakeRowDefinition[rowCount];
			for (int i = 0; i < rowCount; i++)
				rows[i] = new FakeRowDefinition(i % 2 == 0 ? GridLength.Auto : GridLength.Star);

			var cols = new FakeColumnDefinition[GridColumnCount];
			for (int i = 0; i < GridColumnCount; i++)
				cols[i] = new FakeColumnDefinition(i % 2 == 0 ? GridLength.Auto : GridLength.Star);

			var children = new FakeView[ChildCount];
			var cellLocations = new CellLocation[ChildCount];
			for (int i = 0; i < ChildCount; i++)
			{
				double w = 24 + (i % 7) * 4;
				double h = 18 + (i % 5) * 3;
				children[i] = new FakeView(w, h);

				int row = i / GridColumnCount;
				int col = i % GridColumnCount;
				int rs = UseSpans && row + 1 < rowCount && i % 5 == 0 ? 2 : 1;
				int cs = UseSpans && col + 1 < GridColumnCount && i % 3 == 0 ? 2 : 1;
				cellLocations[i] = new CellLocation(row, col, rs, cs);
			}

			var grid = new FakeGridLayout(children, rows, cols, cellLocations);
			_gridManager = new GridLayoutManager(grid);

			var vstackChildren = new FakeView[ChildCount];
			var hstackChildren = new FakeView[ChildCount];
			for (int i = 0; i < ChildCount; i++)
			{
				double w = 24 + (i % 7) * 4;
				double h = 18 + (i % 5) * 3;
				vstackChildren[i] = new FakeView(w, h);
				hstackChildren[i] = new FakeView(w, h);
			}

			_vstackManager = new VerticalStackLayoutManager(new FakeStackLayout(vstackChildren, 6));
			_hstackManager = new HorizontalStackLayoutManager(new FakeStackLayout(hstackChildren, 6));

			SetupFlex();
		}

		[Benchmark]
		public Size GridMeasureArrange()
		{
			var result = _gridManager.Measure(ConstraintWidth, ConstraintHeight);
			_gridManager.ArrangeChildren(new Rect(Point.Zero, result));
			return result;
		}

		[Benchmark]
		public Size VerticalStackMeasureArrange()
		{
			var result = _vstackManager.Measure(ConstraintWidth, ConstraintHeight);
			_vstackManager.ArrangeChildren(new Rect(Point.Zero, result));
			return result;
		}

		[Benchmark]
		public Size HorizontalStackMeasureArrange()
		{
			var result = _hstackManager.Measure(ConstraintWidth, ConstraintHeight);
			_hstackManager.ArrangeChildren(new Rect(Point.Zero, result));
			return result;
		}

		Flex.Item _flexRoot = null!;

		void SetupFlex()
		{
			_flexRoot = new Flex.Item();
			_flexRoot.Width = (float)ConstraintWidth;
			_flexRoot.Height = (float)ConstraintHeight;
			_flexRoot.Direction = Flex.Direction.Row;
			_flexRoot.Wrap = Flex.Wrap.Wrap;
			_flexRoot.PaddingLeft = 8;
			_flexRoot.PaddingTop = 8;
			_flexRoot.PaddingRight = 8;
			_flexRoot.PaddingBottom = 8;

			for (int i = 0; i < ChildCount; i++)
			{
				float w = 20 + (i % 5) * 6;
				float h = 16 + (i % 4) * 4;
				var child = new Flex.Item { Width = w, Height = h, Order = i % 3, Grow = i % 2, Shrink = 1 };
				_flexRoot.Add(child);
			}
		}

		[Benchmark]
		public void FlexCoreMeasureArrange()
		{
			_flexRoot.Layout(true);  // measure mode
			_flexRoot.Layout(false); // arrange mode
		}

		readonly record struct CellLocation(int Row, int Column, int RowSpan, int ColumnSpan);

		sealed class FakeView : IView
		{
			readonly Size _desiredSize;

			public FakeView(double width, double height)
			{
				_desiredSize = new Size(width, height);
			}

			public Size Measure(double widthConstraint, double heightConstraint) => _desiredSize;
			public Size Arrange(Rect bounds) => bounds.Size;
			public Size DesiredSize => _desiredSize;
			public Visibility Visibility => Visibility.Visible;
			public Thickness Margin => Thickness.Zero;
			public double Width => Dimension.Unset;
			public double Height => Dimension.Unset;
			public double MinimumWidth => Dimension.Minimum;
			public double MinimumHeight => Dimension.Minimum;
			public double MaximumWidth => Dimension.Maximum;
			public double MaximumHeight => Dimension.Maximum;
			public Primitives.LayoutAlignment HorizontalLayoutAlignment => Primitives.LayoutAlignment.Fill;
			public Primitives.LayoutAlignment VerticalLayoutAlignment => Primitives.LayoutAlignment.Fill;

			// Unused members — minimal stubs
			public string AutomationId => "";
			public FlowDirection FlowDirection => FlowDirection.MatchParent;
			public Semantics? Semantics => null;
			public IShape? Clip => null;
			public IShadow? Shadow => null;
			public bool IsEnabled => true;
			public bool IsFocused { get => false; set { } }
			public double Opacity => 1;
			public Paint? Background => null;
			public Rect Frame { get => Rect.Zero; set { } }
			public int ZIndex => 0;
			public IViewHandler? Handler { get => null; set { } }
			public bool InputTransparent => false;
			public void InvalidateMeasure() { }
			public void InvalidateArrange() { }
			public bool Focus() => false;
			public void Unfocus() { }
			public IElement? Parent => null;
			IElementHandler? IElement.Handler { get => null; set { } }
			public double TranslationX => 0;
			public double TranslationY => 0;
			public double Scale => 1;
			public double ScaleX => 1;
			public double ScaleY => 1;
			public double Rotation => 0;
			public double RotationX => 0;
			public double RotationY => 0;
			public double AnchorX => 0.5;
			public double AnchorY => 0.5;
		}

		sealed class FakeGridLayout : IGridLayout
		{
			readonly FakeView[] _children;
			readonly FakeRowDefinition[] _rows;
			readonly FakeColumnDefinition[] _cols;
			readonly CellLocation[] _cellLocations;

			public FakeGridLayout(FakeView[] children, FakeRowDefinition[] rows, FakeColumnDefinition[] cols, CellLocation[] cellLocations)
			{
				_children = children;
				_rows = rows;
				_cols = cols;
				_cellLocations = cellLocations;
			}

			public IReadOnlyList<IGridRowDefinition> RowDefinitions => _rows;
			public IReadOnlyList<IGridColumnDefinition> ColumnDefinitions => _cols;
			public double RowSpacing => 4;
			public double ColumnSpacing => 4;
			public int GetRow(IView view) => _cellLocations[IndexOf(view)].Row;
			public int GetRowSpan(IView view) => _cellLocations[IndexOf(view)].RowSpan;
			public int GetColumn(IView view) => _cellLocations[IndexOf(view)].Column;
			public int GetColumnSpan(IView view) => _cellLocations[IndexOf(view)].ColumnSpan;

			int IndexOf(IView view)
			{
				for (int i = 0; i < _children.Length; i++)
					if (ReferenceEquals(_children[i], view)) return i;
				return -1;
			}

			// IList<IView>
			public int Count => _children.Length;
			public bool IsReadOnly => true;
			public IView this[int index] { get => _children[index]; set => throw new NotSupportedException(); }
			int IList<IView>.IndexOf(IView item) => IndexOf(item);
			public void Insert(int index, IView item) => throw new NotSupportedException();
			public void RemoveAt(int index) => throw new NotSupportedException();
			public void Add(IView item) => throw new NotSupportedException();
			public void Clear() => throw new NotSupportedException();
			public bool Contains(IView item) => IndexOf(item) >= 0;
			public void CopyTo(IView[] array, int arrayIndex) => Array.Copy(_children, 0, array, arrayIndex, _children.Length);
			public bool Remove(IView item) => throw new NotSupportedException();
			public IEnumerator<IView> GetEnumerator() => ((IEnumerable<IView>)_children).GetEnumerator();
			IEnumerator IEnumerable.GetEnumerator() => _children.GetEnumerator();

			// ILayout
			public bool ClipsToBounds => false;
			public Size CrossPlatformArrange(Rect bounds) => bounds.Size;
			public Size CrossPlatformMeasure(double widthConstraint, double heightConstraint) => Size.Zero;

			// IPadding
			public Thickness Padding => new Thickness(8);

			// ISafeAreaView
			public bool IgnoreSafeArea => false;

			// IView
			public Size Measure(double widthConstraint, double heightConstraint) => Size.Zero;
			public Size Arrange(Rect bounds) => bounds.Size;
			public Size DesiredSize => Size.Zero;
			public Visibility Visibility => Visibility.Visible;
			public Thickness Margin => Thickness.Zero;
			public double Width => Dimension.Unset;
			public double Height => Dimension.Unset;
			public double MinimumWidth => Dimension.Minimum;
			public double MinimumHeight => Dimension.Minimum;
			public double MaximumWidth => Dimension.Maximum;
			public double MaximumHeight => Dimension.Maximum;
			public Primitives.LayoutAlignment HorizontalLayoutAlignment => Primitives.LayoutAlignment.Fill;
			public Primitives.LayoutAlignment VerticalLayoutAlignment => Primitives.LayoutAlignment.Fill;
			public string AutomationId => "";
			public FlowDirection FlowDirection => FlowDirection.MatchParent;
			public Semantics? Semantics => null;
			public IShape? Clip => null;
			public IShadow? Shadow => null;
			public bool IsEnabled => true;
			public bool IsFocused { get => false; set { } }
			public double Opacity => 1;
			public Paint? Background => null;
			public Rect Frame { get => Rect.Zero; set { } }
			public int ZIndex => 0;
			public IViewHandler? Handler { get => null; set { } }
			public bool InputTransparent => false;
			public void InvalidateMeasure() { }
			public void InvalidateArrange() { }
			public bool Focus() => false;
			public void Unfocus() { }
			public IElement? Parent => null;
			IElementHandler? IElement.Handler { get => null; set { } }
			public double TranslationX => 0;
			public double TranslationY => 0;
			public double Scale => 1;
			public double ScaleX => 1;
			public double ScaleY => 1;
			public double Rotation => 0;
			public double RotationX => 0;
			public double RotationY => 0;
			public double AnchorX => 0.5;
			public double AnchorY => 0.5;
		}

		sealed class FakeStackLayout : IStackLayout
		{
			readonly FakeView[] _children;

			public FakeStackLayout(FakeView[] children, double spacing)
			{
				_children = children;
				Spacing = spacing;
			}

			public double Spacing { get; }

			// IList<IView>
			public int Count => _children.Length;
			public bool IsReadOnly => true;
			public IView this[int index] { get => _children[index]; set => throw new NotSupportedException(); }
			public int IndexOf(IView item)
			{
				for (int i = 0; i < _children.Length; i++)
					if (ReferenceEquals(_children[i], item)) return i;
				return -1;
			}
			public void Insert(int index, IView item) => throw new NotSupportedException();
			public void RemoveAt(int index) => throw new NotSupportedException();
			public void Add(IView item) => throw new NotSupportedException();
			public void Clear() => throw new NotSupportedException();
			public bool Contains(IView item) => IndexOf(item) >= 0;
			public void CopyTo(IView[] array, int arrayIndex) => Array.Copy(_children, 0, array, arrayIndex, _children.Length);
			public bool Remove(IView item) => throw new NotSupportedException();
			public IEnumerator<IView> GetEnumerator() => ((IEnumerable<IView>)_children).GetEnumerator();
			IEnumerator IEnumerable.GetEnumerator() => _children.GetEnumerator();

			// ILayout
			public bool ClipsToBounds => false;
			public Size CrossPlatformArrange(Rect bounds) => bounds.Size;
			public Size CrossPlatformMeasure(double widthConstraint, double heightConstraint) => Size.Zero;

			// IPadding
			public Thickness Padding => new Thickness(8);

			// ISafeAreaView
			public bool IgnoreSafeArea => false;

			// IView
			public Size Measure(double widthConstraint, double heightConstraint) => Size.Zero;
			public Size Arrange(Rect bounds) => bounds.Size;
			public Size DesiredSize => Size.Zero;
			public Visibility Visibility => Visibility.Visible;
			public Thickness Margin => Thickness.Zero;
			public double Width => Dimension.Unset;
			public double Height => Dimension.Unset;
			public double MinimumWidth => Dimension.Minimum;
			public double MinimumHeight => Dimension.Minimum;
			public double MaximumWidth => Dimension.Maximum;
			public double MaximumHeight => Dimension.Maximum;
			public Primitives.LayoutAlignment HorizontalLayoutAlignment => Primitives.LayoutAlignment.Fill;
			public Primitives.LayoutAlignment VerticalLayoutAlignment => Primitives.LayoutAlignment.Fill;
			public string AutomationId => "";
			public FlowDirection FlowDirection => FlowDirection.MatchParent;
			public Semantics? Semantics => null;
			public IShape? Clip => null;
			public IShadow? Shadow => null;
			public bool IsEnabled => true;
			public bool IsFocused { get => false; set { } }
			public double Opacity => 1;
			public Paint? Background => null;
			public Rect Frame { get => Rect.Zero; set { } }
			public int ZIndex => 0;
			public IViewHandler? Handler { get => null; set { } }
			public bool InputTransparent => false;
			public void InvalidateMeasure() { }
			public void InvalidateArrange() { }
			public bool Focus() => false;
			public void Unfocus() { }
			public IElement? Parent => null;
			IElementHandler? IElement.Handler { get => null; set { } }
			public double TranslationX => 0;
			public double TranslationY => 0;
			public double Scale => 1;
			public double ScaleX => 1;
			public double ScaleY => 1;
			public double Rotation => 0;
			public double RotationX => 0;
			public double RotationY => 0;
			public double AnchorX => 0.5;
			public double AnchorY => 0.5;
		}

		sealed class FakeRowDefinition(GridLength height) : IGridRowDefinition
		{
			public GridLength Height => height;
		}

		sealed class FakeColumnDefinition(GridLength width) : IGridColumnDefinition
		{
			public GridLength Width => width;
		}
	}
}
