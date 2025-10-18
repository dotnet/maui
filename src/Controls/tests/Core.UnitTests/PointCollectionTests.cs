#nullable disable

using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Shapes.UnitTests
{
    public class PointCollectionTests
    {
        PointCollectionConverter _pointCollectionConverter;


        public PointCollectionTests()
        {
            _pointCollectionConverter = new PointCollectionConverter();
        }

        [Fact]
        public void ConvertStringToPointCollectionTest()
        {
            PointCollection result = _pointCollectionConverter.ConvertFromInvariantString("0 48, 0 144, 96 150, 100 0, 192 0, 192 96, 50 96, 48 192, 150 200 144 48") as PointCollection;

            Assert.NotNull(result);
            Assert.Equal(10, result.Count);
        }

        /// <summary>
        /// Tests that the constructor throws ArgumentNullException when passed a null array.
        /// Input: null Point array.
        /// Expected: ArgumentNullException is thrown.
        /// </summary>
        [Fact]
        public void Constructor_NullArray_ThrowsArgumentNullException()
        {
            // Arrange
            Point[] nullArray = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new PointCollection(nullArray));
        }

        /// <summary>
        /// Tests that the constructor creates an empty collection when passed an empty array.
        /// Input: empty Point array.
        /// Expected: PointCollection with Count of 0.
        /// </summary>
        [Fact]
        public void Constructor_EmptyArray_CreatesEmptyCollection()
        {
            // Arrange
            Point[] emptyArray = new Point[0];

            // Act
            var collection = new PointCollection(emptyArray);

            // Assert
            Assert.Empty(collection);
            Assert.Equal(0, collection.Count);
        }

        /// <summary>
        /// Tests that the constructor correctly copies a single element from the array.
        /// Input: Point array with one element.
        /// Expected: PointCollection with the same single element.
        /// </summary>
        [Fact]
        public void Constructor_SingleElementArray_CopiesElement()
        {
            // Arrange
            var point = new Point(10.5, 20.7);
            Point[] singleArray = new Point[] { point };

            // Act
            var collection = new PointCollection(singleArray);

            // Assert
            Assert.Single(collection);
            Assert.Equal(point, collection[0]);
            Assert.Equal(10.5, collection[0].X);
            Assert.Equal(20.7, collection[0].Y);
        }

        /// <summary>
        /// Tests that the constructor correctly copies multiple elements from the array in order.
        /// Input: Point array with multiple elements.
        /// Expected: PointCollection with all elements in the same order.
        /// </summary>
        [Fact]
        public void Constructor_MultipleElementsArray_CopiesAllElements()
        {
            // Arrange
            Point[] multipleArray = new Point[]
            {
                new Point(1, 2),
                new Point(3.5, 4.8),
                new Point(-5, -6.2),
                Point.Zero
            };

            // Act
            var collection = new PointCollection(multipleArray);

            // Assert
            Assert.Equal(4, collection.Count);
            Assert.Equal(new Point(1, 2), collection[0]);
            Assert.Equal(new Point(3.5, 4.8), collection[1]);
            Assert.Equal(new Point(-5, -6.2), collection[2]);
            Assert.Equal(Point.Zero, collection[3]);
        }

        /// <summary>
        /// Tests that the constructor preserves duplicate points in the array.
        /// Input: Point array with duplicate points.
        /// Expected: PointCollection with all duplicates preserved.
        /// </summary>
        [Fact]
        public void Constructor_ArrayWithDuplicates_PreservesDuplicates()
        {
            // Arrange
            var duplicatePoint = new Point(5, 5);
            Point[] arrayWithDuplicates = new Point[]
            {
                duplicatePoint,
                new Point(1, 2),
                duplicatePoint,
                duplicatePoint
            };

            // Act
            var collection = new PointCollection(arrayWithDuplicates);

            // Assert
            Assert.Equal(4, collection.Count);
            Assert.Equal(duplicatePoint, collection[0]);
            Assert.Equal(new Point(1, 2), collection[1]);
            Assert.Equal(duplicatePoint, collection[2]);
            Assert.Equal(duplicatePoint, collection[3]);
        }

        /// <summary>
        /// Tests that the constructor handles extreme double values correctly.
        /// Input: Point array with extreme double values including infinity and NaN.
        /// Expected: PointCollection with all extreme values preserved.
        /// </summary>
        [Fact]
        public void Constructor_ArrayWithExtremeValues_CopiesAllValues()
        {
            // Arrange
            Point[] extremeArray = new Point[]
            {
                new Point(double.MinValue, double.MaxValue),
                new Point(double.PositiveInfinity, double.NegativeInfinity),
                new Point(double.NaN, 0),
                new Point(0, double.NaN)
            };

            // Act
            var collection = new PointCollection(extremeArray);

            // Assert
            Assert.Equal(4, collection.Count);
            Assert.Equal(double.MinValue, collection[0].X);
            Assert.Equal(double.MaxValue, collection[0].Y);
            Assert.Equal(double.PositiveInfinity, collection[1].X);
            Assert.Equal(double.NegativeInfinity, collection[1].Y);
            Assert.True(double.IsNaN(collection[2].X));
            Assert.Equal(0, collection[2].Y);
            Assert.Equal(0, collection[3].X);
            Assert.True(double.IsNaN(collection[3].Y));
        }

        /// <summary>
        /// Tests that modifying the original array after construction does not affect the collection.
        /// Input: Point array that is modified after creating the collection.
        /// Expected: PointCollection remains unchanged after array modification.
        /// </summary>
        [Fact]
        public void Constructor_ModifyOriginalArrayAfterConstruction_CollectionUnaffected()
        {
            // Arrange
            Point[] originalArray = new Point[]
            {
                new Point(1, 2),
                new Point(3, 4)
            };

            // Act
            var collection = new PointCollection(originalArray);
            var originalFirstPoint = collection[0];
            var originalSecondPoint = collection[1];

            // Modify the original array
            originalArray[0] = new Point(100, 200);
            originalArray[1] = new Point(300, 400);

            // Assert
            Assert.Equal(2, collection.Count);
            Assert.Equal(originalFirstPoint, collection[0]);
            Assert.Equal(originalSecondPoint, collection[1]);
            Assert.NotEqual(originalArray[0], collection[0]);
            Assert.NotEqual(originalArray[1], collection[1]);
        }
    }
}