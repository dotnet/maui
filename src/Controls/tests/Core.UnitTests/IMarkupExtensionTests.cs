#nullable disable

using Microsoft.Maui.Controls.Xaml;
using System;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class RequireServiceAttributeTests
    {
        /// <summary>
        /// Tests that the RequireServiceAttribute constructor correctly assigns a valid service types array to the ServiceTypes property.
        /// Input: Valid array of Type objects.
        /// Expected: ServiceTypes property returns the same array reference.
        /// </summary>
        [Fact]
        public void Constructor_ValidServiceTypesArray_AssignsToServiceTypesProperty()
        {
            // Arrange
            Type[] serviceTypes = { typeof(string), typeof(int), typeof(object) };

            // Act
            var attribute = new RequireServiceAttribute(serviceTypes);

            // Assert
            Assert.Same(serviceTypes, attribute.ServiceTypes);
        }

        /// <summary>
        /// Tests that the RequireServiceAttribute constructor accepts a null service types array.
        /// Input: null array.
        /// Expected: ServiceTypes property returns null.
        /// </summary>
        [Fact]
        public void Constructor_NullServiceTypesArray_AssignsNullToServiceTypesProperty()
        {
            // Arrange
            Type[] serviceTypes = null;

            // Act
            var attribute = new RequireServiceAttribute(serviceTypes);

            // Assert
            Assert.Null(attribute.ServiceTypes);
        }

        /// <summary>
        /// Tests that the RequireServiceAttribute constructor correctly assigns an empty service types array to the ServiceTypes property.
        /// Input: Empty array of Type objects.
        /// Expected: ServiceTypes property returns the same empty array reference.
        /// </summary>
        [Fact]
        public void Constructor_EmptyServiceTypesArray_AssignsToServiceTypesProperty()
        {
            // Arrange
            Type[] serviceTypes = { };

            // Act
            var attribute = new RequireServiceAttribute(serviceTypes);

            // Assert
            Assert.Same(serviceTypes, attribute.ServiceTypes);
            Assert.Empty(attribute.ServiceTypes);
        }

        /// <summary>
        /// Tests that the RequireServiceAttribute constructor accepts an array containing null elements.
        /// Input: Array with null Type elements.
        /// Expected: ServiceTypes property returns the same array with null elements.
        /// </summary>
        [Fact]
        public void Constructor_ServiceTypesArrayWithNullElements_AssignsToServiceTypesProperty()
        {
            // Arrange
            Type[] serviceTypes = { typeof(string), null, typeof(int) };

            // Act
            var attribute = new RequireServiceAttribute(serviceTypes);

            // Assert
            Assert.Same(serviceTypes, attribute.ServiceTypes);
            Assert.Equal(3, attribute.ServiceTypes.Length);
            Assert.Equal(typeof(string), attribute.ServiceTypes[0]);
            Assert.Null(attribute.ServiceTypes[1]);
            Assert.Equal(typeof(int), attribute.ServiceTypes[2]);
        }

        /// <summary>
        /// Tests that the RequireServiceAttribute constructor accepts an array with duplicate Type elements.
        /// Input: Array with duplicate Type objects.
        /// Expected: ServiceTypes property returns the same array with duplicates preserved.
        /// </summary>
        [Fact]
        public void Constructor_ServiceTypesArrayWithDuplicates_AssignsToServiceTypesProperty()
        {
            // Arrange
            Type[] serviceTypes = { typeof(string), typeof(string), typeof(int) };

            // Act
            var attribute = new RequireServiceAttribute(serviceTypes);

            // Assert
            Assert.Same(serviceTypes, attribute.ServiceTypes);
            Assert.Equal(3, attribute.ServiceTypes.Length);
            Assert.Equal(typeof(string), attribute.ServiceTypes[0]);
            Assert.Equal(typeof(string), attribute.ServiceTypes[1]);
            Assert.Equal(typeof(int), attribute.ServiceTypes[2]);
        }

        /// <summary>
        /// Tests that the RequireServiceAttribute constructor accepts a single-element array.
        /// Input: Array with one Type object.
        /// Expected: ServiceTypes property returns the same single-element array.
        /// </summary>
        [Fact]
        public void Constructor_SingleElementServiceTypesArray_AssignsToServiceTypesProperty()
        {
            // Arrange
            Type[] serviceTypes = { typeof(string) };

            // Act
            var attribute = new RequireServiceAttribute(serviceTypes);

            // Assert
            Assert.Same(serviceTypes, attribute.ServiceTypes);
            Assert.Single(attribute.ServiceTypes);
            Assert.Equal(typeof(string), attribute.ServiceTypes[0]);
        }
    }
}
