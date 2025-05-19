using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class GridMinMaxIntegrationTests : BaseTestFixture
    {
        [Fact]
        public void MinWidthConstraintApplied()
        {
            var grid = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Auto, MinWidth = 100 }
                }
            };

            var box = new BoxView { WidthRequest = 50, HeightRequest = 50 };
            grid.Add(box);

            grid.Measure(double.PositiveInfinity, double.PositiveInfinity);
            Assert.Equal(100, grid.DesiredSize.Width);
        }

        [Fact]
        public void MaxWidthConstraintApplied()
        {
            var grid = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Auto, MaxWidth = 100 }
                }
            };

            var box = new BoxView { WidthRequest = 150, HeightRequest = 50 };
            grid.Add(box);

            grid.Measure(double.PositiveInfinity, double.PositiveInfinity);
            Assert.Equal(100, grid.DesiredSize.Width);
        }

        [Fact]
        public void MinHeightConstraintApplied()
        {
            var grid = new Grid
            {
                RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Auto, MinHeight = 100 }
                }
            };

            var box = new BoxView { WidthRequest = 50, HeightRequest = 50 };
            grid.Add(box);

            grid.Measure(double.PositiveInfinity, double.PositiveInfinity);
            Assert.Equal(100, grid.DesiredSize.Height);
        }

        [Fact]
        public void MaxHeightConstraintApplied()
        {
            var grid = new Grid
            {
                RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Auto, MaxHeight = 100 }
                }
            };

            var box = new BoxView { WidthRequest = 50, HeightRequest = 150 };
            grid.Add(box);

            grid.Measure(double.PositiveInfinity, double.PositiveInfinity);
            Assert.Equal(100, grid.DesiredSize.Height);
        }

        [Fact]
        public void MultipleColumnsWithMinWidthConstraints()
        {
            var grid = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Auto, MinWidth = 75 },
                    new ColumnDefinition { Width = GridLength.Auto, MinWidth = 100 }
                }
            };

            var box1 = new BoxView { WidthRequest = 50, HeightRequest = 50 };
            var box2 = new BoxView { WidthRequest = 50, HeightRequest = 50 };
            
            grid.Add(box1, 0, 0);
            grid.Add(box2, 1, 0);

            grid.Measure(double.PositiveInfinity, double.PositiveInfinity);
            Assert.Equal(75 + 100, grid.DesiredSize.Width);
        }

        [Fact]
        public void MultipleRowsWithMinHeightConstraints()
        {
            var grid = new Grid
            {
                RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Auto, MinHeight = 75 },
                    new RowDefinition { Height = GridLength.Auto, MinHeight = 100 }
                }
            };

            var box1 = new BoxView { WidthRequest = 50, HeightRequest = 50 };
            var box2 = new BoxView { WidthRequest = 50, HeightRequest = 50 };
            
            grid.Add(box1, 0, 0);
            grid.Add(box2, 0, 1);

            grid.Measure(double.PositiveInfinity, double.PositiveInfinity);
            Assert.Equal(75 + 100, grid.DesiredSize.Height);
        }

        [Fact]
        public void MultipleColumnsWithMaxWidthConstraints()
        {
            var grid = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Auto, MaxWidth = 75 },
                    new ColumnDefinition { Width = GridLength.Auto, MaxWidth = 100 }
                }
            };

            var box1 = new BoxView { WidthRequest = 100, HeightRequest = 50 };
            var box2 = new BoxView { WidthRequest = 150, HeightRequest = 50 };
            
            grid.Add(box1, 0, 0);
            grid.Add(box2, 1, 0);

            grid.Measure(double.PositiveInfinity, double.PositiveInfinity);
            Assert.Equal(75 + 100, grid.DesiredSize.Width);
        }

        [Fact]
        public void MultipleRowsWithMaxHeightConstraints()
        {
            var grid = new Grid
            {
                RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Auto, MaxHeight = 75 },
                    new RowDefinition { Height = GridLength.Auto, MaxHeight = 100 }
                }
            };

            var box1 = new BoxView { WidthRequest = 50, HeightRequest = 100 };
            var box2 = new BoxView { WidthRequest = 50, HeightRequest = 150 };
            
            grid.Add(box1, 0, 0);
            grid.Add(box2, 0, 1);

            grid.Measure(double.PositiveInfinity, double.PositiveInfinity);
            Assert.Equal(75 + 100, grid.DesiredSize.Height);
        }

        [Fact]
        public void StarsRespectMinMaxWidthConstraints()
        {
            var grid = new Grid
            {
                WidthRequest = 300,
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star), MinWidth = 100 },
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star), MaxWidth = 100 }
                }
            };

            var box1 = new BoxView { HeightRequest = 50 };
            var box2 = new BoxView { HeightRequest = 50 };
            
            grid.Add(box1, 0, 0);
            grid.Add(box2, 1, 0);

            grid.Measure(double.PositiveInfinity, double.PositiveInfinity);
            grid.Arrange(new Rect(0, 0, 300, 50));

            // Expected: Column 0 should get 200 (due to Column 1 being limited to 100)
            Assert.Equal(300, grid.Bounds.Width);
            Assert.Equal(0, box1.Bounds.X);
            Assert.Equal(200, box1.Bounds.Width);
            Assert.Equal(200, box2.Bounds.X);
            Assert.Equal(100, box2.Bounds.Width);
        }

        [Fact]
        public void StarsRespectMinMaxHeightConstraints()
        {
            var grid = new Grid
            {
                HeightRequest = 300,
                RowDefinitions =
                {
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Star), MinHeight = 100 },
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Star), MaxHeight = 100 }
                }
            };

            var box1 = new BoxView { WidthRequest = 50 };
            var box2 = new BoxView { WidthRequest = 50 };
            
            grid.Add(box1, 0, 0);
            grid.Add(box2, 0, 1);

            grid.Measure(double.PositiveInfinity, double.PositiveInfinity);
            grid.Arrange(new Rect(0, 0, 50, 300));

            // Expected: Row 0 should get 200 (due to Row 1 being limited to 100)
            Assert.Equal(300, grid.Bounds.Height);
            Assert.Equal(0, box1.Bounds.Y);
            Assert.Equal(200, box1.Bounds.Height);
            Assert.Equal(200, box2.Bounds.Y);
            Assert.Equal(100, box2.Bounds.Height);
        }

        [Fact]
        public void SpanningViewsRespectColumnMinMaxWidths()
        {
            var grid = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Auto, MinWidth = 75 },
                    new ColumnDefinition { Width = GridLength.Auto, MaxWidth = 100 }
                }
            };

            var spanningBox = new BoxView { WidthRequest = 200, HeightRequest = 50 };
            var box1 = new BoxView { WidthRequest = 50, HeightRequest = 50 };
            var box2 = new BoxView { WidthRequest = 150, HeightRequest = 50 };
            
            grid.Add(spanningBox, 0, 0);
            Grid.SetColumnSpan(spanningBox, 2);
            grid.Add(box1, 0, 1);
            grid.Add(box2, 1, 1);

            grid.Measure(double.PositiveInfinity, double.PositiveInfinity);
            Assert.Equal(75 + 100, grid.DesiredSize.Width);
        }

        [Fact]
        public void SpanningViewsRespectRowMinMaxHeights()
        {
            var grid = new Grid
            {
                RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Auto, MinHeight = 75 },
                    new RowDefinition { Height = GridLength.Auto, MaxHeight = 100 }
                }
            };

            var spanningBox = new BoxView { WidthRequest = 50, HeightRequest = 200 };
            var box1 = new BoxView { WidthRequest = 50, HeightRequest = 50 };
            var box2 = new BoxView { WidthRequest = 50, HeightRequest = 150 };
            
            grid.Add(spanningBox, 0, 0);
            Grid.SetRowSpan(spanningBox, 2);
            grid.Add(box1, 1, 0);
            grid.Add(box2, 1, 1);

            grid.Measure(double.PositiveInfinity, double.PositiveInfinity);
            Assert.Equal(75 + 100, grid.DesiredSize.Height);
        }
    }
}