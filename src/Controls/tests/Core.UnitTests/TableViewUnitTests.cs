#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;


using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;
using NSubstitute;

using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

    public class TableViewUnitTests : BaseTestFixture
    {
        [Fact]
        public void TestConstructor()
        {
            var table = new TableView();

            Assert.False(table.Root.Any());
            Assert.Equal(LayoutOptions.FillAndExpand, table.HorizontalOptions);
            Assert.Equal(LayoutOptions.FillAndExpand, table.VerticalOptions);
        }

        [Fact]
        public void TestModelChanged()
        {
            var table = new TableView();

            bool changed = false;

            table.ModelChanged += (sender, e) => changed = true;

            table.Root = new TableRoot("NewRoot");

            Assert.True(changed);
        }

        [Fact]
        public void BindingsContextChainsToModel()
        {
            const string context = "Context";
            var table = new TableView { BindingContext = context, Root = new TableRoot() };

            Assert.Equal(context, table.Root.BindingContext);

            // reverse assignment order
            table = new TableView { Root = new TableRoot(), BindingContext = context };
            Assert.Equal(context, table.Root.BindingContext);
        }

        [Fact]
        public void ParentsViewCells()
        {
            ViewCell viewCell = new ViewCell { View = new Label() };
            var table = new TableView
            {
                Root = new TableRoot {
                    new TableSection {
                        viewCell
                    }
                }
            };

            Assert.Equal(table, viewCell.Parent);
            Assert.Equal(viewCell, viewCell.View.Parent);
        }

        [Fact]
        public void ParentsAddedViewCells()
        {
            var viewCell = new ViewCell { View = new Label() };
            var section = new TableSection();
            var table = new TableView
            {
                Root = new TableRoot {
                    section
                }
            };

            section.Add(viewCell);

            Assert.Equal(table, viewCell.Parent);
            Assert.Equal(viewCell, viewCell.View.Parent);
        }
    }

    public partial class TableViewTableSectionModelTests : BaseTestFixture
    {
        /// <summary>
        /// Tests that GetPath throws ArgumentNullException when item parameter is null.
        /// Verifies that the method properly validates its input parameter.
        /// Expected result: ArgumentNullException with correct parameter name.
        /// </summary>
        [Fact]
        public void GetPath_NullItem_ThrowsArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                TableView.TableSectionModel.GetPath(null));

            Assert.Equal("item", exception.ParamName);
        }

        /// <summary>
        /// Tests that GetPath returns the correct tuple when cell has PathProperty set to a valid tuple.
        /// Verifies that the method retrieves and casts the PathProperty value correctly.
        /// Expected result: Returns the tuple value stored in the cell's PathProperty.
        /// </summary>
        [Fact]
        public void GetPath_ValidCellWithPathProperty_ReturnsTuple()
        {
            // Arrange
            var expectedTuple = new Tuple<int, int>(2, 5);
            var cell = new TestCell();

            // Use reflection to access the private PathProperty field
            var pathProperty = typeof(TableView.TableSectionModel)
                .GetField("PathProperty", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)
                ?.GetValue(null) as BindableProperty;

            Assert.NotNull(pathProperty);
            cell.SetValue(pathProperty, expectedTuple);

            // Act
            var result = TableView.TableSectionModel.GetPath(cell);

            // Assert
            Assert.Equal(expectedTuple, result);
            Assert.Equal(2, result.Item1);
            Assert.Equal(5, result.Item2);
        }

        /// <summary>
        /// Tests that GetPath returns null when cell has PathProperty set to null.
        /// Verifies that the method handles null property values correctly.
        /// Expected result: Returns null when PathProperty is null.
        /// </summary>
        [Fact]
        public void GetPath_CellWithNullPathProperty_ReturnsNull()
        {
            // Arrange
            var cell = new TestCell();

            // Use reflection to access the private PathProperty field
            var pathProperty = typeof(TableView.TableSectionModel)
                .GetField("PathProperty", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)
                ?.GetValue(null) as BindableProperty;

            Assert.NotNull(pathProperty);
            cell.SetValue(pathProperty, null);

            // Act
            var result = TableView.TableSectionModel.GetPath(cell);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that GetPath handles boundary values in tuple correctly.
        /// Verifies that the method works with extreme integer values.
        /// Expected result: Returns tuple with boundary integer values.
        /// </summary>
        [Theory]
        [InlineData(int.MinValue, int.MaxValue)]
        [InlineData(0, 0)]
        [InlineData(-1, 1)]
        [InlineData(int.MaxValue, int.MinValue)]
        public void GetPath_CellWithBoundaryTupleValues_ReturnsTuple(int item1, int item2)
        {
            // Arrange
            var expectedTuple = new Tuple<int, int>(item1, item2);
            var cell = new TestCell();

            // Use reflection to access the private PathProperty field
            var pathProperty = typeof(TableView.TableSectionModel)
                .GetField("PathProperty", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)
                ?.GetValue(null) as BindableProperty;

            Assert.NotNull(pathProperty);
            cell.SetValue(pathProperty, expectedTuple);

            // Act
            var result = TableView.TableSectionModel.GetPath(cell);

            // Assert
            Assert.Equal(expectedTuple, result);
            Assert.Equal(item1, result.Item1);
            Assert.Equal(item2, result.Item2);
        }

        /// <summary>
        /// Helper test cell class for testing purposes.
        /// Provides a concrete implementation of the abstract Cell class.
        /// </summary>
        private class TestCell : Cell
        {
        }
    }


    public partial class TableViewTests
    {
        /// <summary>
        /// Tests OnMeasure method with normal screen dimensions where width is smaller than height.
        /// Should ignore input constraints and return portrait-oriented size request.
        /// </summary>
        [Fact]
        public void OnMeasure_PortraitScreenSize_ReturnsPortraitSizeRequest()
        {
            // Arrange
            var mockDeviceDisplay = Substitute.For<IDeviceDisplay>();
            var mockDisplayInfo = Substitute.For<DisplayInfo>();
            mockDisplayInfo.Width.Returns(1080);
            mockDisplayInfo.Height.Returns(1920);
            mockDisplayInfo.Density.Returns(2.0);
            mockDeviceDisplay.MainDisplayInfo.Returns(mockDisplayInfo);
            DeviceDisplay.SetCurrent(mockDeviceDisplay);

            var tableView = new TableView();
            double widthConstraint = 300;
            double heightConstraint = 400;

            // Act
            var result = tableView.OnMeasure(widthConstraint, heightConstraint);

            // Assert
            Assert.Equal(new Size(40, 40), result.Minimum);
            Assert.Equal(540, result.Request.Width); // Min(540, 960) = 540
            Assert.Equal(960, result.Request.Height); // Max(540, 960) = 960
        }

        /// <summary>
        /// Tests OnMeasure method with landscape screen dimensions where width is larger than height.
        /// Should return portrait-oriented size request regardless of actual screen orientation.
        /// </summary>
        [Fact]
        public void OnMeasure_LandscapeScreenSize_ReturnsPortraitSizeRequest()
        {
            // Arrange
            var mockDeviceDisplay = Substitute.For<IDeviceDisplay>();
            var mockDisplayInfo = Substitute.For<DisplayInfo>();
            mockDisplayInfo.Width.Returns(1920);
            mockDisplayInfo.Height.Returns(1080);
            mockDisplayInfo.Density.Returns(1.5);
            mockDeviceDisplay.MainDisplayInfo.Returns(mockDisplayInfo);
            DeviceDisplay.SetCurrent(mockDeviceDisplay);

            var tableView = new TableView();

            // Act
            var result = tableView.OnMeasure(100, 200);

            // Assert
            Assert.Equal(new Size(40, 40), result.Minimum);
            Assert.Equal(720, result.Request.Width); // Min(1280, 720) = 720
            Assert.Equal(1280, result.Request.Height); // Max(1280, 720) = 1280
        }

        /// <summary>
        /// Tests OnMeasure method with equal width and height screen dimensions.
        /// Should handle square screen size correctly.
        /// </summary>
        [Fact]
        public void OnMeasure_SquareScreenSize_ReturnsSquareSizeRequest()
        {
            // Arrange
            var mockDeviceDisplay = Substitute.For<IDeviceDisplay>();
            var mockDisplayInfo = Substitute.For<DisplayInfo>();
            mockDisplayInfo.Width.Returns(800);
            mockDisplayInfo.Height.Returns(800);
            mockDisplayInfo.Density.Returns(1.0);
            mockDeviceDisplay.MainDisplayInfo.Returns(mockDisplayInfo);
            DeviceDisplay.SetCurrent(mockDeviceDisplay);

            var tableView = new TableView();

            // Act
            var result = tableView.OnMeasure(50, 75);

            // Assert
            Assert.Equal(new Size(40, 40), result.Minimum);
            Assert.Equal(800, result.Request.Width); // Min(800, 800) = 800
            Assert.Equal(800, result.Request.Height); // Max(800, 800) = 800
        }

        /// <summary>
        /// Tests OnMeasure method with zero density which should result in Size.Zero from GetScaledScreenSize.
        /// Should handle zero screen dimensions gracefully.
        /// </summary>
        [Fact]
        public void OnMeasure_ZeroDensity_HandlesZeroScreenSizeGracefully()
        {
            // Arrange
            var mockDeviceDisplay = Substitute.For<IDeviceDisplay>();
            var mockDisplayInfo = Substitute.For<DisplayInfo>();
            mockDisplayInfo.Width.Returns(1080);
            mockDisplayInfo.Height.Returns(1920);
            mockDisplayInfo.Density.Returns(0.0); // This causes GetScaledScreenSize to return Size.Zero
            mockDeviceDisplay.MainDisplayInfo.Returns(mockDisplayInfo);
            DeviceDisplay.SetCurrent(mockDeviceDisplay);

            var tableView = new TableView();

            // Act
            var result = tableView.OnMeasure(100, 200);

            // Assert
            Assert.Equal(new Size(40, 40), result.Minimum);
            Assert.Equal(0, result.Request.Width); // Min(0, 0) = 0
            Assert.Equal(0, result.Request.Height); // Max(0, 0) = 0
        }

        /// <summary>
        /// Tests OnMeasure method ignores input constraints and uses only device screen dimensions.
        /// Verifies that various constraint values (including special double values) are ignored.
        /// </summary>
        [Theory]
        [InlineData(double.NaN, double.NaN)]
        [InlineData(double.PositiveInfinity, double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity, double.NegativeInfinity)]
        [InlineData(-1000, -2000)]
        [InlineData(0, 0)]
        [InlineData(1000000, 2000000)]
        public void OnMeasure_VariousConstraints_IgnoresConstraintsAndUsesScreenSize(double widthConstraint, double heightConstraint)
        {
            // Arrange
            var mockDeviceDisplay = Substitute.For<IDeviceDisplay>();
            var mockDisplayInfo = Substitute.For<DisplayInfo>();
            mockDisplayInfo.Width.Returns(600);
            mockDisplayInfo.Height.Returns(1000);
            mockDisplayInfo.Density.Returns(2.0);
            mockDeviceDisplay.MainDisplayInfo.Returns(mockDisplayInfo);
            DeviceDisplay.SetCurrent(mockDeviceDisplay);

            var tableView = new TableView();

            // Act
            var result = tableView.OnMeasure(widthConstraint, heightConstraint);

            // Assert - Result should be the same regardless of input constraints
            Assert.Equal(new Size(40, 40), result.Minimum);
            Assert.Equal(300, result.Request.Width); // Min(300, 500) = 300
            Assert.Equal(500, result.Request.Height); // Max(300, 500) = 500
        }

        /// <summary>
        /// Tests OnMeasure method with very small screen dimensions.
        /// Should handle edge case of tiny screen size correctly.
        /// </summary>
        [Fact]
        public void OnMeasure_VerySmallScreenSize_ReturnsSmallSizeRequest()
        {
            // Arrange
            var mockDeviceDisplay = Substitute.For<IDeviceDisplay>();
            var mockDisplayInfo = Substitute.For<DisplayInfo>();
            mockDisplayInfo.Width.Returns(10);
            mockDisplayInfo.Height.Returns(20);
            mockDisplayInfo.Density.Returns(1.0);
            mockDeviceDisplay.MainDisplayInfo.Returns(mockDisplayInfo);
            DeviceDisplay.SetCurrent(mockDeviceDisplay);

            var tableView = new TableView();

            // Act
            var result = tableView.OnMeasure(500, 600);

            // Assert
            Assert.Equal(new Size(40, 40), result.Minimum);
            Assert.Equal(10, result.Request.Width); // Min(10, 20) = 10
            Assert.Equal(20, result.Request.Height); // Max(10, 20) = 20
        }

        /// <summary>
        /// Tests OnMeasure method with very large screen dimensions.
        /// Should handle edge case of large screen size correctly.
        /// </summary>
        [Fact]
        public void OnMeasure_VeryLargeScreenSize_ReturnsLargeSizeRequest()
        {
            // Arrange
            var mockDeviceDisplay = Substitute.For<IDeviceDisplay>();
            var mockDisplayInfo = Substitute.For<DisplayInfo>();
            mockDisplayInfo.Width.Returns(5000);
            mockDisplayInfo.Height.Returns(3000);
            mockDisplayInfo.Density.Returns(0.5);
            mockDeviceDisplay.MainDisplayInfo.Returns(mockDisplayInfo);
            DeviceDisplay.SetCurrent(mockDeviceDisplay);

            var tableView = new TableView();

            // Act
            var result = tableView.OnMeasure(10, 20);

            // Assert
            Assert.Equal(new Size(40, 40), result.Minimum);
            Assert.Equal(6000, result.Request.Width); // Min(10000, 6000) = 6000
            Assert.Equal(10000, result.Request.Height); // Max(10000, 6000) = 10000
        }

        /// <summary>
        /// Tests that the On method returns a non-null IPlatformElementConfiguration when called with a valid platform type.
        /// </summary>
        [Fact]
        public void On_WithValidPlatformType_ReturnsNonNullConfiguration()
        {
            // Arrange
            var tableView = new TableView();

            // Act
            var result = tableView.On<TestPlatform>();

            // Assert
            Assert.NotNull(result);
        }

        /// <summary>
        /// Tests that the On method returns the same configuration instance when called multiple times with the same platform type.
        /// This verifies the caching behavior of the underlying PlatformConfigurationRegistry.
        /// </summary>
        [Fact]
        public void On_CalledMultipleTimesWithSamePlatformType_ReturnsSameInstance()
        {
            // Arrange
            var tableView = new TableView();

            // Act
            var result1 = tableView.On<TestPlatform>();
            var result2 = tableView.On<TestPlatform>();

            // Assert
            Assert.Same(result1, result2);
        }

        /// <summary>
        /// Tests that the On method returns different configuration instances when called with different platform types.
        /// </summary>
        [Fact]
        public void On_CalledWithDifferentPlatformTypes_ReturnsDifferentInstances()
        {
            // Arrange
            var tableView = new TableView();

            // Act
            var result1 = tableView.On<TestPlatform>();
            var result2 = tableView.On<AnotherTestPlatform>();

            // Assert
            Assert.NotSame(result1, result2);
        }

        /// <summary>
        /// Tests that the On method works correctly with multiple different platform types,
        /// ensuring each type gets its own cached configuration instance.
        /// </summary>
        [Fact]
        public void On_WithMultiplePlatformTypes_CachesEachTypeIndependently()
        {
            // Arrange
            var tableView = new TableView();

            // Act
            var testPlatform1 = tableView.On<TestPlatform>();
            var anotherPlatform1 = tableView.On<AnotherTestPlatform>();
            var testPlatform2 = tableView.On<TestPlatform>();
            var anotherPlatform2 = tableView.On<AnotherTestPlatform>();

            // Assert
            Assert.Same(testPlatform1, testPlatform2);
            Assert.Same(anotherPlatform1, anotherPlatform2);
            Assert.NotSame(testPlatform1, anotherPlatform1);
        }

        /// <summary>
        /// Tests that the On method properly initializes the lazy PlatformConfigurationRegistry on first access.
        /// </summary>
        [Fact]
        public void On_FirstCall_InitializesLazyPlatformConfigurationRegistry()
        {
            // Arrange
            var tableView = new TableView();

            // Act
            var result = tableView.On<TestPlatform>();

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IPlatformElementConfiguration<TestPlatform, TableView>>(result);
        }

        /// <summary>
        /// Test platform type that implements IConfigPlatform for testing purposes.
        /// </summary>
        private class TestPlatform : IConfigPlatform
        {
        }

        /// <summary>
        /// Another test platform type that implements IConfigPlatform for testing purposes.
        /// </summary>
        private class AnotherTestPlatform : IConfigPlatform
        {
        }

        /// <summary>
        /// Tests the TableView constructor with a null TableRoot parameter.
        /// Verifies that the constructor handles null input gracefully and sets up default properties correctly.
        /// Expected result: TableView is created with default empty TableRoot and proper layout options.
        /// </summary>
        [Fact]
        public void Constructor_WithNullRoot_SetsDefaultPropertiesAndCreatesEmptyRoot()
        {
            // Arrange & Act
            var tableView = new TableView(null);

            // Assert
            Assert.Equal(LayoutOptions.FillAndExpand, tableView.VerticalOptions);
            Assert.Equal(LayoutOptions.FillAndExpand, tableView.HorizontalOptions);
            Assert.NotNull(tableView.Model);
            Assert.NotNull(tableView.Root);
            Assert.Empty(tableView.Root);
        }

        /// <summary>
        /// Tests the TableView constructor with a valid TableRoot parameter.
        /// Verifies that the constructor properly uses the provided TableRoot and sets up layout options.
        /// Expected result: TableView is created with the provided TableRoot and proper layout options.
        /// </summary>
        [Fact]
        public void Constructor_WithValidRoot_SetsPropertiesAndUsesProvidedRoot()
        {
            // Arrange
            var tableRoot = new TableRoot();
            var section = new TableSection("Test Section");
            tableRoot.Add(section);

            // Act
            var tableView = new TableView(tableRoot);

            // Assert
            Assert.Equal(LayoutOptions.FillAndExpand, tableView.VerticalOptions);
            Assert.Equal(LayoutOptions.FillAndExpand, tableView.HorizontalOptions);
            Assert.NotNull(tableView.Model);
            Assert.Same(tableRoot, tableView.Root);
            Assert.Single(tableView.Root);
            Assert.Equal("Test Section", tableView.Root[0].Title);
        }

        /// <summary>
        /// Tests the TableView constructor with an empty TableRoot parameter.
        /// Verifies that the constructor properly handles an empty but non-null TableRoot.
        /// Expected result: TableView is created with the provided empty TableRoot and proper layout options.
        /// </summary>
        [Fact]
        public void Constructor_WithEmptyRoot_SetsPropertiesAndUsesEmptyRoot()
        {
            // Arrange
            var emptyTableRoot = new TableRoot();

            // Act
            var tableView = new TableView(emptyTableRoot);

            // Assert
            Assert.Equal(LayoutOptions.FillAndExpand, tableView.VerticalOptions);
            Assert.Equal(LayoutOptions.FillAndExpand, tableView.HorizontalOptions);
            Assert.NotNull(tableView.Model);
            Assert.Same(emptyTableRoot, tableView.Root);
            Assert.Empty(tableView.Root);
        }

        /// <summary>
        /// Tests the TableView constructor with a TableRoot containing multiple sections.
        /// Verifies that the constructor properly handles a complex TableRoot structure.
        /// Expected result: TableView is created with all sections preserved and proper layout options.
        /// </summary>
        [Fact]
        public void Constructor_WithMultipleSections_PreservesAllSections()
        {
            // Arrange
            var tableRoot = new TableRoot();
            var section1 = new TableSection("Section 1");
            var section2 = new TableSection("Section 2");
            var section3 = new TableSection("Section 3");
            tableRoot.Add(section1);
            tableRoot.Add(section2);
            tableRoot.Add(section3);

            // Act
            var tableView = new TableView(tableRoot);

            // Assert
            Assert.Equal(LayoutOptions.FillAndExpand, tableView.VerticalOptions);
            Assert.Equal(LayoutOptions.FillAndExpand, tableView.HorizontalOptions);
            Assert.NotNull(tableView.Model);
            Assert.Same(tableRoot, tableView.Root);
            Assert.Equal(3, tableView.Root.Count);
            Assert.Equal("Section 1", tableView.Root[0].Title);
            Assert.Equal("Section 2", tableView.Root[1].Title);
            Assert.Equal("Section 3", tableView.Root[2].Title);
        }

        /// <summary>
        /// Tests that the TableView constructor properly initializes the Model property.
        /// Verifies that the Model is not null and is properly configured.
        /// Expected result: Model property is initialized and accessible.
        /// </summary>
        [Fact]
        public void Constructor_InitializesModelProperty()
        {
            // Arrange
            var tableRoot = new TableRoot();

            // Act
            var tableView = new TableView(tableRoot);

            // Assert
            Assert.NotNull(tableView.Model);
            Assert.IsType<TableView.TableSectionModel>(tableView.Model);
        }

        /// <summary>
        /// Tests that the TableView constructor properly initializes the platform configuration registry.
        /// Verifies that the On method works correctly after construction, indicating proper initialization.
        /// Expected result: Platform configuration registry is initialized and accessible.
        /// </summary>
        [Fact]
        public void Constructor_InitializesPlatformConfigurationRegistry()
        {
            // Arrange
            var tableRoot = new TableRoot();

            // Act
            var tableView = new TableView(tableRoot);

            // Assert
            // Verify that the platform configuration registry is properly initialized
            // by checking that the On<T>() method doesn't throw
            var config = tableView.On<Microsoft.Maui.Controls.PlatformConfiguration.iOS>();
            Assert.NotNull(config);
        }
    }
}