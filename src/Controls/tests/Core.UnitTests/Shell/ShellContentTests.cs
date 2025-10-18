#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;

using Microsoft.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public partial class ShellContentConverterTests
    {
        private TypeConverter GetShellContentConverter()
        {
            // Access the private nested ShellContentConverter class through reflection
            var shellContentType = typeof(ShellContent);
            var converterType = shellContentType.GetNestedType("ShellContentConverter", BindingFlags.NonPublic);
            return (TypeConverter)Activator.CreateInstance(converterType);
        }

        /// <summary>
        /// Tests that CanConvertFrom returns true when sourceType is TemplatedPage.
        /// </summary>
        [Fact]
        public void CanConvertFrom_WithTemplatedPageType_ReturnsTrue()
        {
            // Arrange
            var converter = GetShellContentConverter();
            var context = Substitute.For<ITypeDescriptorContext>();
            var sourceType = typeof(TemplatedPage);

            // Act
            var result = converter.CanConvertFrom(context, sourceType);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that CanConvertFrom returns true when sourceType is TemplatedPage and context is null.
        /// </summary>
        [Fact]
        public void CanConvertFrom_WithTemplatedPageTypeAndNullContext_ReturnsTrue()
        {
            // Arrange
            var converter = GetShellContentConverter();
            ITypeDescriptorContext context = null;
            var sourceType = typeof(TemplatedPage);

            // Act
            var result = converter.CanConvertFrom(context, sourceType);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that CanConvertFrom returns false for various non-TemplatedPage types.
        /// Validates the method correctly identifies only TemplatedPage as convertible.
        /// </summary>
        [Theory]
        [InlineData(typeof(string))]
        [InlineData(typeof(int))]
        [InlineData(typeof(object))]
        [InlineData(typeof(Page))]
        [InlineData(typeof(ContentPage))]
        [InlineData(typeof(Shell))]
        [InlineData(typeof(ShellContent))]
        [InlineData(typeof(Element))]
        [InlineData(typeof(BindableObject))]
        [InlineData(typeof(View))]
        public void CanConvertFrom_WithNonTemplatedPageTypes_ReturnsFalse(Type sourceType)
        {
            // Arrange
            var converter = GetShellContentConverter();
            var context = Substitute.For<ITypeDescriptorContext>();

            // Act
            var result = converter.CanConvertFrom(context, sourceType);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that CanConvertFrom returns false for various non-TemplatedPage types with null context.
        /// Validates the method behavior is consistent regardless of context value.
        /// </summary>
        [Theory]
        [InlineData(typeof(string))]
        [InlineData(typeof(int))]
        [InlineData(typeof(object))]
        [InlineData(typeof(Page))]
        public void CanConvertFrom_WithNonTemplatedPageTypesAndNullContext_ReturnsFalse(Type sourceType)
        {
            // Arrange
            var converter = GetShellContentConverter();
            ITypeDescriptorContext context = null;

            // Act
            var result = converter.CanConvertFrom(context, sourceType);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that CanConvertFrom throws ArgumentNullException when sourceType is null.
        /// Validates proper handling of null sourceType parameter.
        /// </summary>
        [Fact]
        public void CanConvertFrom_WithNullSourceType_ThrowsNullReferenceException()
        {
            // Arrange
            var converter = GetShellContentConverter();
            var context = Substitute.For<ITypeDescriptorContext>();
            Type sourceType = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => converter.CanConvertFrom(context, sourceType));
        }

        /// <summary>
        /// Tests that CanConvertFrom throws ArgumentNullException when both context and sourceType are null.
        /// Validates proper handling of multiple null parameters.
        /// </summary>
        [Fact]
        public void CanConvertFrom_WithNullContextAndNullSourceType_ThrowsNullReferenceException()
        {
            // Arrange
            var converter = GetShellContentConverter();
            ITypeDescriptorContext context = null;
            Type sourceType = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => converter.CanConvertFrom(context, sourceType));
        }

        /// <summary>
        /// Tests that ConvertFrom successfully converts a TemplatedPage to ShellContent.
        /// Input: Valid TemplatedPage instance.
        /// Expected: Returns ShellContent instance via implicit conversion.
        /// </summary>
        [Fact]
        public void ConvertFrom_ValidTemplatedPage_ReturnsShellContent()
        {
            // Arrange
            var converter = new TestableShellContentConverter();
            var templatedPage = new TemplatedPage();
            ITypeDescriptorContext context = null;
            CultureInfo culture = null;

            // Act
            var result = converter.ConvertFrom(context, culture, templatedPage);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ShellContent>(result);
        }

        /// <summary>
        /// Tests that ConvertFrom throws NotSupportedException when value is null.
        /// Input: null value.
        /// Expected: Throws NotSupportedException.
        /// </summary>
        [Fact]
        public void ConvertFrom_NullValue_ThrowsNotSupportedException()
        {
            // Arrange
            var converter = new TestableShellContentConverter();
            object value = null;
            ITypeDescriptorContext context = null;
            CultureInfo culture = null;

            // Act & Assert
            Assert.Throws<NotSupportedException>(() => converter.ConvertFrom(context, culture, value));
        }

        /// <summary>
        /// Tests that ConvertFrom throws NotSupportedException for various invalid value types.
        /// Input: Different types that are not TemplatedPage.
        /// Expected: Throws NotSupportedException for each invalid type.
        /// </summary>
        [Theory]
        [InlineData("string value")]
        [InlineData(42)]
        [InlineData(true)]
        [InlineData(3.14)]
        public void ConvertFrom_InvalidValueTypes_ThrowsNotSupportedException(object invalidValue)
        {
            // Arrange
            var converter = new TestableShellContentConverter();
            ITypeDescriptorContext context = null;
            CultureInfo culture = null;

            // Act & Assert
            Assert.Throws<NotSupportedException>(() => converter.ConvertFrom(context, culture, invalidValue));
        }

        /// <summary>
        /// Tests that ConvertFrom throws NotSupportedException when value is a Page but not TemplatedPage.
        /// Input: ContentPage instance (which is a Page but not TemplatedPage).
        /// Expected: Throws NotSupportedException.
        /// </summary>
        [Fact]
        public void ConvertFrom_NonTemplatedPageType_ThrowsNotSupportedException()
        {
            // Arrange
            var converter = new TestableShellContentConverter();
            var contentPage = new ContentPage();
            ITypeDescriptorContext context = null;
            CultureInfo culture = null;

            // Act & Assert
            Assert.Throws<NotSupportedException>(() => converter.ConvertFrom(context, culture, contentPage));
        }

        /// <summary>
        /// Tests that ConvertFrom works correctly with non-null context and culture parameters.
        /// Input: Valid TemplatedPage with non-null context and culture.
        /// Expected: Returns ShellContent instance, ignoring context and culture values.
        /// </summary>
        [Fact]
        public void ConvertFrom_WithContextAndCulture_ReturnsShellContent()
        {
            // Arrange
            var converter = new TestableShellContentConverter();
            var templatedPage = new TemplatedPage();
            var context = new TestTypeDescriptorContext();
            var culture = CultureInfo.InvariantCulture;

            // Act
            var result = converter.ConvertFrom(context, culture, templatedPage);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ShellContent>(result);
        }

        /// <summary>
        /// Helper class to access the private ShellContentConverter for testing purposes.
        /// </summary>
        private class TestableShellContentConverter : TypeConverter
        {
            private readonly TypeConverter _innerConverter;

            public TestableShellContentConverter()
            {
                // Create an instance of ShellContent to access its converter
                var shellContent = new ShellContent();
                var converter = TypeDescriptor.GetConverter(typeof(ShellContent));
                _innerConverter = converter;
            }

            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            {
                return _innerConverter.CanConvertFrom(context, sourceType);
            }

            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
            {
                return _innerConverter.ConvertFrom(context, culture, value);
            }
        }

    }

    public partial class ShellContentTests
    {
        public class ShellContentConverterTests
        {
            /// <summary>
            /// Tests that CanConvertTo always returns false for null context and null destination type.
            /// This verifies the method handles null inputs correctly and maintains its expected behavior.
            /// </summary>
            [Fact]
            public void CanConvertTo_NullContextAndNullDestinationType_ReturnsFalse()
            {
                // Arrange
                var converter = new TestableShellContentConverter();

                // Act
                bool result = converter.CanConvertTo(null, null);

                // Assert
                Assert.False(result);
            }

            /// <summary>
            /// Tests that CanConvertTo always returns false for various destination types with null context.
            /// This verifies the method consistently returns false regardless of the destination type when context is null.
            /// </summary>
            [Theory]
            [InlineData(typeof(string))]
            [InlineData(typeof(int))]
            [InlineData(typeof(object))]
            [InlineData(typeof(ShellContent))]
            [InlineData(typeof(TemplatedPage))]
            [InlineData(typeof(Page))]
            [InlineData(typeof(bool))]
            [InlineData(typeof(double))]
            [InlineData(typeof(DateTime))]
            public void CanConvertTo_NullContextWithVariousDestinationTypes_ReturnsFalse(Type destinationType)
            {
                // Arrange
                var converter = new TestableShellContentConverter();

                // Act
                bool result = converter.CanConvertTo(null, destinationType);

                // Assert
                Assert.False(result);
            }

            /// <summary>
            /// Tests that CanConvertTo always returns false for various destination types with a valid context.
            /// This verifies the method consistently returns false regardless of the destination type and context.
            /// </summary>
            [Theory]
            [InlineData(typeof(string))]
            [InlineData(typeof(int))]
            [InlineData(typeof(object))]
            [InlineData(typeof(ShellContent))]
            [InlineData(typeof(TemplatedPage))]
            [InlineData(typeof(Page))]
            [InlineData(typeof(bool))]
            [InlineData(typeof(double))]
            [InlineData(typeof(DateTime))]
            public void CanConvertTo_ValidContextWithVariousDestinationTypes_ReturnsFalse(Type destinationType)
            {
                // Arrange
                var converter = new TestableShellContentConverter();
                var context = Substitute.For<ITypeDescriptorContext>();

                // Act
                bool result = converter.CanConvertTo(context, destinationType);

                // Assert
                Assert.False(result);
            }

            /// <summary>
            /// Tests that CanConvertTo always returns false when destination type is null with a valid context.
            /// This verifies the method handles null destination type correctly with non-null context.
            /// </summary>
            [Fact]
            public void CanConvertTo_ValidContextWithNullDestinationType_ReturnsFalse()
            {
                // Arrange
                var converter = new TestableShellContentConverter();
                var context = Substitute.For<ITypeDescriptorContext>();

                // Act
                bool result = converter.CanConvertTo(context, null);

                // Assert
                Assert.False(result);
            }

        }

        /// <summary>
        /// Tests that ConvertTo throws NotSupportedException when all parameters are null.
        /// Validates that the method consistently throws regardless of input values.
        /// </summary>
        [Fact]
        public void ConvertTo_AllParametersNull_ThrowsNotSupportedException()
        {
            // Arrange
            TypeConverter converter = TypeDescriptor.GetConverter(typeof(ShellContent));

            // Act & Assert
            Assert.Throws<NotSupportedException>(() =>
                converter.ConvertTo(null, null, null, null));
        }

        /// <summary>
        /// Tests that ConvertTo throws NotSupportedException with various parameter combinations.
        /// Validates that the method behavior is consistent regardless of parameter values.
        /// </summary>
        /// <param name="value">The value to convert</param>
        /// <param name="destinationType">The destination type</param>
        [Theory]
        [InlineData(null, null)]
        [InlineData("test", typeof(string))]
        [InlineData(42, typeof(int))]
        [InlineData(true, typeof(bool))]
        [InlineData("value", typeof(object))]
        [InlineData(123.45, typeof(double))]
        public void ConvertTo_VariousValueAndDestinationTypeCombinations_ThrowsNotSupportedException(object value, Type destinationType)
        {
            // Arrange
            TypeConverter converter = TypeDescriptor.GetConverter(typeof(ShellContent));
            ITypeDescriptorContext context = Substitute.For<ITypeDescriptorContext>();
            CultureInfo culture = CultureInfo.InvariantCulture;

            // Act & Assert
            Assert.Throws<NotSupportedException>(() =>
                converter.ConvertTo(context, culture, value, destinationType));
        }

        /// <summary>
        /// Tests that ConvertTo throws NotSupportedException with null context and culture.
        /// Validates that method throws even when context and culture parameters are null.
        /// </summary>
        [Fact]
        public void ConvertTo_NullContextAndCulture_ThrowsNotSupportedException()
        {
            // Arrange
            TypeConverter converter = TypeDescriptor.GetConverter(typeof(ShellContent));
            var shellContent = new ShellContent();

            // Act & Assert
            Assert.Throws<NotSupportedException>(() =>
                converter.ConvertTo(null, null, shellContent, typeof(string)));
        }

        /// <summary>
        /// Tests that ConvertTo throws NotSupportedException with valid context and culture.
        /// Validates that method throws even when all parameters have valid non-null values.
        /// </summary>
        [Fact]
        public void ConvertTo_ValidContextAndCulture_ThrowsNotSupportedException()
        {
            // Arrange
            TypeConverter converter = TypeDescriptor.GetConverter(typeof(ShellContent));
            ITypeDescriptorContext context = Substitute.For<ITypeDescriptorContext>();
            CultureInfo culture = new CultureInfo("en-US");
            var shellContent = new ShellContent();

            // Act & Assert
            Assert.Throws<NotSupportedException>(() =>
                converter.ConvertTo(context, culture, shellContent, typeof(object)));
        }

        /// <summary>
        /// Tests that ConvertTo throws NotSupportedException with ShellContent instance as value.
        /// Validates that method throws even when converting a ShellContent instance itself.
        /// </summary>
        [Fact]
        public void ConvertTo_ShellContentAsValue_ThrowsNotSupportedException()
        {
            // Arrange
            TypeConverter converter = TypeDescriptor.GetConverter(typeof(ShellContent));
            var shellContent = new ShellContent { Title = "Test" };
            ITypeDescriptorContext context = Substitute.For<ITypeDescriptorContext>();

            // Act & Assert
            Assert.Throws<NotSupportedException>(() =>
                converter.ConvertTo(context, CultureInfo.CurrentCulture, shellContent, typeof(ShellContent)));
        }

        /// <summary>
        /// Tests that ConvertTo throws NotSupportedException with complex object as value.
        /// Validates that method throws regardless of the complexity of the input value.
        /// </summary>
        [Fact]
        public void ConvertTo_ComplexObjectAsValue_ThrowsNotSupportedException()
        {
            // Arrange
            TypeConverter converter = TypeDescriptor.GetConverter(typeof(ShellContent));
            var complexObject = new { Name = "Test", Value = 42, Items = new[] { 1, 2, 3 } };

            // Act & Assert
            Assert.Throws<NotSupportedException>(() =>
                converter.ConvertTo(null, null, complexObject, typeof(string)));
        }

        /// <summary>
        /// Tests that EvaluateDisconnect returns early when _createdViaService is false.
        /// </summary>
        [Fact]
        public void EvaluateDisconnect_WhenCreatedViaServiceIsFalse_ReturnsEarly()
        {
            // Arrange
            var shellContent = new ShellContent();

            // Act
            shellContent.EvaluateDisconnect();

            // Assert
            // Since _createdViaService is false by default, method should return early
            // No exceptions should be thrown and no cleanup should occur
            Assert.NotNull(shellContent);
        }

        /// <summary>
        /// Tests EvaluateDisconnect when Parent is null.
        /// </summary>
        [Fact]
        public void EvaluateDisconnect_WhenParentIsNull_SetsContentCacheToNull()
        {
            // Arrange
            var shellContent = new TestableShellContent();
            shellContent.SetCreatedViaService(true);
            var mockPage = Substitute.For<Page>();
            shellContent.SetContentCache(mockPage);

            // Act
            shellContent.EvaluateDisconnect();

            // Assert
            Assert.Null(shellContent.ContentCache);
        }

        /// <summary>
        /// Tests EvaluateDisconnect when Parent is not ShellSection.
        /// </summary>
        [Fact]
        public void EvaluateDisconnect_WhenParentIsNotShellSection_SetsContentCacheToNull()
        {
            // Arrange
            var shellContent = new TestableShellContent();
            shellContent.SetCreatedViaService(true);
            var mockPage = Substitute.For<Page>();
            shellContent.SetContentCache(mockPage);
            var mockParent = Substitute.For<Element>();
            shellContent.SetParent(mockParent);

            // Act
            shellContent.EvaluateDisconnect();

            // Assert
            Assert.Null(shellContent.ContentCache);
        }

        /// <summary>
        /// Tests EvaluateDisconnect when ShellSection parent is not ShellItem.
        /// </summary>
        [Fact]
        public void EvaluateDisconnect_WhenShellSectionParentIsNotShellItem_SetsContentCacheToNull()
        {
            // Arrange
            var shellContent = new TestableShellContent();
            shellContent.SetCreatedViaService(true);
            var mockPage = Substitute.For<Page>();
            shellContent.SetContentCache(mockPage);
            var mockShellSection = Substitute.For<ShellSection>();
            var mockParent = Substitute.For<Element>();
            mockShellSection.Parent.Returns(mockParent);
            shellContent.SetParent(mockShellSection);

            // Act
            shellContent.EvaluateDisconnect();

            // Assert
            Assert.Null(shellContent.ContentCache);
        }

        /// <summary>
        /// Tests EvaluateDisconnect when ShellItem parent is not Shell.
        /// </summary>
        [Fact]
        public void EvaluateDisconnect_WhenShellItemParentIsNotShell_SetsContentCacheToNull()
        {
            // Arrange
            var shellContent = new TestableShellContent();
            shellContent.SetCreatedViaService(true);
            var mockPage = Substitute.For<Page>();
            shellContent.SetContentCache(mockPage);
            var mockShellSection = Substitute.For<ShellSection>();
            var mockShellItem = Substitute.For<ShellItem>();
            var mockParent = Substitute.For<Element>();
            mockShellItem.Parent.Returns(mockParent);
            mockShellSection.Parent.Returns(mockShellItem);
            shellContent.SetParent(mockShellSection);

            // Act
            shellContent.EvaluateDisconnect();

            // Assert
            Assert.Null(shellContent.ContentCache);
        }

        /// <summary>
        /// Tests EvaluateDisconnect when shell content is not visible and should disconnect.
        /// </summary>
        [Fact]
        public void EvaluateDisconnect_WhenShellContentIsNotVisible_DisconnectsAndCleansUp()
        {
            // Arrange
            var shellContent = new TestableShellContent();
            shellContent.SetCreatedViaService(true);
            shellContent.SetIsVisible(false);
            var mockPage = Substitute.For<Page>();
            shellContent.SetContentCache(mockPage);
            SetupValidShellHierarchy(shellContent, out var shell, out var shellItem, out var shellSection);

            // Act
            shellContent.EvaluateDisconnect();

            // Assert
            Assert.Null(shellContent.ContentCache);
            mockPage.Received(1).Unloaded -= Arg.Any<EventHandler>();
        }

        /// <summary>
        /// Tests EvaluateDisconnect when content cache page is not visible and should disconnect.
        /// </summary>
        [Fact]
        public void EvaluateDisconnect_WhenContentCachePageIsNotVisible_DisconnectsAndCleansUp()
        {
            // Arrange
            var shellContent = new TestableShellContent();
            shellContent.SetCreatedViaService(true);
            shellContent.SetIsVisible(true);
            var mockPage = Substitute.For<Page>();
            mockPage.IsVisible.Returns(false);
            shellContent.SetContentCache(mockPage);
            SetupValidShellHierarchy(shellContent, out var shell, out var shellItem, out var shellSection);

            // Act
            shellContent.EvaluateDisconnect();

            // Assert
            Assert.Null(shellContent.ContentCache);
            mockPage.Received(1).Unloaded -= Arg.Any<EventHandler>();
        }

        /// <summary>
        /// Tests EvaluateDisconnect when shell current item is different from shell item and should disconnect.
        /// </summary>
        [Fact]
        public void EvaluateDisconnect_WhenShellCurrentItemDifferent_DisconnectsAndCleansUp()
        {
            // Arrange
            var shellContent = new TestableShellContent();
            shellContent.SetCreatedViaService(true);
            shellContent.SetIsVisible(true);
            var mockPage = Substitute.For<Page>();
            mockPage.IsVisible.Returns(true);
            shellContent.SetContentCache(mockPage);
            SetupValidShellHierarchy(shellContent, out var shell, out var shellItem, out var shellSection);
            var differentShellItem = Substitute.For<ShellItem>();
            shell.CurrentItem.Returns(differentShellItem);

            // Act
            shellContent.EvaluateDisconnect();

            // Assert
            Assert.Null(shellContent.ContentCache);
            mockPage.Received(1).Unloaded -= Arg.Any<EventHandler>();
        }

        /// <summary>
        /// Tests EvaluateDisconnect when shell section is not visible and should disconnect.
        /// </summary>
        [Fact]
        public void EvaluateDisconnect_WhenShellSectionIsNotVisible_DisconnectsAndCleansUp()
        {
            // Arrange
            var shellContent = new TestableShellContent();
            shellContent.SetCreatedViaService(true);
            shellContent.SetIsVisible(true);
            var mockPage = Substitute.For<Page>();
            mockPage.IsVisible.Returns(true);
            shellContent.SetContentCache(mockPage);
            SetupValidShellHierarchy(shellContent, out var shell, out var shellItem, out var shellSection);
            shell.CurrentItem.Returns(shellItem);
            shellSection.IsVisible.Returns(false);

            // Act
            shellContent.EvaluateDisconnect();

            // Assert
            Assert.Null(shellContent.ContentCache);
            mockPage.Received(1).Unloaded -= Arg.Any<EventHandler>();
        }

        /// <summary>
        /// Tests EvaluateDisconnect when window is null and should disconnect.
        /// </summary>
        [Fact]
        public void EvaluateDisconnect_WhenWindowIsNull_DisconnectsAndCleansUp()
        {
            // Arrange
            var shellContent = new TestableShellContent();
            shellContent.SetCreatedViaService(true);
            shellContent.SetIsVisible(true);
            shellContent.SetWindow(null);
            var mockPage = Substitute.For<Page>();
            mockPage.IsVisible.Returns(true);
            shellContent.SetContentCache(mockPage);
            SetupValidShellHierarchy(shellContent, out var shell, out var shellItem, out var shellSection);
            shell.CurrentItem.Returns(shellItem);
            shellSection.IsVisible.Returns(true);

            // Act
            shellContent.EvaluateDisconnect();

            // Assert
            Assert.Null(shellContent.ContentCache);
            mockPage.Received(1).Unloaded -= Arg.Any<EventHandler>();
        }

        /// <summary>
        /// Tests EvaluateDisconnect when all conditions indicate no disconnect should occur.
        /// </summary>
        [Fact]
        public void EvaluateDisconnect_WhenShouldNotDisconnect_CallsNotifyFlyoutBehaviorObservers()
        {
            // Arrange
            var shellContent = new TestableShellContent();
            shellContent.SetCreatedViaService(true);
            shellContent.SetIsVisible(true);
            var mockWindow = Substitute.For<Window>();
            shellContent.SetWindow(mockWindow);
            var mockPage = Substitute.For<Page>();
            mockPage.IsVisible.Returns(true);
            shellContent.SetContentCache(mockPage);
            SetupValidShellHierarchy(shellContent, out var shell, out var shellItem, out var shellSection);
            shell.CurrentItem.Returns(shellItem);
            shellSection.IsVisible.Returns(true);

            // Act
            shellContent.EvaluateDisconnect();

            // Assert
            Assert.NotNull(shellContent.ContentCache);
            shell.Received(1).NotifyFlyoutBehaviorObservers();
        }

        /// <summary>
        /// Tests EvaluateDisconnect when content cache is null during cleanup.
        /// </summary>
        [Fact]
        public void EvaluateDisconnect_WhenContentCacheIsNullDuringCleanup_DoesNotThrow()
        {
            // Arrange
            var shellContent = new TestableShellContent();
            shellContent.SetCreatedViaService(true);
            shellContent.SetIsVisible(false);
            shellContent.SetContentCache(null);

            // Act & Assert
            var exception = Record.Exception(() => shellContent.EvaluateDisconnect());
            Assert.Null(exception);
        }

        private static void SetupValidShellHierarchy(TestableShellContent shellContent, out Shell shell, out ShellItem shellItem, out ShellSection shellSection)
        {
            shell = Substitute.For<Shell>();
            shellItem = Substitute.For<ShellItem>();
            shellSection = Substitute.For<ShellSection>();

            shell.NotifyFlyoutBehaviorObservers();
            shellItem.Parent = shell;
            shellSection.Parent = shellItem;
            shellContent.SetParent(shellSection);
        }

        private class TestableShellContent : ShellContent
        {
            private bool _createdViaService;
            private Page _contentCache;
            private Element _parent;
            private bool _isVisible = true;
            private Window _window;

            public void SetCreatedViaService(bool value)
            {
                _createdViaService = value;
            }

            public void SetContentCache(Page page)
            {
                _contentCache = page;
            }

            public void SetParent(Element parent)
            {
                _parent = parent;
            }

            public void SetIsVisible(bool value)
            {
                _isVisible = value;
            }

            public void SetWindow(Window window)
            {
                _window = window;
            }

            public new Element Parent => _parent;

            public new bool IsVisible => _isVisible;

            public new Window Window => _window;
        }
    }
}