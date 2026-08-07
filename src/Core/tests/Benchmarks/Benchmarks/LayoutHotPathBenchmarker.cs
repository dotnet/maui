using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;
using NSubstitute;
using LayoutAlignment = Microsoft.Maui.Primitives.LayoutAlignment;
using static Microsoft.Maui.Primitives.Dimension;

namespace Microsoft.Maui.Handlers.Benchmarks
{
	[MemoryDiagnoser]
	public class LayoutHotPathBenchmarker
	{
		const double ConstraintWidth = 640;
		const double ConstraintHeight = 480;
		const int GridColumnCount = 4;

		readonly Rect _targetBounds = new(0, 0, ConstraintWidth, ConstraintHeight);

		IGridLayout _gridLayout = null!;
		GridLayoutManager _gridManager = null!;
		IStackLayout _verticalStackLayout = null!;
		VerticalStackLayoutManager _verticalStackManager = null!;
		IStackLayout _horizontalStackLayout = null!;
		HorizontalStackLayoutManager _horizontalStackManager = null!;
		FlexLayout _flexWrapLayout = null!;
		FlexLayout _flexNoWrapLayout = null!;

		[Params(12, 60)]
		public int ChildCount { get; set; }

		[Params(false, true)]
		public bool UseSpans { get; set; }

		[GlobalSetup]
		public void Setup()
		{
			SetupGrid();
			SetupStacks();
			SetupFlexLayouts();
		}

		[Benchmark]
		public Size GridMeasureArrange() => RunMeasureArrange(_gridManager);

		[Benchmark]
		public Size VerticalStackMeasureArrange() => RunMeasureArrange(_verticalStackManager);

		[Benchmark]
		public Size HorizontalStackMeasureArrange() => RunMeasureArrange(_horizontalStackManager);

		[Benchmark]
		public Size FlexMeasureArrangeWrap() => RunFlexMeasureArrange(_flexWrapLayout);

		[Benchmark]
		public Size FlexMeasureArrangeNoWrap() => RunFlexMeasureArrange(_flexNoWrapLayout);

		Size RunMeasureArrange(ILayoutManager manager)
		{
			var result = manager.Measure(ConstraintWidth, ConstraintHeight);
			manager.ArrangeChildren(new Rect(Point.Zero, result));
			return result;
		}

		Size RunFlexMeasureArrange(FlexLayout layout)
		{
			var result = layout.CrossPlatformMeasure(ConstraintWidth, ConstraintHeight);
			result = layout.CrossPlatformArrange(new Rect(Point.Zero, result));
			return result;
		}

		void SetupGrid()
		{
			_gridLayout = Substitute.For<IGridLayout>();
			ConfigureLayoutDefaults(_gridLayout);
			_gridLayout.RowSpacing.Returns(4);
			_gridLayout.ColumnSpacing.Returns(4);

			var rowCount = Math.Max(2, (int)Math.Ceiling((double)ChildCount / GridColumnCount));
			var rowDefinitions = new List<IGridRowDefinition>(rowCount);
			for (int row = 0; row < rowCount; row++)
			{
				var rowDefinition = Substitute.For<IGridRowDefinition>();
				rowDefinition.Height.Returns(row % 2 == 0 ? GridLength.Auto : GridLength.Star);
				rowDefinitions.Add(rowDefinition);
			}

			var columnDefinitions = new List<IGridColumnDefinition>(GridColumnCount);
			for (int column = 0; column < GridColumnCount; column++)
			{
				var columnDefinition = Substitute.For<IGridColumnDefinition>();
				columnDefinition.Width.Returns(column % 2 == 0 ? GridLength.Auto : GridLength.Star);
				columnDefinitions.Add(columnDefinition);
			}

			_gridLayout.RowDefinitions.Returns(rowDefinitions);
			_gridLayout.ColumnDefinitions.Returns(columnDefinitions);

			var children = CreateChildren(ChildCount);
			GridLayoutManagerBenchMarker.SubstituteChildren(_gridLayout, children);

			var locations = new Dictionary<IView, CellLocation>(children.Count);
			for (int index = 0; index < children.Count; index++)
			{
				int row = index / GridColumnCount;
				int column = index % GridColumnCount;

				int rowSpan = UseSpans && row + 1 < rowCount && index % 5 == 0 ? 2 : 1;
				int columnSpan = UseSpans && column + 1 < GridColumnCount && index % 3 == 0 ? 2 : 1;

				locations[children[index]] = new CellLocation(row, column, rowSpan, columnSpan);
			}

			_gridLayout.GetRow(Arg.Any<IView>()).Returns(args => locations[(IView)args[0]].Row);
			_gridLayout.GetColumn(Arg.Any<IView>()).Returns(args => locations[(IView)args[0]].Column);
			_gridLayout.GetRowSpan(Arg.Any<IView>()).Returns(args => locations[(IView)args[0]].RowSpan);
			_gridLayout.GetColumnSpan(Arg.Any<IView>()).Returns(args => locations[(IView)args[0]].ColumnSpan);

			_gridManager = new GridLayoutManager(_gridLayout);
		}

		void SetupStacks()
		{
			var children = CreateChildren(ChildCount);

			_verticalStackLayout = Substitute.For<IStackLayout>();
			ConfigureLayoutDefaults(_verticalStackLayout);
			_verticalStackLayout.Spacing.Returns(6);
			GridLayoutManagerBenchMarker.SubstituteChildren(_verticalStackLayout, children);

			_horizontalStackLayout = Substitute.For<IStackLayout>();
			ConfigureLayoutDefaults(_horizontalStackLayout);
			_horizontalStackLayout.Spacing.Returns(6);
			GridLayoutManagerBenchMarker.SubstituteChildren(_horizontalStackLayout, children);

			_verticalStackManager = new VerticalStackLayoutManager(_verticalStackLayout);
			_horizontalStackManager = new HorizontalStackLayoutManager(_horizontalStackLayout);
		}

		void SetupFlexLayouts()
		{
			_flexWrapLayout = CreateFlexLayout(FlexWrap.Wrap);
			_flexNoWrapLayout = CreateFlexLayout(FlexWrap.NoWrap);
		}

		FlexLayout CreateFlexLayout(FlexWrap wrap)
		{
			var layout = new FlexLayout
			{
				Direction = FlexDirection.Row,
				Wrap = wrap,
				Padding = new Thickness(8)
			};

			var root = new Grid();
			root.Add(layout);

			for (int i = 0; i < ChildCount; i++)
			{
				var child = new Border
				{
					WidthRequest = 20 + (i % 5) * 6,
					HeightRequest = 16 + (i % 4) * 4
				};

				FlexLayout.SetOrder(child, i % 3);
				FlexLayout.SetGrow(child, i % 2);
				FlexLayout.SetShrink(child, 1);

				layout.Add(child);
			}

			return layout;
		}

		static List<IView> CreateChildren(int count)
		{
			var children = new List<IView>(count);

			for (int i = 0; i < count; i++)
			{
				double width = 24 + (i % 7) * 4;
				double height = 18 + (i % 5) * 3;
				children.Add(CreateView(width, height));
			}

			return children;
		}

		static IView CreateView(double width, double height)
		{
			var desiredSize = new Size(width, height);

			var view = Substitute.For<IView>();
			view.Width.Returns(Unset);
			view.Height.Returns(Unset);
			view.MinimumWidth.Returns(Minimum);
			view.MinimumHeight.Returns(Minimum);
			view.MaximumWidth.Returns(Maximum);
			view.MaximumHeight.Returns(Maximum);
			view.Margin.Returns(Thickness.Zero);
			view.Visibility.Returns(Visibility.Visible);
			view.HorizontalLayoutAlignment.Returns(LayoutAlignment.Fill);
			view.VerticalLayoutAlignment.Returns(LayoutAlignment.Fill);
			view.Measure(Arg.Any<double>(), Arg.Any<double>()).Returns(desiredSize);
			view.DesiredSize.Returns(desiredSize);
			view.Arrange(Arg.Any<Rect>()).Returns(args => ((Rect)args[0]).Size);

			return view;
		}

		static void ConfigureLayoutDefaults(ILayout layout)
		{
			layout.Width.Returns(Unset);
			layout.Height.Returns(Unset);
			layout.MinimumWidth.Returns(Minimum);
			layout.MinimumHeight.Returns(Minimum);
			layout.MaximumWidth.Returns(Maximum);
			layout.MaximumHeight.Returns(Maximum);
			layout.Padding.Returns(new Thickness(8));
			layout.Margin.Returns(Thickness.Zero);
			layout.Visibility.Returns(Visibility.Visible);
			layout.HorizontalLayoutAlignment.Returns(LayoutAlignment.Fill);
			layout.VerticalLayoutAlignment.Returns(LayoutAlignment.Fill);
		}

		readonly record struct CellLocation(int Row, int Column, int RowSpan, int ColumnSpan);
	}
}
