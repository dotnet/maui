#nullable disable
//
// TypeConversionExtensionsTests.cs
//
// Author:
//       Stephane Delcroix <stephane@mi8.be>
//
// Copyright (c) 2013 Mobile Inception
// Copyright (c) 2014 Xamarin, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Controls.Xaml.Internals;
using NSubstitute;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class TypeConversionExtensionsTests
    {
        /// <summary>
        /// Tests ConvertTo method with null converter type to ensure it follows the null converter path
        /// and properly handles successful conversion scenarios.
        /// </summary>
        [Fact]
        public void ConvertTo_NullConverterType_CallsInternalConvertToWithNullConverter()
        {
            // Arrange
            var value = "123";
            var toType = typeof(int);
            Type convertertype = null;
            var serviceProvider = Substitute.For<IServiceProvider>();

            // Act
            var result = value.ConvertTo(toType, convertertype, serviceProvider);

            // Assert
            Assert.Equal(123, result);
        }

        /// <summary>
        /// Tests ConvertTo method with null converter type when internal conversion throws an exception
        /// to verify proper exception rethrowing behavior.
        /// </summary>
        [Fact]
        public void ConvertTo_NullConverterType_ThrowsExceptionWhenInternalConversionFails()
        {
            // Arrange
            var value = "invalid";
            var toType = typeof(int);
            Type convertertype = null;
            var serviceProvider = Substitute.For<IServiceProvider>();

            // Act & Assert
            Assert.Throws<FormatException>(() => value.ConvertTo(toType, convertertype, serviceProvider));
        }

        /// <summary>
        /// Tests ConvertTo method with a valid converter type to ensure it creates the converter
        /// and follows the non-null converter path successfully.
        /// </summary>
        [Fact]
        public void ConvertTo_ValidConverterType_CallsInternalConvertToWithCreatedConverter()
        {
            // Arrange
            var value = "123";
            var toType = typeof(int);
            var convertertype = typeof(Int32Converter);
            var serviceProvider = Substitute.For<IServiceProvider>();

            // Act
            var result = value.ConvertTo(toType, convertertype, serviceProvider);

            // Assert
            Assert.Equal(123, result);
        }

        /// <summary>
        /// Tests ConvertTo method with a valid converter type when internal conversion throws an exception
        /// to verify proper exception rethrowing behavior in the non-null converter path.
        /// </summary>
        [Fact]
        public void ConvertTo_ValidConverterType_ThrowsExceptionWhenInternalConversionFails()
        {
            // Arrange
            var value = "invalid";
            var toType = typeof(int);
            var convertertype = typeof(Int32Converter);
            var serviceProvider = Substitute.For<IServiceProvider>();

            // Act & Assert
            Assert.Throws<FormatException>(() => value.ConvertTo(toType, convertertype, serviceProvider));
        }

        /// <summary>
        /// Tests ConvertTo method with an invalid converter type that cannot be instantiated
        /// to verify it throws an exception when the lambda function is invoked.
        /// </summary>
        [Fact]
        public void ConvertTo_InvalidConverterType_ThrowsExceptionWhenActivatorFails()
        {
            // Arrange
            var value = "123";
            var toType = typeof(int);
            var convertertype = typeof(AbstractTypeConverter); // Abstract class cannot be instantiated
            var serviceProvider = Substitute.For<IServiceProvider>();

            // Act & Assert
            Assert.Throws<MissingMethodException>(() => value.ConvertTo(toType, convertertype, serviceProvider));
        }

        /// <summary>
        /// Tests ConvertTo method with null value parameter to ensure it handles null input correctly.
        /// </summary>
        [Fact]
        public void ConvertTo_NullValue_ReturnsNull()
        {
            // Arrange
            object value = null;
            var toType = typeof(int);
            Type convertertype = null;
            var serviceProvider = Substitute.For<IServiceProvider>();

            // Act
            var result = value.ConvertTo(toType, convertertype, serviceProvider);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests ConvertTo method with null service provider to ensure it handles null service provider correctly.
        /// </summary>
        [Fact]
        public void ConvertTo_NullServiceProvider_WorksCorrectly()
        {
            // Arrange
            var value = "123";
            var toType = typeof(int);
            Type convertertype = null;
            IServiceProvider serviceProvider = null;

            // Act
            var result = value.ConvertTo(toType, convertertype, serviceProvider);

            // Assert
            Assert.Equal(123, result);
        }

        /// <summary>
        /// Tests ConvertTo method with both null converter type and null service provider
        /// to ensure it handles multiple null parameters correctly.
        /// </summary>
        [Fact]
        public void ConvertTo_NullConverterTypeAndNullServiceProvider_WorksCorrectly()
        {
            // Arrange
            var value = "true";
            var toType = typeof(bool);
            Type convertertype = null;
            IServiceProvider serviceProvider = null;

            // Act
            var result = value.ConvertTo(toType, convertertype, serviceProvider);

            // Assert
            Assert.Equal(true, result);
        }

        /// <summary>
        /// Tests ConvertTo method with a converter type that has no parameterless constructor
        /// to verify it handles constructor exceptions properly.
        /// </summary>
        [Fact]
        public void ConvertTo_ConverterTypeWithoutParameterlessConstructor_ThrowsException()
        {
            // Arrange
            var value = "123";
            var toType = typeof(int);
            var convertertype = typeof(ConverterWithoutParameterlessConstructor);
            var serviceProvider = Substitute.For<IServiceProvider>();

            // Act & Assert
            Assert.Throws<MissingMethodException>(() => value.ConvertTo(toType, convertertype, serviceProvider));
        }

        /// <summary>
        /// Abstract TypeConverter class used for testing invalid converter type scenarios.
        /// </summary>
        private abstract class AbstractTypeConverter : TypeConverter
        {
        }

        /// <summary>
        /// TypeConverter class without parameterless constructor for testing constructor exception scenarios.
        /// </summary>
        private class ConverterWithoutParameterlessConstructor : TypeConverter
        {
            public ConverterWithoutParameterlessConstructor(string parameter)
            {
            }
        }
    }
}
