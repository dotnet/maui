using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	using Grid = Microsoft.Maui.Controls.Compatibility.Grid;
	using StackLayout = Microsoft.Maui.Controls.Compatibility.StackLayout;


	public class GridTests : BaseTestFixture
	{
		[Fact]
		public void ThrowsOnNullAdd()
		{
			var layout = new Grid();

			Assert.Throws<ArgumentNullException>(() => layout.Children.Add(null));
		}

		[Fact]
		public void ChildrenHaveParentsWhenAdded()
		{
			var layout = new Grid();
			var label = new Label();
			layout.Children.Add(label);

			Assert.Same(layout, label.Parent);

			layout.Children.Remove(label);
			Assert.Null(label.Parent);
		}

		[Fact]
		public void ThrowsOnNullRemove()
		{
			var layout = new Grid();

			Assert.Throws<ArgumentNullException>(() => layout.Children.Remove(null));
		}

		[Fact]
		public void StarColumnsHaveEqualWidths()
		{
			var grid = new Grid
			{
				VerticalOptions = LayoutOptions.Start,
				ColumnSpacing = 12
			};

			grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });
			grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });
			grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(5, GridUnitType.Star) });

			grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

			var label = new ColumnTestLabel
			{
				VerticalOptions = LayoutOptions.Start,
				LineBreakMode = LineBreakMode.WordWrap,
				Text = "There's a 104 days of summer vacation 'til school comes along just to end it. So the annual problem for our generation is finding a good way to spend it."
			};

			grid.Children.Add(label, 1, 0);

			var gridWidth = 411;
			grid.Measure(gridWidth, 1000);
			var column0Width = grid.ColumnDefinitions[0].ActualWidth;
			var column1Width = grid.ColumnDefinitions[1].ActualWidth;

			Assert.Equal(column0Width, column1Width);
			Assert.True(column0Width < gridWidth);
		}

		[Fact]
		public void StarRowsHaveEqualHeights()
		{
			var grid = new Grid
			{
				VerticalOptions = LayoutOptions.Start,
				RowSpacing = 12
			};

			grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Star });
			grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Star });
			grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(5, GridUnitType.Star) });

			grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

			var label = new RowTestLabel
			{
				VerticalOptions = LayoutOptions.Start,
				LineBreakMode = LineBreakMode.WordWrap,
				Text = "There's a 104 days of summer vacation 'til school comes along just to end it. So the annual problem for our generation is finding a good way to spend it."
			};

			grid.Children.Add(label, 1, 0);

			var gridHeight = 411;

			grid.Measure(1000, gridHeight);
			var column0Height = grid.RowDefinitions[0].ActualHeight;
			var column1Height = grid.RowDefinitions[1].ActualHeight;

			Assert.Equal(column0Height, column1Height);
			Assert.True(column0Height < gridHeight);
		}

		[Fact]
		public void StarRowsDoNotOverlapWithStackLayoutOnTop()
		{
			SetupStarRowOverlapTest(rowAIsOnTop: false, out VisualElement rowAControl,
				out VisualElement rowBControl, out Label lastLabel);

			var bottomOfRowB = rowBControl.Y + rowBControl.Height;
			var bottomOfLastLabelInRowB = rowBControl.Y + lastLabel.Y + lastLabel.Height;
			var topOfRowA = rowAControl.Y;

			Assert.Equal(bottomOfRowB, bottomOfLastLabelInRowB);

			Assert.Equal(topOfRowA, bottomOfRowB);
		}

		[Fact]
		public void StarRowsDoNotOverlapWithStackLayoutOnBottom()
		{
			SetupStarRowOverlapTest(rowAIsOnTop: true, out VisualElement rowAControl,
				out VisualElement rowBControl, out Label lastLabel);

			var topOfRowB = rowBControl.Y;
			var bottomOfRowB = rowBControl.Y + rowBControl.Height;
			var bottomOfLastLabelInRowB = rowBControl.Y + lastLabel.Y + lastLabel.Height;
			var bottomOfRowA = rowAControl.Y + rowAControl.Height;

			Assert.Equal(bottomOfRowB, bottomOfLastLabelInRowB);

			Assert.Equal(topOfRowB, bottomOfRowA);
		}

		[Fact]
		public void StarColumnsDoNotOverlapWithStackLayoutAtStart()
		{
			SetupStarColumnOverlapTest(colAIsAtStart: false, out VisualElement colAControl,
				out VisualElement colBControl, out Label lastLabel);

			var endOfColB = colBControl.X + colBControl.Width;
			var endOfLastLabelInColB = colBControl.X + lastLabel.X + lastLabel.Width;
			var startOfColA = colAControl.X;

			Assert.Equal(endOfColB, endOfLastLabelInColB);

			Assert.True(startOfColA == endOfColB,
				"B is before A, so the start of A should be the end of B");
		}

		[Fact]
		public void StarColumnsDoNotOverlapWithStackLayoutAtEnd()
		{
			SetupStarColumnOverlapTest(colAIsAtStart: true, out VisualElement colAControl,
				out VisualElement colBControl, out Label lastLabel);

			var startOfColB = colBControl.X;
			var endOfColB = colBControl.X + colBControl.Width;
			var endOfLastLabelInColB = colBControl.X + lastLabel.X + lastLabel.Width;
			var endOfColA = colAControl.X + colAControl.Width;

			Assert.Equal(endOfColB, endOfLastLabelInColB);

			Assert.True(endOfColA == startOfColB,
				"A is before B, so the end of A should be the start of B");
		}

		void SetupStarRowOverlapTest(bool rowAIsOnTop, out VisualElement rowAControl,
			out VisualElement rowBControl, out Label lastLabel)
		{
			var grid = new Grid
			{
				RowSpacing = 0,
				RowDefinitions = new RowDefinitionCollection
				{
					new RowDefinition() { Height = GridLength.Star },
					new RowDefinition() { Height = GridLength.Star }
				}
			};

			var labelSize = new Size(100, 20);

			Label label;
			rowAControl = label = new FixedSizeLabel(labelSize) { Text = "Hello" };

			StackLayout stackLayout;
			rowBControl = stackLayout = new StackLayout() { Spacing = 0, IsPlatformEnabled = true };

			for (int n = 0; n < 14; n++)
			{
				var labelInStack = new FixedSizeLabel(labelSize) { Text = "Hello" };
				stackLayout.Children.Add(labelInStack);
			}

			lastLabel = new FixedSizeLabel(labelSize) { Text = "Hello" };
			stackLayout.Children.Add(lastLabel);

			grid.Children.Add(stackLayout);
			grid.Children.Add(label);

			if (rowAIsOnTop)
			{
				Grid.SetRow(rowAControl, 0);
				Grid.SetRow(rowBControl, 1);
			}
			else
			{
				Grid.SetRow(rowBControl, 0);
				Grid.SetRow(rowAControl, 1);
			}

			var sizeRequest = grid.Measure(300, double.PositiveInfinity);
			grid.Layout(new Rect(0, 0, sizeRequest.Request.Width, sizeRequest.Request.Height));
		}

		void SetupStarColumnOverlapTest(bool colAIsAtStart, out VisualElement colAControl,
				out VisualElement colBControl, out Label lastLabel)
		{
			var grid = new Grid
			{
				ColumnSpacing = 0,
				ColumnDefinitions = new ColumnDefinitionCollection
				{
					new ColumnDefinition() { Width = GridLength.Star },
					new ColumnDefinition() { Width = GridLength.Star }
				}
			};

			var labelSize = new Size(20, 100);

			Label label;
			colAControl = label = new FixedSizeLabel(labelSize) { Text = "Hello" };

			StackLayout stackLayout;
			colBControl = stackLayout = new StackLayout() { Spacing = 0, Orientation = StackOrientation.Horizontal, IsPlatformEnabled = true };

			for (int n = 0; n < 14; n++)
			{
				var labelInStack = new FixedSizeLabel(labelSize) { Text = "Hello" };
				stackLayout.Children.Add(labelInStack);
			}

			lastLabel = new FixedSizeLabel(labelSize) { Text = "Hello" };
			stackLayout.Children.Add(lastLabel);

			grid.Children.Add(stackLayout);
			grid.Children.Add(label);

			if (colAIsAtStart)
			{
				Grid.SetColumn(colAControl, 0);
				Grid.SetColumn(colBControl, 1);
			}
			else
			{
				Grid.SetColumn(colBControl, 0);
				Grid.SetColumn(colAControl, 1);
			}

			var sizeRequest = grid.Measure(double.PositiveInfinity, 300);
			grid.Layout(new Rect(0, 0, sizeRequest.Request.Width, sizeRequest.Request.Height));
		}

		[Fact("Columns with a Star width less than one should not cause the Grid to contract below the target width; see https://github.com/xamarin/Microsoft.Maui.Controls/issues/11742")]
		public void StarWidthsLessThanOneShouldNotContractGrid()
		{
			var grid = new Grid
			{
				VerticalOptions = LayoutOptions.Start,
				ColumnSpacing = 12
			};

			grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(0.8, GridUnitType.Star) });
			grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });

			grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

			var label0 = new ColumnTestLabel
			{
				VerticalOptions = LayoutOptions.Start,
				LineBreakMode = LineBreakMode.WordWrap,
				Text = "This should wrap a bit"
			};

			var label1 = new ColumnTestLabel
			{
				VerticalOptions = LayoutOptions.Start,
				LineBreakMode = LineBreakMode.WordWrap,
				Text = "This ought to fit in the space just fine."
			};

			grid.Children.Add(label0, 0, 0);
			grid.Children.Add(label1, 1, 0);

			var gridWidth = 411;
			grid.Measure(gridWidth, 1000);
			var column0Width = grid.ColumnDefinitions[0].ActualWidth;
			var column1Width = grid.ColumnDefinitions[1].ActualWidth;

			Assert.True(column0Width < column1Width);

			// Having a first column which is a fraction of a Star width should not cause the grid
			// to contract below the target width
			var totalColumnSpacing = (grid.ColumnDefinitions.Count - 1) * grid.ColumnSpacing;

			Assert.True(column0Width + column1Width + totalColumnSpacing >= gridWidth);
		}

		[Fact]
		public void ColumnsLessThanOneStarShouldBeTallerThanOneStarColumns()
		{
			var gridWidth = 400;

			var grid1 = new Grid() { ColumnSpacing = 0 };

			grid1.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });
			grid1.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });

			grid1.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

			var label1 = new ColumnTestLabel
			{
				Text = "label1"
			};

			grid1.Children.Add(label1, 0, 0);
			grid1.Measure(gridWidth, double.PositiveInfinity);
			var grid1Height = grid1.RowDefinitions[0].ActualHeight;

			var grid2 = new Grid() { ColumnSpacing = 0 };

			// Because the column with the label in it is narrower in this grid (0.5* vs 1*), the label will have
			// grow taller to fit the text. So we expect this grid to grow vertically to accommodate it.
			grid2.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(0.5, GridUnitType.Star) });
			grid2.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });

			grid2.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

			var label2 = new ColumnTestLabel
			{
				Text = "label2"
			};

			grid2.Children.Add(label2, 0, 0);

			grid2.Measure(gridWidth, double.PositiveInfinity);
			var grid2Height = grid2.RowDefinitions[0].ActualHeight;

			Assert.True(grid2Height >= (grid1Height));
		}


		[Fact]
		public void ContentHeightSumShouldMatchGridHeightWithAutoRows()
		{
			var widthConstraint = 400;

			var grid1 = new Grid() { ColumnSpacing = 0, Padding = 0, RowSpacing = 0 };

			grid1.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });
			grid1.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });

			grid1.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
			grid1.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

			var label1 = new ColumnTestLabel { Text = "label1" };
			var label2 = new ColumnTestLabel { Text = "label2" };

			grid1.Children.Add(label1, 0, 0);
			grid1.Children.Add(label2, 0, 1);
			var grid1Size = grid1.Measure(widthConstraint, double.PositiveInfinity, MeasureFlags.IncludeMargins);
			grid1.Layout(new Rect(0, 0, grid1Size.Request.Width, grid1Size.Request.Height));
			var grid1Height = grid1.Height;

			var expectedHeight = label1.Height + label2.Height + grid1.RowSpacing;
			Assert.Equal(grid1Height, expectedHeight);
		}

		[Fact]
		public void UnconstrainedStarRowWithMultipleStarColumnsAllowsTextToGrow()
		{
			var outerGrid = new Grid() { ColumnSpacing = 0, Padding = 0, RowSpacing = 0, IsPlatformEnabled = true };
			outerGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Star });

			outerGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });
			outerGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });

			var sl = new StackLayout { Padding = 0, IsPlatformEnabled = true };

			var label1 = new HeightBasedOnTextLengthLabel
			{
				Text = "The actual text here doesn't matter, just the length. The length determines the height."
			};

			var label2 = new ColumnTestLabel { FontSize = 13, LineBreakMode = LineBreakMode.NoWrap, Text = "Description" };

			sl.Children.Add(label1);
			sl.Children.Add(label2);

			var bv = new BoxView { WidthRequest = 50, BackgroundColor = Colors.Blue, IsPlatformEnabled = true };

			outerGrid.Children.Add(sl);
			outerGrid.Children.Add(bv);
			Grid.SetColumn(bv, 1);

			var width = 400;
			var expectedColumnWidth = width / 2;

			// Measure and layout the grid
			var firstMeasure = outerGrid.Measure(width, double.PositiveInfinity, MeasureFlags.IncludeMargins).Request;
			outerGrid.Layout(new Rect(0, 0, firstMeasure.Width, firstMeasure.Height));

			// Verify that the actual height of the label is what we would expect (within a tolerance)
			AssertEqualWithTolerance(label1.DesiredHeight(expectedColumnWidth), label1.Height, 2);

			var label1OriginalHeight = label1.Height;

			// Increase the text
			label1.Text += label1.Text;

			// And measure/layout again
			var secondMeasure = outerGrid.Measure(width, double.PositiveInfinity, MeasureFlags.IncludeMargins).Request;
			outerGrid.Layout(new Rect(0, 0, secondMeasure.Width, secondMeasure.Height));

			// Verify that the actual height of the label is what we would expect (within a tolerance)
			AssertEqualWithTolerance(label1.Height, label1.DesiredHeight(expectedColumnWidth), 2);

			// And that the new height is taller than the old one (since there's more text, and the column width did not change)
			Assert.True(label1.Height >= (label1OriginalHeight));
		}

		static void AssertEqualWithTolerance(double a, double b, double tolerance)
		{
			var diff = Math.Abs(a - b);
			Assert.True(diff <= tolerance);
		}

		[Theory]
		[InlineData(0.1), InlineData(0.2), InlineData(0.3), InlineData(0.4), InlineData(0.5)]
		[InlineData(0.6), InlineData(0.7), InlineData(0.8), InlineData(0.9)]
		public void AbsoluteColumnShouldNotBloatStarredColumns(double firstColumnWidth)
		{
			// This is a re-creation of the layout from Issue 12292
			// The problem is that a huge label in a star column between a partial star column and
			// an absolute column causes the container to get the wrong width during an early measure pass.
			// The big label gets the wrong dimensions for measurement, and returns a height that won't actually
			// work in the final layout.

			var outerGrid = new Grid { ColumnSpacing = 0, Padding = 0, RowSpacing = 0, IsPlatformEnabled = true };

			outerGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

			outerGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(firstColumnWidth, GridUnitType.Star) }); // 0.3, but 0.2 works fine
			outerGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });

			// This last column is the trouble spot; MeasureAndContractStarredColumns needs to account for this width
			// when measuring and distributing the starred column space. Otherwise it reports the wrong width and every
			// measure after that is wrong.
			outerGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = 36 });

			var innerGrid = new Grid() { ColumnSpacing = 0, Padding = 0, RowSpacing = 0, IsPlatformEnabled = true };

			outerGrid.Children.Add(innerGrid);
			Grid.SetColumn(innerGrid, 1);

			innerGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
			innerGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

			var hugeLabel = new _12292TestLabel() { };
			var tinyLabel = new ColumnTestLabel { Text = "label1" };

			innerGrid.Children.Add(hugeLabel);
			innerGrid.Children.Add(tinyLabel);
			Grid.SetRow(tinyLabel, 1);

			var scrollView = new ScrollView() { IsPlatformEnabled = true };
			scrollView.Content = outerGrid;

			var layoutSize = scrollView.Measure(411, 603, MeasureFlags.IncludeMargins);

			// The containing ScrollView should measure a width of about 411; the absolute column at the end of the grid
			// shouldn't expand the ScrollView's measure to 447-ish. It's this expansion of the ScrollView that causes
			// all subsequent parts of layout to go pear-shaped.
			AssertEqualWithTolerance(411, layoutSize.Request.Width, 2);
		}

		[Fact]
		public void ContractionAppliedEquallyOnMultiStarColumns()
		{
			var grid = new Grid { ColumnSpacing = 0 };

			grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
			grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(2, GridUnitType.Star) });

			grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

			var smaller = new Size(100, 10);
			var larger = new Size(200, 10);
			var min = new Size(0, 10);

			var label0 = new FixedSizeLabel(min, smaller)
			{
				Text = "label0"
			};

			var label1 = new FixedSizeLabel(min, larger)
			{
				Text = "label1"
			};

			grid.Children.Add(label0, 0, 0);
			grid.Children.Add(label1, 1, 0);

			// requested total width is 300, so this will force a contraction to 200
			grid.Measure(200, 100);
			var column0Width = grid.ColumnDefinitions[0].ActualWidth;
			var column1Width = grid.ColumnDefinitions[1].ActualWidth;

			Assert.Equal(column0Width, column1Width / 2);
		}

		[Fact]
		public void AllStarColumnsCanOnlyContractToTheLargestMinimum()
		{
			var grid = new Grid { ColumnSpacing = 0 };

			grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
			grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });

			grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

			var leftColumn = new Size(100, 10);
			var rightColumn = new Size(100, 10);
			var largerMin = new Size(75, 10);
			var smallerMin = new Size(50, 10);

			var leftLabel = new FixedSizeLabel(largerMin, leftColumn)
			{
				Text = "label0"
			};

			var rightLabel = new FixedSizeLabel(smallerMin, rightColumn)
			{
				Text = "label1"
			};

			grid.Children.Add(leftLabel, 0, 0);
			grid.Children.Add(rightLabel, 1, 0);

			// requested total width is 200, so this will force an attemped contraction to 100
			grid.Measure(100, 100);
			var column0Width = grid.ColumnDefinitions[0].ActualWidth;
			var column1Width = grid.ColumnDefinitions[1].ActualWidth;

			Assert.Equal(column0Width, column1Width);
		}

		[Fact]
		public void ContractionAppliedEquallyOnMultiStarRows()
		{
			var grid = new Grid { ColumnSpacing = 0, RowSpacing = 0 };

			grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
			grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(2, GridUnitType.Star) });

			var smaller = new Size(100, 100);
			var larger = new Size(100, 200);
			var min = new Size(100, 0);

			var label0 = new FixedSizeLabel(min, smaller)
			{
				Text = "label0"
			};

			var label1 = new FixedSizeLabel(min, larger)
			{
				Text = "label1"
			};

			grid.Children.Add(label0, 0, 0);
			grid.Children.Add(label1, 1, 0);

			// requested total height is 300, so this will force a contraction to 200
			grid.Measure(200, 200);
			var column0Height = grid.RowDefinitions[0].ActualHeight;
			var column1Height = grid.RowDefinitions[1].ActualHeight;

			Assert.Equal(column0Height, column1Height / 2);
		}

		[Fact]
		public void Issue13127()
		{
			var outerGrid = new Grid() { RowSpacing = 0, IsPlatformEnabled = true };
			var outerStackLayout = new StackLayout() { Spacing = 0, IsPlatformEnabled = true };

			var innerGrid = new Grid() { RowSpacing = 0, IsPlatformEnabled = true };
			innerGrid.RowDefinitions = new RowDefinitionCollection() {
				new RowDefinition(){ Height = new GridLength(6, GridUnitType.Star)},
				new RowDefinition(){ Height = new GridLength(4, GridUnitType.Star)},
			};

			// Set up the background view, only covers the first row
			var background = new BoxView() { IsPlatformEnabled = true };
			Grid.SetRowSpan(background, 1);

			// Create the foreground, which spans both rows
			var foreground = new StackLayout() { Spacing = 0, IsPlatformEnabled = true };
			var view1 = new FixedSizeLabel(new Size(200, 50)) { IsPlatformEnabled = true };
			var view2 = new FixedSizeLabel(new Size(200, 100)) { IsPlatformEnabled = true };
			foreground.Children.Add(view1);
			foreground.Children.Add(view2);
			Grid.SetRowSpan(foreground, 2);

			innerGrid.Children.Add(background);
			innerGrid.Children.Add(foreground);

			outerStackLayout.Children.Add(innerGrid);
			outerGrid.Children.Add(outerStackLayout);

			var sizeRequest = outerGrid.Measure(500, 1000);
			outerGrid.Layout(new Rect(0, 0, sizeRequest.Request.Width, 1000));

			Assert.Equal(innerGrid.Height, foreground.Height);
			AssertEqualWithTolerance(background.Height, foreground.Height * 0.6, 0.01);

			Assert.Equal(165, background.Height);
		}

		abstract class TestLabel : Label
		{
			protected TestLabel()
			{
				IsPlatformEnabled = true;
			}
		}

		class FixedSizeLabel : TestLabel
		{
			readonly Size _minimumSize;
			readonly Size _requestedSize;

			public FixedSizeLabel(Size minimumSize, Size requestedSize)
			{
				_minimumSize = minimumSize;
				_requestedSize = requestedSize;
			}

			public FixedSizeLabel(Size size) : this(size, size) { }

			protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
			{
				return new SizeRequest(_requestedSize, _minimumSize);
			}
		}

		class _12292TestLabel : Label
		{
			// We need a label that simulates a fairly specific layout/measure pattern to reproduce
			// the circumstances of issue 12292.

			int _counter;

			public _12292TestLabel()
			{
				IsPlatformEnabled = true;
				Text = "dfghjkl;SCAsdnlv dvjhdbcviaijdlvnkhubv oebwepuvjlvsdiljh dvjhdbcviaijdlvnkhubv dvjhdbcviaijdlvnkhubv oebwepuvjlvsdiljh dvjhdbcviaijdlvnkhubv dvjhdbcviaijdlvnkhubv oebwepuvjlvsdiljh dvjhdbcviaijdlvnkhubv dvjhdbcviaijdlvnkhubv oebwepuvjlvsdiljh dvjhdbcviaijdlvnkhubv dvjhdbcviaijdlvnkhubv oebwepuvjlvsdiljh dvjhdbcviaijdlvnkhubv  oebwepuvjlvsdiljh dvjhdbcviaijdlvnkhubv dvjhdbcviaijdlvnkhubv oebwepuvjlvsdiljh oebwepuvjlvsdiljhssdlncaCSN SNCAascsbdn  sciwohwfwef wbodlaoj bcwhuofw9qph nxaxhsavcgsdcvewp ibewfwfhpo sbcshclcsdc aasusos 9 p;fqpnwuvaycxaslucn;we;oivwemopv mre]bn ;nvw  modcefin e['vmdkv wqs vwlj vqur;/ b;bnoerbor blk evoneifb;4rbkk-eq'o ge  vlfbmokfen wov mdkqncvw;bnzdFCGHSIAJDOKFBLKVSCBAXVCGFAYGUIOK;LBMDF, NZBCHGFSYUGAEUHRPK;LBMFNVBCFYEWYGUIOPBK; M,MNBCDTFYYU9GIPL;LMVNCX KOEKFULIDJOPKWLFSBVHGIROIQWDMC, ;QLKHFEUHFIJOKPDS;LMNDFVGUHFIDJXHFJOKPEOEJGHRIFJEWODK;LMNBVJHGIOJFEPKWD;LMNDBF VBIWOPKWFKBNRGJOFKPELDWKNVDSHFIOEFIEPKLMDWNDVSFBDIHOFEPDKWL;MNVFBF C,P POIUYFUYGIHOJ;LMFE WGREBFX CUOUIGYUFCHJDKJLFK;EGRHMNBHIOKVEJKVNERNVOEIV OWIEFNIENEKEDLC,WP,EFF dfghjkl;SCAsdnlv dvjhdbcviaijdlvnkhubv oebwepuvjlvsdiljh ssdlncaCSN SNCAascsbdn  sciwohwfwef wbodlaoj bcwhuofw9qph nxaxhsavcgsdcvewp ibewfwfhpo sbcshclcsdc aasusos 9 p;fqpnwuvaycxaslucn;we;oivwemopv mre]bn ;nvw  modcefin e['vmdkv wqs vwlj vqur;/ b;bnoerbor blk evoneifb;4rbkk-eq'o ge  vlfbmokfen wov mdkqncvw;bnzdFCGHSIAJDOKFBLKVSCBAXVCGFAYGUIOK;LBMDF, NZBCHGFSYUGAEUHRPK;LBMFNVBCFYEWYGUIOPBK; M,MNBCDTFYYU9GIPL;LMVNCX KOEKFULIDJOPKWLFSBVHGIROIQWDMC, ;QLKHFEUHFIJOKPDS;LMNDFVGUHFIDJXHFJOKPEOEJGHRIFJEWODK;LMNBVJHGIOJFEPKWD;LMNDBF VBIWOPKWFKBNRGJOFKPELDWKNVDSHFIOEFIEPKLMDWNDVSFBDIHOFEPDKWL;MNVFBF C,P POIUYFUYGIHOJ;LMFE WGREBFX CUOUIGYUFCHJDKJLFK;EGRHMNBHIOKVEJKVNERNVOEIV OWIEFNIENEKEDLC,WP,1234567890234567890";
			}

			protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
			{
				double minWidth = 10.2857142857143;

				_counter += 1;

				switch (_counter % 6)
				{
					case 1:
						return new SizeRequest(
							new Size(375.619047619048, 673.904761904762),
							new Size(minWidth, 673.904761904762));
					case 2:
						return new SizeRequest(
							new Size(411.428571428571, 575.619047619048),
							new Size(minWidth, 575.619047619048));
					case 3:
						return new SizeRequest(
							new Size(336.380952380952, 755.809523809524),
							new Size(minWidth, 755.809523809524));
					case 4:
						return new SizeRequest(
							new Size(375.619047619048, 673.904761904762),
							new Size(minWidth, 673.904761904762));
					case 5:
						return new SizeRequest(
							new Size(313.142857142857, 772.190476190476),
							new Size(minWidth, 772.190476190476));
					case 0:
						return new SizeRequest(
							new Size(313.142857142857, 772.190476190476),
							new Size(minWidth, 772.190476190476));
				}

				throw new Exception("This shouldn't happen, unless we make measure/layout more or less efficient and " +
					"OnMeasure isn't called 6 times during this test.");
			}
		}

		class HeightBasedOnTextLengthLabel : TestLabel
		{
			public double DesiredHeight(double widthConstraint)
			{
				return (Text.Length / widthConstraint) * 200;
			}

			protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
			{
				var minimumSize = new Size(20, 20);
				var height = DesiredHeight(widthConstraint);
				return new SizeRequest(new Size(widthConstraint, height), minimumSize);
			}
		}

		class ColumnTestLabel : TestLabel
		{
			protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
			{
				var minimumSize = new Size(20, 20);
				var height = 10000 / widthConstraint;
				return new SizeRequest(new Size(widthConstraint, height), minimumSize);
			}
		}

		class RowTestLabel : TestLabel
		{
			protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
			{
				var minimumSize = new Size(20, 20);

				if (double.IsInfinity(heightConstraint))
				{
					heightConstraint = 1000;
				}

				var width = 10000 / heightConstraint;
				return new SizeRequest(new Size(width, heightConstraint), minimumSize);
			}
		}


		public class AddDimension : GridTests
		{
			public static IEnumerable<object[]> Operations()
			{
				var opsStrings = new string[]
				{
					"HHH",
					"HHV",
					"HVH",
					"HVV",
					"VHH",
					"VHV",
					"VVH",
					"VVV",

					"RCRHVHVHVHVHV",

					"HHHV",
					"VVVH",

					"RV",
					"RH",
					"CV",
					"CH",

					"RVRRV",
					"CHCCH"
				 };

				foreach (var ops in opsStrings)
				{
					yield return new object[] { ops };
				}
			}

			Grid _grid;

			int _id = 0;
			int _rowDef = 0;
			int _colDef = 0;
			int _totalWidth = 0;
			int _totalHeight = 0;

			void AddHorizontal()
			{
				// new block gets new id
				var id = _id++;

				// adding column only increases height if no rows exist
				if (_totalHeight == 0)
					_totalHeight = 1;

				// adding column always increased width by 1
				_totalWidth++;

				// column spans rows 0 to the last row
				var row = 0;
				var height = _totalHeight;

				// column is always added at the end with a width of 1
				var column = _totalWidth - 1;
				var width = 1;

				_grid.Children.AddHorizontal(
					new Label()
					{
						Text = $"{id}: {column}x{row} {width}x{height}"
					}
				);
			}

			void AddVertical()
			{
				// new block gets new id
				var id = _id++;

				// adding row only increases width if no columns exist
				if (_totalWidth == 0)
					_totalWidth = 1;

				// adding row always increased height by 1
				_totalHeight++;

				// row spans columns 0 to the last column
				var column = 0;
				var width = _totalWidth;

				// row is always added at the end with a height of 1
				var row = _totalHeight - 1;
				var height = 1;

				_grid.Children.AddVertical(
					new Label()
					{
						Text = $"{id}: {column}x{row} {width}x{height}"
					}
				);
			}

			void AddRowDef()
			{
				_rowDef++;
				_totalHeight = Math.Max(_rowDef, _totalHeight);

				_grid.RowDefinitions.Add(new RowDefinition());
			}

			void AddColumnDef()
			{
				_colDef++;
				_totalWidth = Math.Max(_colDef, _totalWidth);

				_grid.ColumnDefinitions.Add(new ColumnDefinition());
			}

			[Theory]
			[MemberData(nameof(Operations))]
			public void AddDimensionTheory(string operations)
			{
				_grid = new Grid();

				foreach (var op in operations)
				{
					if (op == 'H')
						AddHorizontal();

					if (op == 'V')
						AddVertical();

					if (op == 'R')
						AddRowDef();

					if (op == 'C')
						AddColumnDef();

					_grid.Layout(new Rect(0, 0, 912, 912));
				}

				Console.WriteLine($"Operations: {string.Join(string.Empty, operations)}");

				var id = 0;
				foreach (var view in _grid.Children.Cast<Label>().OrderBy(o => o.Text))
				{
					var expected = $"{id++}: {Grid.GetColumn(view)}x{Grid.GetRow(view)} {Grid.GetColumnSpan(view)}x{Grid.GetRowSpan(view)}";

					var actual = view.Text;

					Console.WriteLine($"  {expected} == {actual}");
					Assert.True(expected == actual);
				}
			}
		}

		[Fact]
		public void TestBasicVerticalLayout()
		{
			var layout = new Grid();

			var label1 = new Label { IsPlatformEnabled = true };
			var label2 = new Label { IsPlatformEnabled = true };
			var label3 = new Label { IsPlatformEnabled = true };

			layout.Children.AddVertical(new View[] {
				label1,
				label2,
				label3
			});

			layout.Layout(new Rect(0, 0, 912, 912));

			Assert.Equal(912, layout.Width);
			Assert.Equal(912, layout.Height);

			Assert.Equal(new Rect(0, 0, 912, 300), label1.Bounds);
			Assert.Equal(new Rect(0, 306, 912, 300), label2.Bounds);
			Assert.Equal(new Rect(0, 612, 912, 300), label3.Bounds);
		}

		[Fact]
		public void TestBasicHorizontalLayout()
		{
			var layout = new Grid();

			var label1 = new Label { IsPlatformEnabled = true };
			var label2 = new Label { IsPlatformEnabled = true };
			var label3 = new Label { IsPlatformEnabled = true };

			layout.Children.AddHorizontal(new View[] {
				label1,
				label2,
				label3
			});

			layout.Layout(new Rect(0, 0, 912, 912));

			Assert.Equal(912, layout.Width);
			Assert.Equal(912, layout.Height);

			Assert.Equal(new Rect(0, 0, 300, 912), label1.Bounds);
			Assert.Equal(new Rect(306, 0, 300, 912), label2.Bounds);
			Assert.Equal(new Rect(612, 0, 300, 912), label3.Bounds);
		}

		[Fact]
		public void TestVerticalExpandStart()
		{
			var layout = new Grid();

			var label1 = new Label { IsPlatformEnabled = true };
			var label2 = new Label { IsPlatformEnabled = true };

			layout.RowDefinitions = new RowDefinitionCollection {
				new RowDefinition { Height = new GridLength (1, GridUnitType.Star) },
				new RowDefinition { Height = GridLength.Auto},
			};
			layout.Children.Add(label1, 0, 0);
			layout.Children.Add(label2, 0, 1);

			layout.Layout(new Rect(0, 0, 1000, 1000));

			Assert.Equal(1000, layout.Width);
			Assert.Equal(1000, layout.Height);

			Assert.Equal(new Rect(0, 0, 1000, 1000 - 20 - layout.RowSpacing), label1.Bounds);
			Assert.Equal(new Rect(0, 1000 - 20, 1000, 20), label2.Bounds);
		}

		[Fact]
		public void TestHorizontalExpandStart()
		{
			var layout = new Grid();

			var label1 = new Label { IsPlatformEnabled = true };
			var label2 = new Label { IsPlatformEnabled = true };

			layout.ColumnDefinitions = new ColumnDefinitionCollection {
				new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
				new ColumnDefinition { Width = GridLength.Auto },
			};
			layout.Children.Add(label1, 0, 0);
			layout.Children.Add(label2, 1, 0);

			layout.Layout(new Rect(0, 0, 1000, 1000));

			Assert.Equal(1000, layout.Width);
			Assert.Equal(1000, layout.Height);

			Assert.Equal(new Rect(0, 0, 1000 - 106, 1000), label1.Bounds);
			Assert.Equal(new Rect(1000 - 100, 0, 100, 1000), label2.Bounds);
		}

		[Fact]
		public void TestVerticalExpandEnd()
		{
			var layout = new Grid();

			var label1 = new Label { IsPlatformEnabled = true };
			var label2 = new Label { IsPlatformEnabled = true };

			layout.RowDefinitions = new RowDefinitionCollection {
				new RowDefinition { Height = GridLength.Auto},
				new RowDefinition { Height = new GridLength (1, GridUnitType.Star) },
			};
			layout.Children.Add(label1, 0, 0);
			layout.Children.Add(label2, 0, 1);

			layout.Layout(new Rect(0, 0, 1000, 1000));

			Assert.Equal(1000, layout.Width);
			Assert.Equal(1000, layout.Height);

			Assert.Equal(new Rect(0, 0, 1000, 20), label1.Bounds);
			Assert.Equal(new Rect(0, 26, 1000, 1000 - 26), label2.Bounds);
		}

		[Fact]
		public void TestHorizontalExpandEnd()
		{
			var layout = new Grid();

			var label1 = new Label { IsPlatformEnabled = true };
			var label2 = new Label { IsPlatformEnabled = true };

			layout.ColumnDefinitions = new ColumnDefinitionCollection {
				new ColumnDefinition { Width = GridLength.Auto },
				new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
			};

			layout.Children.Add(label1, 0, 0);
			layout.Children.Add(label2, 1, 0);

			layout.Layout(new Rect(0, 0, 1000, 1000));

			Assert.Equal(1000, layout.Width);
			Assert.Equal(1000, layout.Height);

			Assert.Equal(new Rect(0, 0, 100, 1000), label1.Bounds);
			Assert.Equal(new Rect(106, 0, 1000 - 106, 1000), label2.Bounds);
		}

		[Fact]
		public void TestVerticalExpandMiddle()
		{
			var layout = new Grid();

			var label1 = new Label { IsPlatformEnabled = true };
			var label2 = new Label { IsPlatformEnabled = true };
			var label3 = new Label { IsPlatformEnabled = true };

			layout.RowDefinitions = new RowDefinitionCollection {
				new RowDefinition { Height = GridLength.Auto},
				new RowDefinition { Height = new GridLength (1, GridUnitType.Star) },
				new RowDefinition { Height = GridLength.Auto}
			};
			layout.Children.Add(label1, 0, 0);
			layout.Children.Add(label2, 0, 1);
			layout.Children.Add(label3, 0, 2);

			layout.Layout(new Rect(0, 0, 1000, 1000));

			Assert.Equal(1000, layout.Width);
			Assert.Equal(1000, layout.Height);

			Assert.Equal(new Rect(0, 0, 1000, 20), label1.Bounds);
			Assert.Equal(new Rect(0, 26, 1000, 1000 - 52), label2.Bounds);
			Assert.Equal(new Rect(0, 980, 1000, 20), label3.Bounds);
		}

		[Fact]
		public void TestHorizontalExpandMiddle()
		{
			var layout = new Grid();

			var label1 = new Label { IsPlatformEnabled = true };
			var label2 = new Label { IsPlatformEnabled = true };
			var label3 = new Label { IsPlatformEnabled = true };

			layout.ColumnDefinitions = new ColumnDefinitionCollection {
				new ColumnDefinition { Width = GridLength.Auto },
				new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
				new ColumnDefinition { Width = GridLength.Auto },
			};

			layout.Children.Add(label1, 0, 0);
			layout.Children.Add(label2, 1, 0);
			layout.Children.Add(label3, 2, 0);

			layout.Layout(new Rect(0, 0, 1000, 1000));

			Assert.Equal(1000, layout.Width);
			Assert.Equal(1000, layout.Height);

			Assert.Equal(new Rect(0, 0, 100, 1000), label1.Bounds);
			Assert.Equal(new Rect(106, 0, 1000 - 212, 1000), label2.Bounds);
			Assert.Equal(new Rect(900, 0, 100, 1000), label3.Bounds);
		}

		[Fact]
		public void TestTableNoExpand()
		{
			var layout = new Grid();

			var label1 = new Label { IsPlatformEnabled = true };
			var label2 = new Label { IsPlatformEnabled = true };
			var label3 = new Label { IsPlatformEnabled = true };
			var label4 = new Label { IsPlatformEnabled = true };

			layout.Children.Add(label1, 0, 0);
			layout.Children.Add(label2, 1, 0);
			layout.Children.Add(label3, 0, 1);
			layout.Children.Add(label4, 1, 1);

			layout.ColumnDefinitions = new ColumnDefinitionCollection {
				new ColumnDefinition { Width = GridLength.Auto },
				new ColumnDefinition { Width = GridLength.Auto },
			};
			layout.RowDefinitions = new RowDefinitionCollection {
				new RowDefinition { Height = GridLength.Auto},
				new RowDefinition { Height = GridLength.Auto}
			};

			layout.Layout(new Rect(0, 0, 1000, 1000));

			Assert.Equal(1000, layout.Width);
			Assert.Equal(1000, layout.Height);

			Assert.Equal(new Rect(0, 0, 100, 20), label1.Bounds);
			Assert.Equal(new Rect(106, 0, 100, 20), label2.Bounds);
			Assert.Equal(new Rect(0, 26, 100, 20), label3.Bounds);
			Assert.Equal(new Rect(106, 26, 100, 20), label4.Bounds);
		}

		[Fact]
		public void TestTableExpand()
		{
			var layout = new Grid();

			var label1 = new Label { IsPlatformEnabled = true };
			var label2 = new Label { IsPlatformEnabled = true };
			var label3 = new Label { IsPlatformEnabled = true };
			var label4 = new Label { IsPlatformEnabled = true };

			layout.ColumnDefinitions = new ColumnDefinitionCollection {
				new ColumnDefinition { Width = GridLength.Auto },
				new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
			};

			layout.Children.Add(label1, 0, 0);
			layout.Children.Add(label2, 1, 0);
			layout.Children.Add(label3, 0, 1);
			layout.Children.Add(label4, 1, 1);

			layout.Layout(new Rect(0, 0, 1000, 1000));

			Assert.Equal(1000, layout.Width);
			Assert.Equal(1000, layout.Height);

			Assert.Equal(new Rect(0, 0, 100, 497), label1.Bounds);
			Assert.Equal(new Rect(106, 0, 894, 497), label2.Bounds);
			Assert.Equal(new Rect(0, 503, 100, 497), label3.Bounds);
			Assert.Equal(new Rect(106, 503, 894, 497), label4.Bounds);
		}

		[Fact]
		public void TestTableSpan()
		{
			var layout = new Grid();

			var label1 = new Label { IsPlatformEnabled = true };
			var label2 = new Label { IsPlatformEnabled = true };
			var label3 = new Label { IsPlatformEnabled = true };

			layout.Children.Add(label1, 0, 2, 0, 1);
			layout.Children.Add(label2, 0, 1, 1, 2);
			layout.Children.Add(label3, 1, 2, 1, 2);

			layout.ColumnDefinitions = new ColumnDefinitionCollection {
				new ColumnDefinition { Width = GridLength.Auto },
				new ColumnDefinition { Width = GridLength.Auto },
			};
			layout.RowDefinitions = new RowDefinitionCollection {
				new RowDefinition { Height = GridLength.Auto},
				new RowDefinition { Height = GridLength.Auto}
			};

			layout.Layout(new Rect(0, 0, 1000, 1000));

			Assert.Equal(1000, layout.Width);
			Assert.Equal(1000, layout.Height);

			Assert.Equal(new Rect(0, 0, 206, 20), label1.Bounds);
			Assert.Equal(new Rect(0, 26, 100, 20), label2.Bounds);
			Assert.Equal(new Rect(106, 26, 100, 20), label3.Bounds);
		}

		[Fact]
		public void TestTableExpandedSpan()
		{
			var layout = new Grid();

			var label1 = new Label { IsPlatformEnabled = true };
			var label2 = new Label { IsPlatformEnabled = true };
			var label3 = new Label { IsPlatformEnabled = true };

			layout.ColumnDefinitions = new ColumnDefinitionCollection {
				new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
				new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
			};
			layout.RowDefinitions = new RowDefinitionCollection {
				new RowDefinition { Height = GridLength.Auto},
				new RowDefinition { Height = GridLength.Auto}
			};

			layout.Children.Add(label1, 0, 2, 0, 1);
			layout.Children.Add(label2, 0, 1, 1, 2);
			layout.Children.Add(label3, 1, 2, 1, 2);

			layout.Layout(new Rect(0, 0, 1000, 1000));

			Assert.Equal(1000, layout.Width);
			Assert.Equal(1000, layout.Height);

			Assert.Equal(new Rect(0, 0, 1000, 20), label1.Bounds);
			Assert.Equal(new Rect(0, 26, 497, 20), label2.Bounds);
			Assert.Equal(new Rect(503, 26, 497, 20), label3.Bounds);
		}

		[Fact]
		public void TestInvalidSet()
		{
			var layout = new Grid();

			var label1 = new Label { IsPlatformEnabled = true };

			bool thrown = false;

			try
			{
				layout.Children.Add(label1, 2, 1, 0, 1);
			}
			catch (ArgumentOutOfRangeException)
			{
				thrown = true;
			}

			Assert.True(thrown);
		}

		[Fact]
		public void TestCentering()
		{
			var layout = new Grid();

			var label1 = new Label { IsPlatformEnabled = true, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
			layout.ColumnDefinitions = new ColumnDefinitionCollection {
				new ColumnDefinition () {Width = new GridLength (1, GridUnitType.Star)},
			};
			layout.RowDefinitions = new RowDefinitionCollection {
				new RowDefinition () {Height = new GridLength (1,GridUnitType.Star)},
			};

			layout.Children.Add(label1);

			layout.Layout(new Rect(0, 0, 1000, 1000));

			Assert.Equal(new Rect(450, 490, 100, 20), label1.Bounds);
		}

		[Fact]
		public void TestStart()
		{
			var layout = new Grid();

			var label1 = new Label { IsPlatformEnabled = true, HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.StartAndExpand };

			layout.Children.AddVertical(label1);
			layout.ColumnDefinitions = new ColumnDefinitionCollection {
				new ColumnDefinition () {Width = new GridLength (1, GridUnitType.Star)},
			};
			layout.RowDefinitions = new RowDefinitionCollection {
				new RowDefinition () {Height = new GridLength (1,GridUnitType.Star)},
			};

			layout.Layout(new Rect(0, 0, 1000, 1000));

			Assert.Equal(new Rect(0, 0, 100, 20), label1.Bounds);
		}

		[Fact]
		public void TestEnd()
		{
			var layout = new Grid();

			var label1 = new Label { IsPlatformEnabled = true, HorizontalOptions = LayoutOptions.End, VerticalOptions = LayoutOptions.EndAndExpand };

			layout.Children.AddVertical(label1);
			layout.ColumnDefinitions = new ColumnDefinitionCollection {
				new ColumnDefinition () {Width = new GridLength (1, GridUnitType.Star)},
			};
			layout.RowDefinitions = new RowDefinitionCollection {
				new RowDefinition () {Height = new GridLength (1,GridUnitType.Star)},
			};

			layout.Layout(new Rect(0, 0, 1000, 1000));

			Assert.Equal(new Rect(900, 980, 100, 20), label1.Bounds);
		}

		[Fact]
		public void TestDefaultRowSpacing()
		{
			var layout = new Grid();

			bool preferredSizeChanged = false;
			layout.MeasureInvalidated += (sender, args) =>
			{
				preferredSizeChanged = true;
			};

			layout.RowSpacing = layout.RowSpacing;

			Assert.False(preferredSizeChanged);

			layout.RowSpacing = 10;

			Assert.True(preferredSizeChanged);
		}

		[Fact]
		public void TestDefaultColumnSpacing()
		{
			var layout = new Grid();

			bool preferredSizeChanged = false;
			layout.MeasureInvalidated += (sender, args) =>
			{
				preferredSizeChanged = true;
			};

			layout.ColumnSpacing = layout.ColumnSpacing;

			Assert.False(preferredSizeChanged);

			layout.ColumnSpacing = 10;

			Assert.True(preferredSizeChanged);
		}

		[Fact]
		public void TestAddCell()
		{
			var layout = new Grid();
			bool preferredSizeChanged = false;
			layout.MeasureInvalidated += (sender, args) => preferredSizeChanged = true;

			Assert.False(preferredSizeChanged);

			layout.Children.Add(new Label(), 0, 0);

			Assert.True(preferredSizeChanged);
		}

		[Fact]
		public void TestMoveCell()
		{
			var layout = new Grid();
			var label = new Label();
			layout.Children.Add(label, 0, 0);

			bool preferredSizeChanged = false;
			layout.MeasureInvalidated += (sender, args) =>
			{
				preferredSizeChanged = true;
			};

			Assert.False(preferredSizeChanged);
			Grid.SetRow(label, 2);
			Assert.True(preferredSizeChanged);

			preferredSizeChanged = false;
			Assert.False(preferredSizeChanged);
			Grid.SetColumn(label, 2);
			Assert.True(preferredSizeChanged);

			preferredSizeChanged = false;
			Assert.False(preferredSizeChanged);
			Grid.SetRowSpan(label, 2);
			Assert.True(preferredSizeChanged);

			preferredSizeChanged = false;
			Assert.False(preferredSizeChanged);
			Grid.SetColumnSpan(label, 2);
			Assert.True(preferredSizeChanged);
		}

		[Fact]
		public void TestInvalidBottomAdd()
		{
			var layout = new Grid();

			Assert.Throws<ArgumentOutOfRangeException>(() => layout.Children.Add(new View(), 0, 1, 1, 0));
		}

		[Fact]
		public void TestZeroSizeConstraints()
		{
			var layout = new Grid();

			Assert.Equal(new Size(0, 0), layout.Measure(0, 0).Request);
			Assert.Equal(new Size(0, 0), layout.Measure(0, 10).Request);
			Assert.Equal(new Size(0, 0), layout.Measure(10, 0).Request);
		}

		[Fact]
		public void TestSizeRequest()
		{
			var layout = new Grid { IsPlatformEnabled = true };
			layout.Children.AddVertical(new[] {
				new View {IsPlatformEnabled = true},
				new View {IsPlatformEnabled = true},
				new View {IsPlatformEnabled = true}
			});

			var result = layout.Measure(double.PositiveInfinity, double.PositiveInfinity).Request;
			Assert.Equal(new Size(100, 72), result);
		}

		[Fact]
		public void TestLimitedSizeRequest()
		{
			var layout = new Grid { IsPlatformEnabled = true };
			layout.Children.AddVertical(new[] {
				new View {IsPlatformEnabled = true},
				new View {IsPlatformEnabled = true},
				new View {IsPlatformEnabled = true}
			});

			var result = layout.Measure(10, 10).Request;
			Assert.Equal(new Size(100, 72), result);
		}

		[Fact]
		public void TestLimitedWidthSizeRequest()
		{
			var layout = new Grid { IsPlatformEnabled = true };
			layout.Children.AddVertical(new[] {
				new View {IsPlatformEnabled = true},
				new View {IsPlatformEnabled = true},
				new View {IsPlatformEnabled = true}
			});

			var result = layout.Measure(10, double.PositiveInfinity).Request;
			Assert.Equal(new Size(100, 72), result);
		}

		[Fact]
		public void TestLimitedHeightSizeRequest()
		{

			var layout = new Grid { IsPlatformEnabled = true };
			layout.Children.AddVertical(new[] {
				new View {IsPlatformEnabled = true},
				new View {IsPlatformEnabled = true},
				new View {IsPlatformEnabled = true}
			});

			var result = layout.Measure(double.PositiveInfinity, 10).Request;
			Assert.Equal(new Size(100, 72), result);
		}

		[Fact]
		public void IgnoresInvisibleChildren()
		{
			var layout = new Grid();

			var label1 = new Label { IsVisible = false, IsPlatformEnabled = true, VerticalOptions = LayoutOptions.FillAndExpand };
			var label2 = new Label { IsPlatformEnabled = true };

			layout.Children.AddVertical(label1);
			layout.Children.AddVertical(label2);

			layout.ColumnDefinitions = new ColumnDefinitionCollection {
				new ColumnDefinition { Width = GridLength.Auto },
			};
			layout.RowDefinitions = new RowDefinitionCollection {
				new RowDefinition { Height = GridLength.Auto},
				new RowDefinition { Height = GridLength.Auto},
			};

			layout.Layout(new Rect(0, 0, 1000, 1000));

			Assert.Equal(1000, layout.Width);
			Assert.Equal(1000, layout.Height);

			Assert.Equal(new Rect(0, 0, -1, -1), label1.Bounds);
			Assert.Equal(new Rect(0, 6, 100, 20), label2.Bounds);
		}

		[Fact]
		public void TestSizeRequestWithPadding()
		{
			var layout = new Grid { IsPlatformEnabled = true, Padding = new Thickness(20, 10, 15, 5) };
			layout.Children.AddVertical(new[] {
				new View {IsPlatformEnabled = true},
				new View {IsPlatformEnabled = true},
				new View {IsPlatformEnabled = true}
			});

			var result = layout.Measure(double.PositiveInfinity, double.PositiveInfinity).Request;
			Assert.Equal(new Size(135, 87), result);
		}

		[Fact]
		public void InvalidCallsToStaticMethods()
		{
			var label = new Label();
			Grid.SetRow(label, -1);
			Grid.SetColumn(label, -1);
			Grid.SetRowSpan(label, 0);
			Grid.SetColumnSpan(label, 0);

			Assert.NotEqual(-1, Grid.GetRow(label));
			Assert.NotEqual(-1, Grid.GetColumn(label));
			Assert.NotEqual(0, Grid.GetRowSpan(label));
			Assert.NotEqual(0, Grid.GetColumnSpan(label));
		}

		[Fact]
		public void TestAddedBP()
		{
			var labela0 = new Label { IsPlatformEnabled = true };
			var labela1 = new Label { IsPlatformEnabled = true };
			Grid.SetColumn(labela1, 1);
			var labelb1 = new Label { IsPlatformEnabled = true };
			Grid.SetRow(labelb1, 1);
			Grid.SetColumn(labelb1, 1);
			var labelc = new Label { IsPlatformEnabled = true };
			Grid.SetRow(labelc, 2);
			Grid.SetColumnSpan(labelc, 2);

			var layout = new Grid
			{
				Children = {
					labela0,
					labela1,
					labelb1,
					labelc
				}
			};

			layout.ColumnDefinitions = new ColumnDefinitionCollection {
				new ColumnDefinition { Width = GridLength.Auto },
				new ColumnDefinition { Width = GridLength.Auto },
			};
			layout.RowDefinitions = new RowDefinitionCollection {
				new RowDefinition { Height = GridLength.Auto},
				new RowDefinition { Height = GridLength.Auto},
				new RowDefinition { Height = GridLength.Auto},
			};

			layout.Layout(new Rect(0, 0, 1000, 1000));

			Assert.Equal(1000, layout.Width);
			Assert.Equal(1000, layout.Height);

			Assert.Equal(new Rect(0, 0, 100, 20), labela0.Bounds);
			Assert.Equal(new Rect(106, 0, 100, 20), labela1.Bounds);
			Assert.Equal(new Rect(106, 26, 100, 20), labelb1.Bounds);
			Assert.Equal(new Rect(0, 52, 206, 20), labelc.Bounds);
		}

		[Fact]
		public void Remove()
		{
			var labela0 = new Label { IsPlatformEnabled = true };
			var labela1 = new Label { IsPlatformEnabled = true };
			Grid.SetColumn(labela1, 1);
			var labelb1 = new Label { IsPlatformEnabled = true };
			Grid.SetRow(labelb1, 1);
			Grid.SetColumn(labelb1, 1);
			var labelc = new Label { IsPlatformEnabled = true };
			Grid.SetRow(labelc, 2);
			Grid.SetColumnSpan(labelc, 2);

			var layout = new Grid
			{
				Children = {
					labela0,
					labela1,
					labelb1,
					labelc
				}
			};

			layout.Children.Remove(labela0);
			Assert.DoesNotContain(labela0, ((IElementController)layout).LogicalChildren);
		}

		[Fact]
		public void TestAbsoluteLayout()
		{
			var layout = new Grid();

			var label1 = new Label { IsPlatformEnabled = true };
			var label2 = new Label { IsPlatformEnabled = true };
			var label3 = new Label { IsPlatformEnabled = true };

			layout.ColumnDefinitions = new ColumnDefinitionCollection {
				new ColumnDefinition {Width = new GridLength (150)},
				new ColumnDefinition {Width = new GridLength (150)},
				new ColumnDefinition {Width = new GridLength (150)},
			};
			layout.RowDefinitions = new RowDefinitionCollection {
				new RowDefinition {Height = new GridLength (30)},
				new RowDefinition {Height = new GridLength (30)},
				new RowDefinition {Height = new GridLength (30)},
			};
			layout.Children.Add(label1, 0, 0);
			layout.Children.Add(label2, 1, 1);
			layout.Children.Add(label3, 2, 2);


			layout.Layout(new Rect(0, 0, 1000, 1000));

			Assert.Equal(1000, layout.Width);
			Assert.Equal(1000, layout.Height);

			Assert.Equal(new Rect(0, 0, 150, 30), label1.Bounds);
			Assert.Equal(new Rect(156, 36, 150, 30), label2.Bounds);
			Assert.Equal(new Rect(312, 72, 150, 30), label3.Bounds);
		}

		[Fact]
		public void TestAbsoluteLayoutWithSpans()
		{
			var layout = new Grid();

			var label1 = new Label { IsPlatformEnabled = true };
			var label2 = new Label { IsPlatformEnabled = true };
			var label3 = new Label { IsPlatformEnabled = true };

			layout.ColumnDefinitions = new ColumnDefinitionCollection {
				new ColumnDefinition {Width = new GridLength (150)},
				new ColumnDefinition {Width = new GridLength (150)},
				new ColumnDefinition {Width = new GridLength (150)},
			};
			layout.RowDefinitions = new RowDefinitionCollection {
				new RowDefinition {Height = new GridLength (30)},
				new RowDefinition {Height = new GridLength (30)},
				new RowDefinition {Height = new GridLength (30)},
			};
			layout.Children.Add(label1, 0, 2, 0, 1);
			layout.Children.Add(label2, 2, 3, 0, 2);
			layout.Children.Add(label3, 1, 2);


			layout.Layout(new Rect(0, 0, 1000, 1000));

			Assert.Equal(1000, layout.Width);
			Assert.Equal(1000, layout.Height);

			Assert.Equal(new Rect(0, 0, 306, 30), label1.Bounds);
			Assert.Equal(new Rect(312, 0, 150, 66), label2.Bounds);
			Assert.Equal(new Rect(156, 72, 150, 30), label3.Bounds);
		}

		[Fact]
		public void TestStarLayout()
		{
			var layout = new Grid();

			var label1 = new Label { IsPlatformEnabled = true };
			var label2 = new Label { IsPlatformEnabled = true };
			var label3 = new Label { IsPlatformEnabled = true };

			layout.ColumnDefinitions = new ColumnDefinitionCollection {
				new ColumnDefinition {Width = new GridLength (1, GridUnitType.Star)},
				new ColumnDefinition {Width = new GridLength (1, GridUnitType.Star)},
				new ColumnDefinition {Width = new GridLength (1, GridUnitType.Star)},
			};
			layout.RowDefinitions = new RowDefinitionCollection {
				new RowDefinition {Height = new GridLength (1, GridUnitType.Star)},
				new RowDefinition {Height = new GridLength (1, GridUnitType.Star)},
				new RowDefinition {Height = new GridLength (1, GridUnitType.Star)},
			};
			layout.Children.Add(label1, 0, 0);
			layout.Children.Add(label2, 1, 1);
			layout.Children.Add(label3, 2, 2);

			var request = layout.Measure(1002, 462);
			Assert.Equal(312, request.Request.Width);
			Assert.Equal(72, request.Request.Height);

			layout.Layout(new Rect(0, 0, 1002, 462));
			Assert.Equal(1002, layout.Width);
			Assert.Equal(462, layout.Height);

			Assert.Equal(new Rect(0, 0, 330, 150), label1.Bounds);
			Assert.Equal(new Rect(336, 156, 330, 150), label2.Bounds);
			Assert.Equal(new Rect(672, 312, 330, 150), label3.Bounds);
		}

		[Fact]
		public void TestStarLayoutWithSpans()
		{
			var layout = new Grid();

			var label1 = new Label { IsPlatformEnabled = true };
			var label2 = new Label { IsPlatformEnabled = true };
			var label3 = new Label { IsPlatformEnabled = true };

			layout.ColumnDefinitions = new ColumnDefinitionCollection {
				new ColumnDefinition {Width = new GridLength (1, GridUnitType.Star)},
				new ColumnDefinition {Width = new GridLength (1, GridUnitType.Star)},
				new ColumnDefinition {Width = new GridLength (1, GridUnitType.Star)},
			};
			layout.RowDefinitions = new RowDefinitionCollection {
				new RowDefinition {Height = new GridLength (1, GridUnitType.Star)},
				new RowDefinition {Height = new GridLength (1, GridUnitType.Star)},
				new RowDefinition {Height = new GridLength (1, GridUnitType.Star)},
			};
			layout.Children.Add(label1, 0, 2, 0, 1);
			layout.Children.Add(label2, 2, 3, 0, 2);
			layout.Children.Add(label3, 1, 2);

			layout.Layout(new Rect(0, 0, 1002, 462));

			Assert.Equal(1002, layout.Width);
			Assert.Equal(462, layout.Height);

			Assert.Equal(new Rect(0, 0, 666, 150), label1.Bounds);
			Assert.Equal(new Rect(672, 0, 330, 306), label2.Bounds);
			Assert.Equal(new Rect(336, 312, 330, 150), label3.Bounds);
		}

		[Fact]
		public void TestAutoLayout()
		{
			var layout = new Grid();

			var label1 = new Label { IsPlatformEnabled = true };
			var label2 = new Label { IsPlatformEnabled = true };
			var label3 = new Label { IsPlatformEnabled = true };

			layout.ColumnDefinitions = new ColumnDefinitionCollection {
				new ColumnDefinition {Width = GridLength.Auto},
				new ColumnDefinition {Width = GridLength.Auto},
				new ColumnDefinition {Width = GridLength.Auto},
			};
			layout.RowDefinitions = new RowDefinitionCollection {
				new RowDefinition {Height = GridLength.Auto},
				new RowDefinition {Height = GridLength.Auto},
				new RowDefinition {Height = GridLength.Auto},
			};
			layout.Children.Add(label1, 0, 0);
			layout.Children.Add(label2, 1, 1);
			layout.Children.Add(label3, 2, 2);


			layout.Layout(new Rect(0, 0, 1000, 1000));

			Assert.Equal(1000, layout.Width);
			Assert.Equal(1000, layout.Height);

			Assert.Equal(new Rect(0, 0, 100, 20), label1.Bounds);
			Assert.Equal(new Rect(106, 26, 100, 20), label2.Bounds);
			Assert.Equal(new Rect(212, 52, 100, 20), label3.Bounds);
		}

		[Fact]
		public void TestAutoLayoutWithSpans()
		{
			var layout = new Grid();

			var label1 = new Label { IsPlatformEnabled = true, WidthRequest = 150, Text = "label1" };
			var label2 = new Label { IsPlatformEnabled = true, HeightRequest = 50, Text = "label2" };
			var label3 = new Label { IsPlatformEnabled = true, Text = "label3" };

			layout.ColumnDefinitions = new ColumnDefinitionCollection {
				new ColumnDefinition {Width = GridLength.Auto},
				new ColumnDefinition {Width = GridLength.Auto},
				new ColumnDefinition {Width = GridLength.Auto},
			};
			layout.RowDefinitions = new RowDefinitionCollection {
				new RowDefinition {Height = GridLength.Auto},
				new RowDefinition {Height = GridLength.Auto},
				new RowDefinition {Height = GridLength.Auto},
			};
			layout.Children.Add(label1, 0, 2, 0, 1);
			layout.Children.Add(label2, 2, 3, 0, 2);
			layout.Children.Add(label3, 1, 2);

			layout.Layout(new Rect(0, 0, 1002, 462));

			Assert.Equal(1002, layout.Width);
			Assert.Equal(462, layout.Height);

			Assert.Equal(new Rect(0, 0, 150, 20), label1.Bounds);
			Assert.Equal(new Rect(156, 0, 100, 50), label2.Bounds);
			Assert.Equal(new Rect(50, 56, 100, 20), label3.Bounds);
		}

		[Fact]
		public void AutoLayoutWithComplexSpans()
		{
			var layout = new Grid();

			var label1 = new Label { IsPlatformEnabled = true };
			var label2 = new Label { IsPlatformEnabled = true };
			var label3 = new Label { IsPlatformEnabled = true };
			var label4 = new Label { IsPlatformEnabled = true, WidthRequest = 206 };
			var label5 = new Label { IsPlatformEnabled = true, WidthRequest = 312 };
			var label6 = new Label { IsPlatformEnabled = true, WidthRequest = 312 };

			layout.ColumnDefinitions = new ColumnDefinitionCollection {
				new ColumnDefinition {Width = GridLength.Auto},
				new ColumnDefinition {Width = GridLength.Auto},
				new ColumnDefinition {Width = GridLength.Auto},
				new ColumnDefinition {Width = GridLength.Auto},
				new ColumnDefinition {Width = GridLength.Auto},
			};

			layout.Children.Add(label1, 0, 0);
			layout.Children.Add(label2, 1, 0);
			layout.Children.Add(label3, 4, 0);
			layout.Children.Add(label4, 2, 4, 0, 1);
			layout.Children.Add(label5, 0, 3, 0, 1);
			layout.Children.Add(label6, 2, 6, 0, 1);

			layout.Layout(new Rect(0, 0, 1000, 500));

			Assert.Equal(100, layout.ColumnDefinitions[0].ActualWidth);
			Assert.Equal(100, layout.ColumnDefinitions[1].ActualWidth);
			Assert.Equal(100, layout.ColumnDefinitions[2].ActualWidth);
			Assert.Equal(100, layout.ColumnDefinitions[3].ActualWidth);
			Assert.Equal(100, layout.ColumnDefinitions[4].ActualWidth);
		}

		[Fact]
		public void AutoLayoutExpandColumns()
		{
			var layout = new Grid();

			var label1 = new Label { IsPlatformEnabled = true };
			var label2 = new Label { IsPlatformEnabled = true };
			var label3 = new Label { IsPlatformEnabled = true, WidthRequest = 300 };

			layout.ColumnDefinitions = new ColumnDefinitionCollection {
				new ColumnDefinition { Width = GridLength.Auto },
				new ColumnDefinition { Width = GridLength.Auto },
			};

			layout.Children.Add(label1, 0, 0);
			layout.Children.Add(label2, 1, 0);
			layout.Children.Add(label3, 0, 2, 0, 1);

			layout.Layout(new Rect(0, 0, 1000, 500));

			Assert.Equal(100, layout.ColumnDefinitions[0].ActualWidth);
			Assert.Equal(194, layout.ColumnDefinitions[1].ActualWidth);
		}

		[Fact]
		public void GridHasDefaultDefinitions()
		{
			var grid = new Grid();
			Assert.NotNull(grid.ColumnDefinitions);
			Assert.NotNull(grid.RowDefinitions);
		}

		[Fact]
		public void DefaultDefinitionsArentSharedAccrossInstances()
		{
			var grid0 = new Grid();
			var coldefs = grid0.ColumnDefinitions;
			var rowdefs = grid0.RowDefinitions;

			var grid1 = new Grid();
			Assert.NotSame(grid0, grid1);
			Assert.NotSame(coldefs, grid1.ColumnDefinitions);
			Assert.NotSame(rowdefs, grid1.RowDefinitions);
		}

		[Fact]
		public void ChildrenLayoutRespectAlignment()
		{
			var grid = new Grid
			{
				ColumnDefinitions = { new ColumnDefinition { Width = new GridLength(300) } },
				RowDefinitions = { new RowDefinition { Height = new GridLength(100) } },
			};
			var label = new Label
			{
				IsPlatformEnabled = true,
				VerticalOptions = LayoutOptions.Center,
				HorizontalOptions = LayoutOptions.End,
			};

			grid.Children.Add(label);
			grid.Layout(new Rect(0, 0, 500, 500));

			Assert.Equal(new Rect(200, 40, 100, 20), label.Bounds);
		}

		[Fact]
		public void BothChildrenPropertiesUseTheSameBackendStore()
		{
			var view = new View();
			var grid = new Grid();
			Assert.Empty(grid.Children);
			(grid as Compatibility.Layout<View>).Children.Add(view);
			Assert.Single(grid.Children);
			Assert.Single((grid as Compatibility.Layout<View>).Children);
			Assert.Same(view, (grid as Compatibility.Layout<View>).Children.First());
			Assert.Same(view, grid.Children.First());
		}

		[Fact]
		//Issue 1384
		public void ImageInAutoCellIsProperlyConstrained()
		{
			var content = new Image
			{
				Aspect = Aspect.AspectFit,
				IsPlatformEnabled = true
			};
			var grid = new Grid
			{
				IsPlatformEnabled = true,
				BackgroundColor = Colors.Red,
				VerticalOptions = LayoutOptions.Start,
				Children = {
					content
				},
				RowDefinitions = { new RowDefinition { Height = GridLength.Auto } },
				ColumnDefinitions = { new ColumnDefinition { Width = GridLength.Auto } }
			};
			var view = new ContentView
			{
				IsPlatformEnabled = true,
				Content = grid,
			};
			view.Layout(new Rect(0, 0, 100, 100));
			Assert.Equal(100, grid.Width);
			Assert.Equal(20, grid.Height);

			view.Layout(new Rect(0, 0, 50, 50));
			Assert.Equal(50, grid.Width);
			Assert.Equal(10, grid.Height);
		}

		[Fact]
		//Issue 1384
		public void ImageInStarCellIsProperlyConstrained()
		{
			var content = new Image
			{
				Aspect = Aspect.AspectFit,
				MinimumHeightRequest = 10,
				MinimumWidthRequest = 50,
				IsPlatformEnabled = true
			};
			var grid = new Grid
			{
				IsPlatformEnabled = true,
				BackgroundColor = Colors.Red,
				VerticalOptions = LayoutOptions.Start,
				Children = {
					content
				}
			};
			var view = new ContentView
			{
				IsPlatformEnabled = true,
				Content = grid,
			};
			view.Layout(new Rect(0, 0, 100, 100));
			Assert.Equal(100, grid.Width);
			Assert.Equal(20, grid.Height);

			view.Layout(new Rect(0, 0, 50, 50));
			Assert.Equal(50, grid.Width);
			Assert.Equal(10, grid.Height);
		}

		[Fact]
		public void SizeRequestForStar()
		{
			var grid = new Grid
			{
				RowDefinitions = new RowDefinitionCollection {
					new RowDefinition {Height = new GridLength (1, GridUnitType.Star)},
					new RowDefinition {Height = GridLength.Auto},
				},
				ColumnDefinitions = new ColumnDefinitionCollection {
					new ColumnDefinition {Width = new GridLength (1, GridUnitType.Star)},
					new ColumnDefinition {Width = GridLength.Auto},
				}
			};
			grid.Children.Add(new Label { BackgroundColor = Colors.Lime, Text = "Foo", IsPlatformEnabled = true });
			grid.Children.Add(new Label { Text = "Bar", IsPlatformEnabled = true }, 0, 1);
			grid.Children.Add(new Label { Text = "Baz", HorizontalTextAlignment = TextAlignment.End, IsPlatformEnabled = true }, 1, 0);
			grid.Children.Add(new Label { Text = "Qux", HorizontalTextAlignment = TextAlignment.End, IsPlatformEnabled = true }, 1, 1);

			var request = grid.Measure(double.PositiveInfinity, double.PositiveInfinity);
			Assert.Equal(206, request.Request.Width);
			Assert.Equal(46, request.Request.Height);

			Assert.Equal(106, request.Minimum.Width);
			Assert.Equal(26, request.Minimum.Height);
			//
		}

		[Fact]
		//Issue 1497
		public void StarRowsShouldOccupyTheSpace()
		{
			var label = new Label
			{
				IsPlatformEnabled = true,
			};
			var Button = new Button
			{
				HorizontalOptions = LayoutOptions.FillAndExpand,
				VerticalOptions = LayoutOptions.EndAndExpand,
				IsPlatformEnabled = true,
			};
			var grid = new Grid
			{
				RowDefinitions = new RowDefinitionCollection {
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = new GridLength (1, GridUnitType.Star) },
				},
				ColumnDefinitions = new ColumnDefinitionCollection {
					new ColumnDefinition {Width = new GridLength(1, GridUnitType.Star)},
				},
				IsPlatformEnabled = true,
			};

			grid.Children.Add(label);
			grid.Children.Add(Button, 0, 1);

			grid.Layout(new Rect(0, 0, 300, 300));
			Assert.Equal(new Rect(0, 280, 300, 20), Button.Bounds);
		}

		[Fact]
		public void StarColumnsWithSpansDoNotExpandAutos()
		{
			var grid = new Grid
			{
				RowDefinitions = {
					new RowDefinition {Height = GridLength.Auto},
					new RowDefinition {Height = GridLength.Auto},
				},
				ColumnDefinitions = {
					new ColumnDefinition {Width = new GridLength (1, GridUnitType.Auto)},
					new ColumnDefinition {Width = new GridLength (1, GridUnitType.Auto)},
					new ColumnDefinition {Width = new GridLength (1, GridUnitType.Star)}
				},
				IsPlatformEnabled = true
			};

			var spanBox = new BoxView { WidthRequest = 70, HeightRequest = 20, IsPlatformEnabled = true };
			var box1 = new BoxView { WidthRequest = 20, HeightRequest = 20, IsPlatformEnabled = true };
			var box2 = new BoxView { WidthRequest = 20, HeightRequest = 20, IsPlatformEnabled = true };
			var box3 = new BoxView { WidthRequest = 20, HeightRequest = 20, IsPlatformEnabled = true };

			grid.Children.Add(spanBox, 0, 3, 0, 1);
			grid.Children.Add(box1, 0, 1);
			grid.Children.Add(box2, 1, 1);
			grid.Children.Add(box3, 2, 1);

			grid.Layout(new Rect(0, 0, 300, 46));

			Assert.Equal(new Rect(0, 0, 300, 20), spanBox.Bounds);
			Assert.Equal(new Rect(0, 26, 20, 20), box1.Bounds);
			Assert.Equal(new Rect(26, 26, 20, 20), box2.Bounds);
			Assert.Equal(new Rect(52, 26, 248, 20), box3.Bounds);
		}

		static SizeRequest GetResizableSize(VisualElement view, double widthconstraint, double heightconstraint)
		{
			if (!(view is Editor))
				return new SizeRequest(new Size(100, 20));
			if (widthconstraint < 100)
				return new SizeRequest(new Size(widthconstraint, 2000 / widthconstraint));
			if (heightconstraint < 20)
				return new SizeRequest(new Size(2000 / heightconstraint, heightconstraint));
			return new SizeRequest(new Size(100, 20));
		}

		[Fact]
		//Issue 1893
		public void EditorSpanningOnMultipleAutoRows()
		{
			MockPlatformSizeService.Current.GetPlatformSizeFunc = GetResizableSize;

			var grid0 = new Grid
			{
				ColumnDefinitions = {
					new ColumnDefinition { Width = GridLength.Auto },
					new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
				},
				RowDefinitions = {
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Auto },
				},
				IsPlatformEnabled = true,
			};

			var label0 = new Label { IsPlatformEnabled = true };
			var editor0 = new Editor { IsPlatformEnabled = true };
			grid0.Children.Add(label0, 0, 0);
			grid0.Children.Add(editor0, 1, 2, 0, 2);

			grid0.Layout(new Rect(0, 0, 156, 200));
			Assert.Equal(new Rect(106, 0, 50, 40), editor0.Bounds);

			var grid1 = new Grid
			{
				ColumnDefinitions = {
					new ColumnDefinition { Width = GridLength.Auto },
					new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) },
				},
				RowDefinitions = {
					new RowDefinition { Height = GridLength.Auto },
				},
				IsPlatformEnabled = true,
			};

			var label1 = new Label { IsPlatformEnabled = true };
			var editor1 = new Editor { IsPlatformEnabled = true };
			grid1.Children.Add(label1, 0, 0);
			grid1.Children.Add(editor1, 1, 0);

			grid1.Layout(new Rect(0, 0, 156, 200));
			Assert.Equal(new Rect(106, 0, 50, 40), editor1.Bounds);
		}

		[Fact]
		public void WidthBoundRequestRespected()
		{
			MockPlatformSizeService.Current.GetPlatformSizeFunc = GetResizableSize;

			var grid = new Grid
			{
				ColumnDefinitions = {
					new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
					new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
				},
				RowDefinitions = {
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Auto },
				},
				IsPlatformEnabled = true,
				RowSpacing = 0,
				ColumnSpacing = 0,
			};

			var topLabel = new Editor { IsPlatformEnabled = true };
			var leftLabel = new Label { IsPlatformEnabled = true, WidthRequest = 10 };
			var rightLabel = new Label { IsPlatformEnabled = true, WidthRequest = 10 };

			grid.Children.Add(topLabel, 0, 2, 0, 1);
			grid.Children.Add(leftLabel, 0, 1);
			grid.Children.Add(rightLabel, 1, 1);

			var unboundRequest = grid.Measure(double.PositiveInfinity, double.PositiveInfinity);
			var widthBoundRequest = grid.Measure(50, double.PositiveInfinity);

			Assert.Equal(new SizeRequest(new Size(20, 120), new Size(0, 120)), unboundRequest);
			Assert.Equal(new SizeRequest(new Size(50, 60), new Size(0, 60)), widthBoundRequest);
		}

		[Fact]
		//https://bugzilla.xamarin.com/show_bug.cgi?id=31608
		public void ColAndRowDefinitionsAreActuallyBindable()
		{
			var rowdef = new RowDefinition();
			rowdef.SetBinding(RowDefinition.HeightProperty, "Height");
			var grid = new Grid
			{
				RowDefinitions = new RowDefinitionCollection { rowdef },
			};
			Assert.Equal(RowDefinition.HeightProperty.DefaultValue, rowdef.Height);
			grid.BindingContext = new { Height = 32 };
			Assert.Equal(new GridLength(32), rowdef.Height);
		}

		[Fact]
		//https://bugzilla.xamarin.com/show_bug.cgi?id=31967
		public void ChangingRowHeightViaBindingTriggersRedraw()
		{
			var rowdef = new RowDefinition();
			rowdef.SetBinding(RowDefinition.HeightProperty, "Height");
			var grid = new Grid
			{
				//				RowDefinitions = new RowDefinitionCollection {
				//					new RowDefinition { Height = GridLength.Auto },
				//					rowdef
				//				},
				RowSpacing = 0,
				IsPlatformEnabled = true,
			};
			grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
			grid.RowDefinitions.Add(rowdef);

			var label0 = new Label { IsPlatformEnabled = true };
			Grid.SetRow(label0, 0);
			var label1 = new Label { IsPlatformEnabled = true };
			Grid.SetRow(label1, 1);

			grid.BindingContext = new { Height = 0 };
			grid.Children.Add(label0);
			grid.Children.Add(label1);

			Assert.Equal(new SizeRequest(new Size(100, 20), new Size(0, 20)), grid.Measure(double.PositiveInfinity, double.PositiveInfinity));
			grid.BindingContext = new { Height = 42 };
			Assert.Equal(new SizeRequest(new Size(100, 62), new Size(0, 62)), grid.Measure(double.PositiveInfinity, double.PositiveInfinity));
		}

		[Fact]
		public void InvalidationBlockedForAbsoluteCell()
		{
			var grid = new Grid()
			{
				IsPlatformEnabled = true,
				RowDefinitions = {
					new RowDefinition { Height = new GridLength (100, GridUnitType.Absolute) }
				},
				ColumnDefinitions = {
					new ColumnDefinition { Width = new GridLength (200, GridUnitType.Absolute) }
				}
			};

			var label = new Label { IsPlatformEnabled = true };
			grid.Children.Add(label);

			bool invalidated = false;
			grid.MeasureInvalidated += (sender, args) =>
			{
				invalidated = true;
			};

			label.Text = "Testing";

			Assert.False(invalidated);
		}

		[Fact]
		//https://github.com/xamarin/Microsoft.Maui.Controls/issues/4933
		public void GridHeightCorrectWhenAspectFitImageGetsShrinked()
		{
			var contentGrid = new Grid
			{
				HorizontalOptions = LayoutOptions.FillAndExpand,
				VerticalOptions = LayoutOptions.Center,
				RowDefinitions = new RowDefinitionCollection()
				{
					new RowDefinition(){Height = GridLength.Auto}
				}
			};
			//image will have "EVERYTHING IS 100 x 20" size so grid should shrink it and itself to 50x10
			contentGrid.Children.Add(new Image() { IsPlatformEnabled = true }, 0, 0);
			var measurement = contentGrid.Measure(50, 100);
			Assert.Equal(50, measurement.Request.Width);
			Assert.Equal(10, measurement.Request.Height);
		}

		[Fact]
		public void MinimumWidthRequestInAutoCells()
		{
			var boxRow0Column0 = new BoxView
			{
				MinimumWidthRequest = 50,
				WidthRequest = 200,
				IsPlatformEnabled = true
			};
			var boxRow1Column0 = new BoxView
			{
				MinimumWidthRequest = 50,
				WidthRequest = 200,
				IsPlatformEnabled = true
			};

			var boxRow0Column1 = new BoxView
			{
				WidthRequest = 800,
				IsPlatformEnabled = true
			};
			var boxRow1Column1 = new BoxView
			{
				WidthRequest = 800,
				IsPlatformEnabled = true
			};

			var grid = new Grid
			{
				IsPlatformEnabled = true,
				BackgroundColor = Colors.Red
			};

			grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
			grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

			grid.Children.Add(boxRow0Column0, 0, 0);
			grid.Children.Add(boxRow1Column0, 0, 1);
			grid.Children.Add(boxRow1Column1, 1, 1);
			grid.Children.Add(boxRow0Column1, 1, 0);

			var view = new ContentView
			{
				IsPlatformEnabled = true,
				Content = grid,
			};
			view.Layout(new Rect(0, 0, 800, 800));


			Assert.Equal(boxRow0Column0.MinimumWidthRequest, boxRow0Column0.Width);
			Assert.Equal(boxRow1Column0.MinimumWidthRequest, boxRow1Column0.Width);
		}


		[Fact]
		public void MinimumHeightRequestInAutoCells()
		{
			var boxRow0Column0 = new BoxView
			{
				MinimumHeightRequest = 50,
				HeightRequest = 800,
				IsPlatformEnabled = true
			};
			var boxRow1Column0 = new BoxView
			{
				HeightRequest = 800,
				IsPlatformEnabled = true
			};

			var boxRow0Column1 = new BoxView
			{
				MinimumHeightRequest = 50,
				HeightRequest = 800,
				IsPlatformEnabled = true
			};
			var boxRow1Column1 = new BoxView
			{
				HeightRequest = 800,
				IsPlatformEnabled = true
			};

			var grid = new Grid
			{
				IsPlatformEnabled = true,
				BackgroundColor = Colors.Red
			};

			grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
			grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

			grid.Children.Add(boxRow0Column0, 0, 0);
			grid.Children.Add(boxRow1Column0, 0, 1);
			grid.Children.Add(boxRow1Column1, 1, 1);
			grid.Children.Add(boxRow0Column1, 1, 0);

			var view = new ContentView
			{
				IsPlatformEnabled = true,
				Content = grid,
			};
			view.Layout(new Rect(0, 0, 800, 800));

			Assert.Equal(boxRow0Column0.MinimumHeightRequest, boxRow0Column0.Height);
			Assert.Equal(boxRow0Column1.MinimumHeightRequest, boxRow0Column1.Height);
		}

		// because the constraint is internal, we need this
		public enum HackLayoutConstraint
		{
			None = LayoutConstraint.None,
			VerticallyFixed = LayoutConstraint.VerticallyFixed,
			HorizontallyFixed = LayoutConstraint.HorizontallyFixed,
			Fixed = LayoutConstraint.Fixed
		}

		[Theory]
		[InlineData(HackLayoutConstraint.None, GridUnitType.Absolute, GridUnitType.Absolute, true)]
		[InlineData(HackLayoutConstraint.None, GridUnitType.Star, GridUnitType.Absolute, false)]
		[InlineData(HackLayoutConstraint.None, GridUnitType.Absolute, GridUnitType.Star, false)]
		[InlineData(HackLayoutConstraint.None, GridUnitType.Auto, GridUnitType.Absolute, false)]
		[InlineData(HackLayoutConstraint.None, GridUnitType.Absolute, GridUnitType.Auto, false)]
		[InlineData(HackLayoutConstraint.None, GridUnitType.Star, GridUnitType.Star, false)]
		[InlineData(HackLayoutConstraint.None, GridUnitType.Auto, GridUnitType.Star, false)]
		[InlineData(HackLayoutConstraint.None, GridUnitType.Star, GridUnitType.Auto, false)]
		[InlineData(HackLayoutConstraint.None, GridUnitType.Auto, GridUnitType.Auto, false)]
		[InlineData(HackLayoutConstraint.VerticallyFixed, GridUnitType.Absolute, GridUnitType.Absolute, true)]
		[InlineData(HackLayoutConstraint.VerticallyFixed, GridUnitType.Star, GridUnitType.Absolute, false)]
		[InlineData(HackLayoutConstraint.VerticallyFixed, GridUnitType.Absolute, GridUnitType.Star, true)]
		[InlineData(HackLayoutConstraint.VerticallyFixed, GridUnitType.Auto, GridUnitType.Absolute, false)]
		[InlineData(HackLayoutConstraint.VerticallyFixed, GridUnitType.Absolute, GridUnitType.Auto, false)]
		[InlineData(HackLayoutConstraint.VerticallyFixed, GridUnitType.Star, GridUnitType.Star, false)]
		[InlineData(HackLayoutConstraint.VerticallyFixed, GridUnitType.Auto, GridUnitType.Star, false)]
		[InlineData(HackLayoutConstraint.VerticallyFixed, GridUnitType.Star, GridUnitType.Auto, false)]
		[InlineData(HackLayoutConstraint.VerticallyFixed, GridUnitType.Auto, GridUnitType.Auto, false)]
		[InlineData(HackLayoutConstraint.HorizontallyFixed, GridUnitType.Absolute, GridUnitType.Absolute, true)]
		[InlineData(HackLayoutConstraint.HorizontallyFixed, GridUnitType.Star, GridUnitType.Absolute, true)]
		[InlineData(HackLayoutConstraint.HorizontallyFixed, GridUnitType.Absolute, GridUnitType.Star, false)]
		[InlineData(HackLayoutConstraint.HorizontallyFixed, GridUnitType.Auto, GridUnitType.Absolute, false)]
		[InlineData(HackLayoutConstraint.HorizontallyFixed, GridUnitType.Absolute, GridUnitType.Auto, false)]
		[InlineData(HackLayoutConstraint.HorizontallyFixed, GridUnitType.Star, GridUnitType.Star, false)]
		[InlineData(HackLayoutConstraint.HorizontallyFixed, GridUnitType.Auto, GridUnitType.Star, false)]
		[InlineData(HackLayoutConstraint.HorizontallyFixed, GridUnitType.Star, GridUnitType.Auto, false)]
		[InlineData(HackLayoutConstraint.HorizontallyFixed, GridUnitType.Auto, GridUnitType.Auto, false)]
		[InlineData(HackLayoutConstraint.Fixed, GridUnitType.Absolute, GridUnitType.Absolute, true)]
		[InlineData(HackLayoutConstraint.Fixed, GridUnitType.Star, GridUnitType.Absolute, true)]
		[InlineData(HackLayoutConstraint.Fixed, GridUnitType.Absolute, GridUnitType.Star, true)]
		[InlineData(HackLayoutConstraint.Fixed, GridUnitType.Auto, GridUnitType.Absolute, false)]
		[InlineData(HackLayoutConstraint.Fixed, GridUnitType.Absolute, GridUnitType.Auto, false)]
		[InlineData(HackLayoutConstraint.Fixed, GridUnitType.Star, GridUnitType.Star, true)]
		[InlineData(HackLayoutConstraint.Fixed, GridUnitType.Auto, GridUnitType.Star, false)]
		[InlineData(HackLayoutConstraint.Fixed, GridUnitType.Star, GridUnitType.Auto, false)]
		[InlineData(HackLayoutConstraint.Fixed, GridUnitType.Auto, GridUnitType.Auto, false)]
		public void InvalidationPropogationTests(HackLayoutConstraint gridConstraint, GridUnitType horizontalType, GridUnitType verticalType, bool expectedResult)
		{
			var grid = new Grid
			{
				ComputedConstraint = (LayoutConstraint)gridConstraint,
				IsPlatformEnabled = true,
				RowDefinitions = {
					new RowDefinition { Height = new GridLength (1, verticalType) }
				},
				ColumnDefinitions = {
					new ColumnDefinition { Width = new GridLength (1, horizontalType) }
				}
			};

			var label = new Label { IsPlatformEnabled = true };
			grid.Children.Add(label);

			bool invalidated = false;
			grid.MeasureInvalidated += (sender, args) =>
			{
				invalidated = true;
			};

			label.Text = "Testing";

			Assert.Equal(expectedResult, !invalidated);
		}

		[Fact]
		public Task NestedInvalidateMeasureDoesNotCrash()
		{
			var delayActions = new List<Action>();

			return DispatcherTest.Run(() =>
			{
				DispatcherProviderStubOptions.InvokeOnMainThread = a => { delayActions.Add(a); };

				var grid = new Grid
				{
					IsPlatformEnabled = true
				};

				var child = new Label
				{
					IsPlatformEnabled = true
				};
				grid.Children.Add(child);

				var child2 = new Label
				{
					IsPlatformEnabled = true
				};
				grid.Children.Add(child2);

				bool fire = true;
				child.SizeChanged += (sender, args) =>
				{
					if (fire)
						((IVisualElementController)child).InvalidateMeasure(InvalidationTrigger.Undefined);
					fire = false;
				};

				grid.Layout(new Rect(0, 0, 100, 100));

				foreach (var delayAction in delayActions)
				{
					delayAction();
				}
			});
		}
	}
}