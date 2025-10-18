#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;


using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

    public class CellTests : BaseTestFixture
    {
        internal class TestCell : Cell
        {
            public bool OnAppearingSent { get; set; }
            public bool OnDisappearingSent { get; set; }

            protected override void OnAppearing()
            {
                base.OnAppearing();
                OnAppearingSent = true;
            }

            protected override void OnDisappearing()
            {
                base.OnDisappearing();
                OnDisappearingSent = true;
            }
        }

        [Fact]
        public void Selected()
        {
            var cell = new TestCell();

            bool tapped = false;
            cell.Tapped += (sender, args) => tapped = true;

            cell.OnTapped();
            Assert.True(tapped);
        }

        [Fact]
        public void AppearingEvent()
        {
            var cell = new TestCell();

            bool emitted = false;
            cell.Appearing += (sender, args) => emitted = true;

            cell.SendAppearing();
            Assert.True(emitted);
            Assert.True(cell.OnAppearingSent);
            Assert.False(cell.OnDisappearingSent);
        }

        [Fact]
        public void DisappearingEvent()
        {
            var cell = new TestCell();

            bool emitted = false;
            cell.Disappearing += (sender, args) => emitted = true;

            cell.SendDisappearing();
            Assert.True(emitted);
            Assert.False(cell.OnAppearingSent);
            Assert.True(cell.OnDisappearingSent);
        }

        [Fact]
        public void TestBindingContextPropagationOnImageCell()
        {
            var context = new object();
            var cell = new ImageCell();
            cell.BindingContext = context;
            var source = new FileImageSource();
            cell.ImageSource = source;
            Assert.Same(context, source.BindingContext);

            cell = new ImageCell();
            source = new FileImageSource();
            cell.ImageSource = source;
            cell.BindingContext = context;
            Assert.Same(context, source.BindingContext);
        }

        [Fact]
        public void HasContextActions()
        {
            bool changed = false;

            var cell = new TextCell();
            cell.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "HasContextActions")
                    changed = true;
            };

            Assert.False(cell.HasContextActions);
            Assert.False(changed);

            var collection = cell.ContextActions;

            Assert.False(cell.HasContextActions);
            Assert.False(changed);

            collection.Add(new MenuItem());

            Assert.True(cell.HasContextActions);
            Assert.True(changed);
        }

        [Fact]
        public void MenuItemsGetBindingContext()
        {
            var cell = new TextCell
            {
                ContextActions = {
                    new MenuItem ()
                }
            };

            object bc = new object();

            cell.BindingContext = bc;
            Assert.Same(cell.ContextActions[0].BindingContext, bc);

            cell = new TextCell { BindingContext = new object() };
            cell.ContextActions.Add(new MenuItem());

            Assert.Same(cell.ContextActions[0].BindingContext, cell.BindingContext);
        }

        [Fact]
        public void RenderHeightINPCFromParent()
        {
            var lv = new ListView();
            var cell = new TextCell();
            cell.Parent = lv;

            int changing = 0, changed = 0;
            cell.PropertyChanging += (sender, args) =>
            {
                if (args.PropertyName == "RenderHeight")
                    changing++;
            };

            cell.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "RenderHeight")
                    changed++;
            };

            lv.RowHeight = 5;

            Assert.Equal(5, cell.RenderHeight);

            Assert.Equal(1, changing);
            Assert.Equal(1, changed);
        }

        [Fact]
        public async Task ForceUpdateSizeCallsAreRateLimited()
        {
            var lv = new ListView { HasUnevenRows = true };
            var cell = new ViewCell { Parent = lv };

            int numberOfCalls = 0;
            cell.ForceUpdateSizeRequested += (object sender, System.EventArgs e) => { numberOfCalls++; };

            cell.ForceUpdateSize();
            cell.ForceUpdateSize();
            cell.ForceUpdateSize();
            cell.ForceUpdateSize();

            await System.Threading.Tasks.Task.Delay(TimeSpan.FromMilliseconds(150));

            Assert.Equal(1, numberOfCalls);
        }

        [Fact]
        public async Task ForceUpdateSizeWillNotBeCalledIfParentIsNotAListViewWithUnevenRows()
        {
            var lv = new ListView { HasUnevenRows = false };
            var cell = new ViewCell { Parent = lv };

            int numberOfCalls = 0;
            cell.ForceUpdateSizeRequested += (object sender, System.EventArgs e) => { numberOfCalls++; };

            cell.ForceUpdateSize();

            await System.Threading.Tasks.Task.Delay(TimeSpan.FromMilliseconds(16));

            Assert.Equal(0, numberOfCalls);
        }

        /// <summary>
        /// Tests that the Cell constructor initializes successfully without throwing exceptions.
        /// Verifies that the constructor completes and creates a valid Cell instance.
        /// Expected result: Constructor completes successfully and creates a valid Cell instance.
        /// </summary>
        [Fact]
        public void Constructor_InitializesSuccessfully_CreatesValidCellInstance()
        {
            // Arrange & Act
            TestCell cell = null;
            var exception = Record.Exception(() => cell = new TestCell());

            // Assert
            Assert.Null(exception);
            Assert.NotNull(cell);
        }

        /// <summary>
        /// Tests that the Cell constructor properly initializes the lazy ElementConfiguration.
        /// Verifies that accessing the ElementConfiguration through the On method works correctly.
        /// Expected result: ElementConfiguration is created and accessible without throwing exceptions.
        /// </summary>
        [Fact]
        public void Constructor_InitializesLazyElementConfiguration_ElementConfigurationAccessibleThroughOnMethod()
        {
            // Arrange
            var cell = new TestCell();

            // Act & Assert
            var exception = Record.Exception(() =>
            {
                // This will trigger the lazy initialization of _elementConfiguration
                var config = cell.On<Microsoft.Maui.Controls.PlatformConfiguration.iOS>();
                Assert.NotNull(config);
            });

            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that multiple accesses to ElementConfiguration return the same instance.
        /// Verifies the lazy initialization singleton behavior of ElementConfiguration.
        /// Expected result: Multiple calls to On method return configurations from the same ElementConfiguration instance.
        /// </summary>
        [Fact]
        public void Constructor_ElementConfigurationLazyBehavior_MultipleAccessesReturnSameInstance()
        {
            // Arrange
            var cell = new TestCell();

            // Act
            var config1 = cell.On<Microsoft.Maui.Controls.PlatformConfiguration.iOS>();
            var config2 = cell.On<Microsoft.Maui.Controls.PlatformConfiguration.iOS>();

            // Assert
            Assert.NotNull(config1);
            Assert.NotNull(config2);
            Assert.Same(config1, config2);
        }

        /// <summary>
        /// Tests that the ElementConfiguration is properly associated with the correct Cell instance.
        /// Verifies that different Cell instances have different ElementConfiguration instances.
        /// Expected result: Each Cell instance has its own unique ElementConfiguration instance.
        /// </summary>
        [Fact]
        public void Constructor_ElementConfigurationAssociation_DifferentCellsHaveDifferentConfigurations()
        {
            // Arrange
            var cell1 = new TestCell();
            var cell2 = new TestCell();

            // Act
            var config1 = cell1.On<Microsoft.Maui.Controls.PlatformConfiguration.iOS>();
            var config2 = cell2.On<Microsoft.Maui.Controls.PlatformConfiguration.iOS>();

            // Assert
            Assert.NotNull(config1);
            Assert.NotNull(config2);
            Assert.NotSame(config1, config2);
        }

        /// <summary>
        /// Tests that the Cell constructor works correctly when called multiple times.
        /// Verifies that creating multiple Cell instances doesn't interfere with each other.
        /// Expected result: Multiple Cell instances can be created successfully with independent configurations.
        /// </summary>
        [Fact]
        public void Constructor_MultipleInstances_EachInstanceInitializesIndependently()
        {
            // Arrange & Act
            var cells = new TestCell[3];
            var exception = Record.Exception(() =>
            {
                for (int i = 0; i < 3; i++)
                {
                    cells[i] = new TestCell();
                }
            });

            // Assert
            Assert.Null(exception);
            for (int i = 0; i < 3; i++)
            {
                Assert.NotNull(cells[i]);
                var config = cells[i].On<Microsoft.Maui.Controls.PlatformConfiguration.iOS>();
                Assert.NotNull(config);
            }
        }

        /// <summary>
        /// Tests that On method returns a non-null IPlatformElementConfiguration when called with a valid platform type.
        /// Verifies the method correctly delegates to the internal ElementConfiguration.
        /// </summary>
        [Fact]
        public void On_WithValidPlatformType_ReturnsNonNullConfiguration()
        {
            // Arrange
            var cell = new TestCell();

            // Act
            var result = cell.On<TestPlatform>();

            // Assert
            Assert.NotNull(result);
        }

        /// <summary>
        /// Tests that On method returns different configuration instances for different platform types.
        /// Verifies the method properly handles multiple platform configurations.
        /// </summary>
        [Fact]
        public void On_WithDifferentPlatformTypes_ReturnsDistinctConfigurations()
        {
            // Arrange
            var cell = new TestCell();

            // Act
            var config1 = cell.On<TestPlatform>();
            var config2 = cell.On<AnotherTestPlatform>();

            // Assert
            Assert.NotNull(config1);
            Assert.NotNull(config2);
            Assert.NotSame(config1, config2);
        }

        /// <summary>
        /// Tests that On method returns the same configuration instance when called multiple times with the same platform type.
        /// Verifies the internal configuration system maintains consistency.
        /// </summary>
        [Fact]
        public void On_CalledMultipleTimesWithSamePlatform_ReturnsSameConfiguration()
        {
            // Arrange
            var cell = new TestCell();

            // Act
            var config1 = cell.On<TestPlatform>();
            var config2 = cell.On<TestPlatform>();

            // Assert
            Assert.NotNull(config1);
            Assert.NotNull(config2);
            Assert.Same(config1, config2);
        }

        private class TestPlatform : IConfigPlatform
        {
        }

        private class AnotherTestPlatform : IConfigPlatform
        {
        }

        /// <summary>
        /// Tests that the Registry property returns a non-null PlatformConfigurationRegistry instance.
        /// </summary>
        [Fact]
        public void Registry_WhenAccessed_ReturnsNonNullPlatformConfigurationRegistry()
        {
            // Arrange
            var cell = new TestCell();
            var elementConfiguration = new Cell.ElementConfiguration(cell);

            // Act
            var registry = elementConfiguration.Registry;

            // Assert
            Assert.NotNull(registry);
            Assert.IsType<PlatformConfigurationRegistry<Cell>>(registry);
        }

        /// <summary>
        /// Tests that the Registry property returns the same instance on multiple accesses (lazy initialization behavior).
        /// </summary>
        [Fact]
        public void Registry_WhenAccessedMultipleTimes_ReturnsSameInstance()
        {
            // Arrange
            var cell = new TestCell();
            var elementConfiguration = new Cell.ElementConfiguration(cell);

            // Act
            var registry1 = elementConfiguration.Registry;
            var registry2 = elementConfiguration.Registry;

            // Assert
            Assert.Same(registry1, registry2);
        }

        /// <summary>
        /// Tests that the Registry property throws ArgumentNullException when ElementConfiguration is constructed with null cell.
        /// </summary>
        [Fact]
        public void Registry_WhenConstructedWithNullCell_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => new Cell.ElementConfiguration(null));
            Assert.Equal("element", exception.ParamName);
        }

        /// <summary>
        /// Tests that different ElementConfiguration instances have different Registry instances.
        /// </summary>
        [Fact]
        public void Registry_WhenAccessedFromDifferentElementConfigurations_ReturnsDifferentInstances()
        {
            // Arrange
            var cell1 = new TestCell();
            var cell2 = new TestCell();
            var elementConfiguration1 = new Cell.ElementConfiguration(cell1);
            var elementConfiguration2 = new Cell.ElementConfiguration(cell2);

            // Act
            var registry1 = elementConfiguration1.Registry;
            var registry2 = elementConfiguration2.Registry;

            // Assert
            Assert.NotSame(registry1, registry2);
        }

        /// <summary>
        /// Tests that the Registry property lazy initialization only creates the instance once.
        /// </summary>
        [Fact]
        public void Registry_LazyInitialization_CreatesInstanceOnlyOnce()
        {
            // Arrange
            var cell = new TestCell();
            var elementConfiguration = new Cell.ElementConfiguration(cell);

            // Act - Access Registry multiple times
            var registry1 = elementConfiguration.Registry;
            var registry2 = elementConfiguration.Registry;
            var registry3 = elementConfiguration.Registry;

            // Assert - All references should be the same instance
            Assert.Same(registry1, registry2);
            Assert.Same(registry2, registry3);
            Assert.Same(registry1, registry3);
        }

        /// <summary>
        /// Tests that RenderHeight returns the cell's Height when the parent is a TableView with HasUnevenRows = true and Height > 0.
        /// </summary>
        [Theory]
        [InlineData(50.0)]
        [InlineData(100.5)]
        [InlineData(0.1)]
        public void RenderHeight_TableViewParentWithUnevenRowsAndPositiveHeight_ReturnsHeight(double cellHeight)
        {
            // Arrange
            var cell = new TestCell();
            var mockTableView = Substitute.For<TableView>();
            mockTableView.HasUnevenRows.Returns(true);
            mockTableView.RowHeight.Returns(25);

            cell.Height = cellHeight;

            // Use reflection to set the RealParent property since it's internal
            var realParentProperty = typeof(Element).GetProperty("RealParent");
            realParentProperty.SetValue(cell, mockTableView);

            // Act
            var result = cell.RenderHeight;

            // Assert
            Assert.Equal(cellHeight, result);
        }

        /// <summary>
        /// Tests that RenderHeight returns the TableView's RowHeight when HasUnevenRows = true but Height <= 0.
        /// </summary>
        [Theory]
        [InlineData(0.0, 30)]
        [InlineData(-1.0, 45)]
        [InlineData(-10.5, 20)]
        public void RenderHeight_TableViewParentWithUnevenRowsAndNonPositiveHeight_ReturnsRowHeight(double cellHeight, int tableRowHeight)
        {
            // Arrange
            var cell = new TestCell();
            var mockTableView = Substitute.For<TableView>();
            mockTableView.HasUnevenRows.Returns(true);
            mockTableView.RowHeight.Returns(tableRowHeight);

            cell.Height = cellHeight;

            var realParentProperty = typeof(Element).GetProperty("RealParent");
            realParentProperty.SetValue(cell, mockTableView);

            // Act
            var result = cell.RenderHeight;

            // Assert
            Assert.Equal(tableRowHeight, result);
        }

        /// <summary>
        /// Tests that RenderHeight returns the TableView's RowHeight when HasUnevenRows = false regardless of Height value.
        /// </summary>
        [Theory]
        [InlineData(50.0, 30)]
        [InlineData(0.0, 25)]
        [InlineData(-5.0, 40)]
        public void RenderHeight_TableViewParentWithEvenRows_ReturnsRowHeight(double cellHeight, int tableRowHeight)
        {
            // Arrange
            var cell = new TestCell();
            var mockTableView = Substitute.For<TableView>();
            mockTableView.HasUnevenRows.Returns(false);
            mockTableView.RowHeight.Returns(tableRowHeight);

            cell.Height = cellHeight;

            var realParentProperty = typeof(Element).GetProperty("RealParent");
            realParentProperty.SetValue(cell, mockTableView);

            // Act
            var result = cell.RenderHeight;

            // Assert
            Assert.Equal(tableRowHeight, result);
        }

        /// <summary>
        /// Tests that RenderHeight returns the cell's Height when the parent is a ListView with HasUnevenRows = true and Height > 0.
        /// </summary>
        [Theory]
        [InlineData(75.0)]
        [InlineData(200.5)]
        [InlineData(0.5)]
        public void RenderHeight_ListViewParentWithUnevenRowsAndPositiveHeight_ReturnsHeight(double cellHeight)
        {
            // Arrange
            var cell = new TestCell();
            var mockListView = Substitute.For<ListView>();
            mockListView.HasUnevenRows.Returns(true);
            mockListView.RowHeight.Returns(35);

            cell.Height = cellHeight;

            var realParentProperty = typeof(Element).GetProperty("RealParent");
            realParentProperty.SetValue(cell, mockListView);

            // Act
            var result = cell.RenderHeight;

            // Assert
            Assert.Equal(cellHeight, result);
        }

        /// <summary>
        /// Tests that RenderHeight returns the ListView's RowHeight when HasUnevenRows = true but Height <= 0.
        /// </summary>
        [Theory]
        [InlineData(0.0, 40)]
        [InlineData(-2.0, 55)]
        [InlineData(-15.5, 25)]
        public void RenderHeight_ListViewParentWithUnevenRowsAndNonPositiveHeight_ReturnsRowHeight(double cellHeight, int listRowHeight)
        {
            // Arrange
            var cell = new TestCell();
            var mockListView = Substitute.For<ListView>();
            mockListView.HasUnevenRows.Returns(true);
            mockListView.RowHeight.Returns(listRowHeight);

            cell.Height = cellHeight;

            var realParentProperty = typeof(Element).GetProperty("RealParent");
            realParentProperty.SetValue(cell, mockListView);

            // Act
            var result = cell.RenderHeight;

            // Assert
            Assert.Equal(listRowHeight, result);
        }

        /// <summary>
        /// Tests that RenderHeight returns the ListView's RowHeight when HasUnevenRows = false regardless of Height value.
        /// </summary>
        [Theory]
        [InlineData(80.0, 45)]
        [InlineData(0.0, 30)]
        [InlineData(-3.0, 50)]
        public void RenderHeight_ListViewParentWithEvenRows_ReturnsRowHeight(double cellHeight, int listRowHeight)
        {
            // Arrange
            var cell = new TestCell();
            var mockListView = Substitute.For<ListView>();
            mockListView.HasUnevenRows.Returns(false);
            mockListView.RowHeight.Returns(listRowHeight);

            cell.Height = cellHeight;

            var realParentProperty = typeof(Element).GetProperty("RealParent");
            realParentProperty.SetValue(cell, mockListView);

            // Act
            var result = cell.RenderHeight;

            // Assert
            Assert.Equal(listRowHeight, result);
        }

        /// <summary>
        /// Tests that RenderHeight returns DefaultCellHeight when the parent is neither TableView nor ListView.
        /// </summary>
        [Theory]
        [InlineData(100.0)]
        [InlineData(0.0)]
        [InlineData(-5.0)]
        public void RenderHeight_NonListViewNonTableViewParent_ReturnsDefaultCellHeight(double cellHeight)
        {
            // Arrange
            var cell = new TestCell();
            var mockElement = Substitute.For<Element>();

            cell.Height = cellHeight;

            var realParentProperty = typeof(Element).GetProperty("RealParent");
            realParentProperty.SetValue(cell, mockElement);

            // Act
            var result = cell.RenderHeight;

            // Assert
            Assert.Equal(Cell.DefaultCellHeight, result);
        }

        /// <summary>
        /// Tests that RenderHeight returns DefaultCellHeight when the parent is null.
        /// </summary>
        [Theory]
        [InlineData(150.0)]
        [InlineData(0.0)]
        [InlineData(-10.0)]
        public void RenderHeight_NullParent_ReturnsDefaultCellHeight(double cellHeight)
        {
            // Arrange
            var cell = new TestCell();

            cell.Height = cellHeight;

            var realParentProperty = typeof(Element).GetProperty("RealParent");
            realParentProperty.SetValue(cell, null);

            // Act
            var result = cell.RenderHeight;

            // Assert
            Assert.Equal(Cell.DefaultCellHeight, result);
        }

        /// <summary>
        /// Tests that RenderHeight returns DefaultCellHeight when no parent is set.
        /// </summary>
        [Fact]
        public void RenderHeight_NoParent_ReturnsDefaultCellHeight()
        {
            // Arrange
            var cell = new TestCell();
            cell.Height = 75.0;

            // Act
            var result = cell.RenderHeight;

            // Assert
            Assert.Equal(Cell.DefaultCellHeight, result);
        }

        /// <summary>
        /// Tests edge cases for Height values with TableView parent.
        /// </summary>
        [Theory]
        [InlineData(double.MaxValue, 25)]
        [InlineData(double.MinValue, 30)]
        [InlineData(double.Epsilon, 35)]
        public void RenderHeight_TableViewParentWithExtremeHeightValues_HandlesCorrectly(double cellHeight, int tableRowHeight)
        {
            // Arrange
            var cell = new TestCell();
            var mockTableView = Substitute.For<TableView>();
            mockTableView.HasUnevenRows.Returns(true);
            mockTableView.RowHeight.Returns(tableRowHeight);

            cell.Height = cellHeight;

            var realParentProperty = typeof(Element).GetProperty("RealParent");
            realParentProperty.SetValue(cell, mockTableView);

            // Act
            var result = cell.RenderHeight;

            // Assert
            var expected = cellHeight > 0 ? cellHeight : tableRowHeight;
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests edge cases for Height values with ListView parent.
        /// </summary>
        [Theory]
        [InlineData(double.MaxValue, 40)]
        [InlineData(double.MinValue, 50)]
        [InlineData(double.Epsilon, 60)]
        public void RenderHeight_ListViewParentWithExtremeHeightValues_HandlesCorrectly(double cellHeight, int listRowHeight)
        {
            // Arrange
            var cell = new TestCell();
            var mockListView = Substitute.For<ListView>();
            mockListView.HasUnevenRows.Returns(true);
            mockListView.RowHeight.Returns(listRowHeight);

            cell.Height = cellHeight;

            var realParentProperty = typeof(Element).GetProperty("RealParent");
            realParentProperty.SetValue(cell, mockListView);

            // Act
            var result = cell.RenderHeight;

            // Assert
            var expected = cellHeight > 0 ? cellHeight : listRowHeight;
            Assert.Equal(expected, result);
        }
    }


    public partial class CellElementConfigurationTests : BaseTestFixture
    {
        /// <summary>
        /// Test cell implementation for testing ElementConfiguration constructor
        /// </summary>
        internal class TestCell : Cell
        {
        }

        /// <summary>
        /// Tests that ElementConfiguration constructor properly initializes with a valid cell parameter.
        /// Verifies that the internal lazy registry is created and the object is in a valid state.
        /// </summary>
        [Fact]
        public void ElementConfiguration_Constructor_ValidCell_InitializesSuccessfully()
        {
            // Arrange
            var cell = new TestCell();

            // Act
            var elementConfig = new Cell.ElementConfiguration(cell);

            // Assert
            Assert.NotNull(elementConfig);
            Assert.NotNull(elementConfig.Registry);
        }

        /// <summary>
        /// Tests that ElementConfiguration constructor handles null cell parameter.
        /// Constructor should complete successfully, but accessing registry might cause issues.
        /// </summary>
        [Fact]
        public void ElementConfiguration_Constructor_NullCell_InitializesWithoutException()
        {
            // Arrange
            Cell cell = null;

            // Act & Assert
            var elementConfig = new Cell.ElementConfiguration(cell);
            Assert.NotNull(elementConfig);
        }

        /// <summary>
        /// Tests that ElementConfiguration constructor with null cell throws when accessing registry.
        /// The lazy initialization should work, but the underlying registry creation should succeed
        /// since PlatformConfigurationRegistry accepts null elements.
        /// </summary>
        [Fact]
        public void ElementConfiguration_Constructor_NullCell_RegistryAccessSucceeds()
        {
            // Arrange
            Cell cell = null;
            var elementConfig = new Cell.ElementConfiguration(cell);

            // Act & Assert - Registry should be accessible even with null cell
            Assert.NotNull(elementConfig.Registry);
        }

        /// <summary>
        /// Tests that multiple ElementConfiguration instances can be created independently.
        /// Verifies that each instance maintains its own registry state.
        /// </summary>
        [Fact]
        public void ElementConfiguration_Constructor_MultipleInstances_CreateIndependentObjects()
        {
            // Arrange
            var cell1 = new TestCell();
            var cell2 = new TestCell();

            // Act
            var elementConfig1 = new Cell.ElementConfiguration(cell1);
            var elementConfig2 = new Cell.ElementConfiguration(cell2);

            // Assert
            Assert.NotNull(elementConfig1);
            Assert.NotNull(elementConfig2);
            Assert.NotSame(elementConfig1, elementConfig2);
            Assert.NotSame(elementConfig1.Registry, elementConfig2.Registry);
        }

        /// <summary>
        /// Tests that ElementConfiguration lazy registry initialization works correctly.
        /// Verifies that accessing Registry property multiple times returns the same instance.
        /// </summary>
        [Fact]
        public void ElementConfiguration_Constructor_LazyRegistryInitialization_ReturnsSameInstance()
        {
            // Arrange
            var cell = new TestCell();
            var elementConfig = new Cell.ElementConfiguration(cell);

            // Act
            var registry1 = elementConfig.Registry;
            var registry2 = elementConfig.Registry;

            // Assert
            Assert.NotNull(registry1);
            Assert.NotNull(registry2);
            Assert.Same(registry1, registry2);
        }

        /// <summary>
        /// Tests that the On method returns a non-null platform element configuration.
        /// </summary>
        [Fact]
        public void On_WithValidPlatformType_ReturnsNonNullConfiguration()
        {
            // Arrange
            var testCell = new TestCell();
            var elementConfiguration = new Cell.ElementConfiguration(testCell);

            // Act
            var result = elementConfiguration.On<TestPlatform>();

            // Assert
            Assert.NotNull(result);
        }

        /// <summary>
        /// Tests that the On method returns the same configuration instance on subsequent calls with the same platform type.
        /// </summary>
        [Fact]
        public void On_CalledMultipleTimesWithSamePlatform_ReturnsSameInstance()
        {
            // Arrange
            var testCell = new TestCell();
            var elementConfiguration = new Cell.ElementConfiguration(testCell);

            // Act
            var result1 = elementConfiguration.On<TestPlatform>();
            var result2 = elementConfiguration.On<TestPlatform>();

            // Assert
            Assert.Same(result1, result2);
        }

        /// <summary>
        /// Tests that the On method returns different configuration instances for different platform types.
        /// </summary>
        [Fact]
        public void On_CalledWithDifferentPlatforms_ReturnsDifferentInstances()
        {
            // Arrange
            var testCell = new TestCell();
            var elementConfiguration = new Cell.ElementConfiguration(testCell);

            // Act
            var result1 = elementConfiguration.On<TestPlatform>();
            var result2 = elementConfiguration.On<AnotherTestPlatform>();

            // Assert
            Assert.NotSame(result1, result2);
        }

        internal class TestPlatform : IConfigPlatform
        {
        }

        internal class AnotherTestPlatform : IConfigPlatform
        {
        }
    }
}