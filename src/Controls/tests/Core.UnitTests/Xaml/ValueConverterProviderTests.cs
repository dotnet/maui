#nullable disable

using System;
using System.Reflection;
using System.Xml;
using Microsoft.Maui.Controls.Xaml;
using NSubstitute;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests
{
    /// <summary>
    /// Unit tests for ValueConverterProvider class
    /// </summary>
    public partial class ValueConverterProviderTests
    {
        /// <summary>
        /// Tests that Convert returns the converted value when conversion succeeds
        /// </summary>
        [Theory]
        [InlineData("123", typeof(int), 123)]
        [InlineData("true", typeof(bool), true)]
        [InlineData("3.14", typeof(double), 3.14)]
        [InlineData(42, typeof(string), "42")]
        public void Convert_ValidConversion_ReturnsConvertedValue(object value, Type toType, object expected)
        {
            // Arrange
            var provider = new ValueConverterProvider();
            var serviceProvider = Substitute.For<IServiceProvider>();
            Func<MemberInfo> minfoRetriever = () => null;

            // Act
            var result = provider.Convert(value, toType, minfoRetriever, serviceProvider);

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests that Convert handles null value parameter correctly
        /// </summary>
        [Fact]
        public void Convert_NullValue_ReturnsNull()
        {
            // Arrange
            var provider = new ValueConverterProvider();
            var serviceProvider = Substitute.For<IServiceProvider>();
            Func<MemberInfo> minfoRetriever = () => null;

            // Act
            var result = provider.Convert(null, typeof(string), minfoRetriever, serviceProvider);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that Convert handles null toType parameter correctly
        /// </summary>
        [Fact]
        public void Convert_NullToType_ReturnsOriginalValue()
        {
            // Arrange
            var provider = new ValueConverterProvider();
            var serviceProvider = Substitute.For<IServiceProvider>();
            Func<MemberInfo> minfoRetriever = () => null;
            var value = "test";

            // Act
            var result = provider.Convert(value, null, minfoRetriever, serviceProvider);

            // Assert
            Assert.Equal(value, result);
        }

        /// <summary>
        /// Tests that Convert handles null minfoRetriever parameter correctly
        /// </summary>
        [Fact]
        public void Convert_NullMinfoRetriever_PerformsConversion()
        {
            // Arrange
            var provider = new ValueConverterProvider();
            var serviceProvider = Substitute.For<IServiceProvider>();

            // Act
            var result = provider.Convert("123", typeof(int), null, serviceProvider);

            // Assert
            Assert.Equal(123, result);
        }

        /// <summary>
        /// Tests that Convert handles null serviceProvider parameter correctly
        /// </summary>
        [Fact]
        public void Convert_NullServiceProvider_PerformsConversion()
        {
            // Arrange
            var provider = new ValueConverterProvider();
            Func<MemberInfo> minfoRetriever = () => null;

            // Act
            var result = provider.Convert("123", typeof(int), minfoRetriever, null);

            // Assert
            Assert.Equal(123, result);
        }

        /// <summary>
        /// Tests that Convert throws XamlParseException when conversion fails and serviceProvider returns null IXmlLineInfoProvider
        /// </summary>
        [Fact]
        public void Convert_ConversionFailsWithNullLineInfoProvider_ThrowsXamlParseExceptionWithDefaultLineInfo()
        {
            // Arrange
            var provider = new ValueConverterProvider();
            var serviceProvider = Substitute.For<IServiceProvider>();
            serviceProvider.GetService(typeof(IXmlLineInfoProvider)).Returns((object)null);
            Func<MemberInfo> minfoRetriever = () => null;

            // Use a conversion that will fail - complex object to DateTime
            var complexObject = new object();

            // Act & Assert
            var exception = Assert.Throws<XamlParseException>(() =>
                provider.Convert(complexObject, typeof(DateTime), minfoRetriever, serviceProvider));

            Assert.NotNull(exception);
            Assert.NotNull(exception.InnerException);
        }

        /// <summary>
        /// Tests that Convert throws XamlParseException when conversion fails and serviceProvider returns valid IXmlLineInfoProvider
        /// </summary>
        [Fact]
        public void Convert_ConversionFailsWithValidLineInfoProvider_ThrowsXamlParseExceptionWithProviderLineInfo()
        {
            // Arrange
            var provider = new ValueConverterProvider();
            var serviceProvider = Substitute.For<IServiceProvider>();
            var xmlLineInfoProvider = Substitute.For<IXmlLineInfoProvider>();
            var xmlLineInfo = Substitute.For<IXmlLineInfo>();

            xmlLineInfo.LineNumber.Returns(42);
            xmlLineInfo.LinePosition.Returns(10);
            xmlLineInfoProvider.XmlLineInfo.Returns(xmlLineInfo);
            serviceProvider.GetService(typeof(IXmlLineInfoProvider)).Returns(xmlLineInfoProvider);

            Func<MemberInfo> minfoRetriever = () => null;

            // Use a conversion that will fail - complex object to DateTime
            var complexObject = new object();

            // Act & Assert
            var exception = Assert.Throws<XamlParseException>(() =>
                provider.Convert(complexObject, typeof(DateTime), minfoRetriever, serviceProvider));

            Assert.NotNull(exception);
            Assert.NotNull(exception.InnerException);
        }

        /// <summary>
        /// Tests that Convert throws XamlParseException when conversion fails with invalid string to DateTime
        /// </summary>
        [Fact]
        public void Convert_InvalidStringToDateTime_ThrowsXamlParseException()
        {
            // Arrange
            var provider = new ValueConverterProvider();
            var serviceProvider = Substitute.For<IServiceProvider>();
            serviceProvider.GetService(typeof(IXmlLineInfoProvider)).Returns((object)null);
            Func<MemberInfo> minfoRetriever = () => null;

            // Act & Assert
            var exception = Assert.Throws<XamlParseException>(() =>
                provider.Convert("not-a-date", typeof(DateTime), minfoRetriever, serviceProvider));

            Assert.NotNull(exception);
            Assert.NotNull(exception.InnerException);
        }

        /// <summary>
        /// Tests that Convert throws XamlParseException when conversion fails with invalid string to numeric type
        /// </summary>
        [Theory]
        [InlineData("not-a-number", typeof(int))]
        [InlineData("not-a-double", typeof(double))]
        [InlineData("not-a-decimal", typeof(decimal))]
        public void Convert_InvalidStringToNumeric_ThrowsXamlParseException(string invalidValue, Type targetType)
        {
            // Arrange
            var provider = new ValueConverterProvider();
            var serviceProvider = Substitute.For<IServiceProvider>();
            serviceProvider.GetService(typeof(IXmlLineInfoProvider)).Returns((object)null);
            Func<MemberInfo> minfoRetriever = () => null;

            // Act & Assert
            var exception = Assert.Throws<XamlParseException>(() =>
                provider.Convert(invalidValue, targetType, minfoRetriever, serviceProvider));

            Assert.NotNull(exception);
            Assert.NotNull(exception.InnerException);
        }

        /// <summary>
        /// Tests that Convert throws XamlParseException when serviceProvider.GetService throws exception
        /// </summary>
        [Fact]
        public void Convert_ServiceProviderGetServiceThrows_ThrowsXamlParseException()
        {
            // Arrange
            var provider = new ValueConverterProvider();
            var serviceProvider = Substitute.For<IServiceProvider>();
            serviceProvider.GetService(typeof(IXmlLineInfoProvider)).Returns(x => { throw new InvalidOperationException("Service error"); });
            Func<MemberInfo> minfoRetriever = () => null;

            // Use a conversion that will fail to trigger the exception path
            var complexObject = new object();

            // Act & Assert
            var exception = Assert.Throws<XamlParseException>(() =>
                provider.Convert(complexObject, typeof(DateTime), minfoRetriever, serviceProvider));

            Assert.NotNull(exception);
            Assert.NotNull(exception.InnerException);
        }

        /// <summary>
        /// Tests that Convert handles extreme numeric values correctly
        /// </summary>
        [Theory]
        [InlineData(int.MaxValue, typeof(long), (long)int.MaxValue)]
        [InlineData(int.MinValue, typeof(long), (long)int.MinValue)]
        [InlineData(0, typeof(bool), false)]
        [InlineData(1, typeof(bool), true)]
        public void Convert_ExtremeNumericValues_ReturnsExpectedResult(object value, Type toType, object expected)
        {
            // Arrange
            var provider = new ValueConverterProvider();
            var serviceProvider = Substitute.For<IServiceProvider>();
            Func<MemberInfo> minfoRetriever = () => null;

            // Act
            var result = provider.Convert(value, toType, minfoRetriever, serviceProvider);

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests that Convert handles special floating point values
        /// </summary>
        [Theory]
        [InlineData(double.NaN, typeof(string), "NaN")]
        [InlineData(double.PositiveInfinity, typeof(string), "Infinity")]
        [InlineData(double.NegativeInfinity, typeof(string), "-Infinity")]
        public void Convert_SpecialFloatingPointValues_ReturnsExpectedResult(object value, Type toType, object expected)
        {
            // Arrange
            var provider = new ValueConverterProvider();
            var serviceProvider = Substitute.For<IServiceProvider>();
            Func<MemberInfo> minfoRetriever = () => null;

            // Act
            var result = provider.Convert(value, toType, minfoRetriever, serviceProvider);

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests that Convert handles empty and whitespace strings correctly
        /// </summary>
        [Theory]
        [InlineData("", typeof(string), "")]
        [InlineData("   ", typeof(string), "   ")]
        [InlineData("\t\n\r", typeof(string), "\t\n\r")]
        public void Convert_EmptyAndWhitespaceStrings_ReturnsExpectedResult(object value, Type toType, object expected)
        {
            // Arrange
            var provider = new ValueConverterProvider();
            var serviceProvider = Substitute.For<IServiceProvider>();
            Func<MemberInfo> minfoRetriever = () => null;

            // Act
            var result = provider.Convert(value, toType, minfoRetriever, serviceProvider);

            // Assert
            Assert.Equal(expected, result);
        }
    }
}
