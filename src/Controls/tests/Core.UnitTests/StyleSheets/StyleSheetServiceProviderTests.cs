#nullable disable

using System;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.StyleSheets;
using Microsoft.Maui.Controls.Xaml;
using Xunit;

namespace Microsoft.Maui.Controls.StyleSheets.UnitTests
{
    public class StyleSheetServiceProviderTests
    {
        /// <summary>
        /// Tests that the IgnoreCase property of ConverterOptions returns true.
        /// This verifies the hardcoded behavior of the property implementation.
        /// </summary>
        [Fact]
        public void ConverterOptions_IgnoreCase_ReturnsTrue()
        {
            // Arrange
            var converterOptions = new StyleSheetServiceProvider.ConverterOptions();

            // Act
            bool result = converterOptions.IgnoreCase;

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that GetService returns the vtProvider when requesting IProvideValueTarget service type.
        /// </summary>
        [Fact]
        public void GetService_IProvideValueTargetType_ReturnsVtProvider()
        {
            // Arrange
            var targetObject = new object();
            var targetProperty = new object();
            var serviceProvider = new StyleSheetServiceProvider(targetObject, targetProperty);

            // Act
            var result = serviceProvider.GetService(typeof(IProvideValueTarget));

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IProvideValueTarget>(result);
            var provideValueTarget = (IProvideValueTarget)result;
            Assert.Same(targetObject, provideValueTarget.TargetObject);
            Assert.Same(targetProperty, provideValueTarget.TargetProperty);
        }

        /// <summary>
        /// Tests that GetService returns the convOptions when requesting IConverterOptions service type.
        /// </summary>
        [Fact]
        public void GetService_IConverterOptionsType_ReturnsConverterOptions()
        {
            // Arrange
            var targetObject = new object();
            var targetProperty = new object();
            var serviceProvider = new StyleSheetServiceProvider(targetObject, targetProperty);

            // Act
            var result = serviceProvider.GetService(typeof(IConverterOptions));

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IConverterOptions>(result);
            var converterOptions = (IConverterOptions)result;
            Assert.True(converterOptions.IgnoreCase);
        }

        /// <summary>
        /// Tests that GetService returns null when passed a null service type.
        /// </summary>
        [Fact]
        public void GetService_NullServiceType_ReturnsNull()
        {
            // Arrange
            var targetObject = new object();
            var targetProperty = new object();
            var serviceProvider = new StyleSheetServiceProvider(targetObject, targetProperty);

            // Act
            var result = serviceProvider.GetService(null);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that GetService returns null when requesting unknown service types.
        /// </summary>
        /// <param name="serviceType">The unknown service type to request.</param>
        [Theory]
        [InlineData(typeof(string))]
        [InlineData(typeof(int))]
        [InlineData(typeof(object))]
        [InlineData(typeof(DateTime))]
        public void GetService_UnknownServiceType_ReturnsNull(Type serviceType)
        {
            // Arrange
            var targetObject = new object();
            var targetProperty = new object();
            var serviceProvider = new StyleSheetServiceProvider(targetObject, targetProperty);

            // Act
            var result = serviceProvider.GetService(serviceType);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that GetService returns a new instance of IConverterOptions on each call.
        /// </summary>
        [Fact]
        public void GetService_IConverterOptionsType_ReturnsNewInstanceEachCall()
        {
            // Arrange
            var targetObject = new object();
            var targetProperty = new object();
            var serviceProvider = new StyleSheetServiceProvider(targetObject, targetProperty);

            // Act
            var result1 = serviceProvider.GetService(typeof(IConverterOptions));
            var result2 = serviceProvider.GetService(typeof(IConverterOptions));

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.NotSame(result1, result2);
        }

        /// <summary>
        /// Tests that GetService returns the same IProvideValueTarget instance on multiple calls.
        /// </summary>
        [Fact]
        public void GetService_IProvideValueTargetType_ReturnsSameInstanceOnMultipleCalls()
        {
            // Arrange
            var targetObject = new object();
            var targetProperty = new object();
            var serviceProvider = new StyleSheetServiceProvider(targetObject, targetProperty);

            // Act
            var result1 = serviceProvider.GetService(typeof(IProvideValueTarget));
            var result2 = serviceProvider.GetService(typeof(IProvideValueTarget));

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.Same(result1, result2);
        }
    }
}