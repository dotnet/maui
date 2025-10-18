#nullable disable

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests.Shapes
{
    public partial class PathTests
    {
        /// <summary>
        /// Tests that the RenderTransform property setter correctly stores a valid Transform object.
        /// Validates that SetValue is called with the correct BindableProperty and value.
        /// </summary>
        [Fact]
        public void RenderTransform_SetValidTransform_StoresValueCorrectly()
        {
            // Arrange
            var path = new Path();
            var transform = Substitute.For<Transform>();

            // Act
            path.RenderTransform = transform;

            // Assert
            Assert.Equal(transform, path.RenderTransform);
        }

        /// <summary>
        /// Tests that the RenderTransform property setter correctly handles null values.
        /// Validates that null can be assigned and retrieved from the property.
        /// </summary>
        [Fact]
        public void RenderTransform_SetNull_StoresNullCorrectly()
        {
            // Arrange
            var path = new Path();

            // Act
            path.RenderTransform = null;

            // Assert
            Assert.Null(path.RenderTransform);
        }

        /// <summary>
        /// Tests that the RenderTransform property getter returns null when no value has been set.
        /// Validates the default state behavior of the property.
        /// </summary>
        [Fact]
        public void RenderTransform_GetWithoutSetting_ReturnsNull()
        {
            // Arrange
            var path = new Path();

            // Act
            var result = path.RenderTransform;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that the RenderTransform property correctly handles multiple assignments.
        /// Validates that subsequent assignments overwrite previous values correctly.
        /// </summary>
        [Fact]
        public void RenderTransform_SetMultipleValues_StoresLatestValueCorrectly()
        {
            // Arrange
            var path = new Path();
            var firstTransform = Substitute.For<Transform>();
            var secondTransform = Substitute.For<Transform>();

            // Act
            path.RenderTransform = firstTransform;
            path.RenderTransform = secondTransform;

            // Assert
            Assert.Equal(secondTransform, path.RenderTransform);
            Assert.NotEqual(firstTransform, path.RenderTransform);
        }

        /// <summary>
        /// Tests that the RenderTransform property correctly handles setting null after a valid value.
        /// Validates that null assignment overwrites previously set Transform objects.
        /// </summary>
        [Fact]
        public void RenderTransform_SetValidThenNull_StoresNullCorrectly()
        {
            // Arrange
            var path = new Path();
            var transform = Substitute.For<Transform>();

            // Act
            path.RenderTransform = transform;
            path.RenderTransform = null;

            // Assert
            Assert.Null(path.RenderTransform);
        }

        /// <summary>
        /// Tests that the RenderTransform property works correctly with a real Transform instance.
        /// Validates integration with concrete Transform objects rather than mocks.
        /// </summary>
        [Fact]
        public void RenderTransform_SetRealTransform_StoresValueCorrectly()
        {
            // Arrange
            var path = new Path();
            var transform = new Transform();

            // Act
            path.RenderTransform = transform;

            // Assert
            Assert.Equal(transform, path.RenderTransform);
        }

        /// <summary>
        /// Tests that GetPath returns a new PathF instance when Data is null.
        /// The method should create a new PathF, skip calling AppendPath since Data is null,
        /// and return the empty PathF instance.
        /// </summary>
        [Fact]
        public void GetPath_WhenDataIsNull_ReturnsNewEmptyPathF()
        {
            // Arrange
            var path = new Path();
            // Data property will be null by default

            // Act
            var result = path.GetPath();

            // Assert
            Assert.NotNull(result);
            Assert.IsType<PathF>(result);
        }

        /// <summary>
        /// Tests that GetPath returns a new PathF instance and calls AppendPath when Data is not null.
        /// The method should create a new PathF, call AppendPath on the Data geometry,
        /// and return the PathF instance.
        /// </summary>
        [Fact]
        public void GetPath_WhenDataIsNotNull_CallsAppendPathAndReturnsPathF()
        {
            // Arrange
            var mockGeometry = Substitute.For<Geometry>();
            var path = new Path { Data = mockGeometry };

            // Act
            var result = path.GetPath();

            // Assert
            Assert.NotNull(result);
            Assert.IsType<PathF>(result);
            mockGeometry.Received(1).AppendPath(Arg.Is<PathF>(p => p == result));
        }

        /// <summary>
        /// Tests that GetPath returns different PathF instances on multiple calls.
        /// Each call should create a new PathF instance, ensuring no caching behavior.
        /// </summary>
        [Fact]
        public void GetPath_MultipleCallsWithNullData_ReturnsDifferentPathFInstances()
        {
            // Arrange
            var path = new Path();

            // Act
            var result1 = path.GetPath();
            var result2 = path.GetPath();

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.NotSame(result1, result2);
        }

        /// <summary>
        /// Tests that GetPath returns different PathF instances on multiple calls when Data is not null.
        /// Each call should create a new PathF instance and call AppendPath on each instance.
        /// </summary>
        [Fact]
        public void GetPath_MultipleCallsWithData_ReturnsDifferentPathFInstancesAndCallsAppendPath()
        {
            // Arrange
            var mockGeometry = Substitute.For<Geometry>();
            var path = new Path { Data = mockGeometry };

            // Act
            var result1 = path.GetPath();
            var result2 = path.GetPath();

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.NotSame(result1, result2);
            mockGeometry.Received(2).AppendPath(Arg.Any<PathF>());
            mockGeometry.Received(1).AppendPath(result1);
            mockGeometry.Received(1).AppendPath(result2);
        }

        /// <summary>
        /// Tests that GetPath works correctly when Data is set after construction.
        /// The method should respect the current value of the Data property regardless of when it was set.
        /// </summary>
        [Fact]
        public void GetPath_WhenDataSetAfterConstruction_CallsAppendPathOnCurrentData()
        {
            // Arrange
            var mockGeometry = Substitute.For<Geometry>();
            var path = new Path();

            // Act - first call with null data
            var resultWithNullData = path.GetPath();

            // Set data and call again
            path.Data = mockGeometry;
            var resultWithData = path.GetPath();

            // Assert
            Assert.NotNull(resultWithNullData);
            Assert.NotNull(resultWithData);
            Assert.NotSame(resultWithNullData, resultWithData);
            mockGeometry.Received(1).AppendPath(resultWithData);
            mockGeometry.DidNotReceive().AppendPath(resultWithNullData);
        }

        /// <summary>
        /// Tests that the Path constructor with Geometry parameter correctly sets the Data property.
        /// Input: A valid mock Geometry object.
        /// Expected: Data property should be set to the provided geometry object.
        /// </summary>
        [Fact]
        public void Constructor_WithValidGeometry_SetsDataProperty()
        {
            // Arrange
            var mockGeometry = Substitute.For<Geometry>();

            // Act
            var path = new Path(mockGeometry);

            // Assert
            Assert.Equal(mockGeometry, path.Data);
        }

        /// <summary>
        /// Tests that the Path constructor with null Geometry parameter sets the Data property to null.
        /// Input: null geometry.
        /// Expected: Data property should be set to null.
        /// </summary>
        [Fact]
        public void Constructor_WithNullGeometry_SetsDataPropertyToNull()
        {
            // Arrange
            Geometry nullGeometry = null;

            // Act
            var path = new Path(nullGeometry);

            // Assert
            Assert.Null(path.Data);
        }

        /// <summary>
        /// Tests that the Path constructor with Geometry parameter calls the parameterless constructor.
        /// Input: A valid mock Geometry object.
        /// Expected: Path should be properly initialized with default values from base constructor.
        /// </summary>
        [Fact]
        public void Constructor_WithGeometry_CallsParameterlessConstructor()
        {
            // Arrange
            var mockGeometry = Substitute.For<Geometry>();

            // Act
            var path = new Path(mockGeometry);

            // Assert
            Assert.NotNull(path);
            Assert.Equal(mockGeometry, path.Data);
        }
    }
}