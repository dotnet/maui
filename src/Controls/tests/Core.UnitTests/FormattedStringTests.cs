#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;


using Microsoft.Maui;
using Microsoft.Maui.Controls;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

    public class FormattedStringTests : BaseTestFixture
    {
        [Fact]
        public void NullSpansNotAllowed()
        {
            var fs = new FormattedString();
            Assert.Throws<ArgumentNullException>(() => fs.Spans.Add(null));

            fs = new FormattedString();
            fs.Spans.Add(new Span());

            Assert.Throws<ArgumentNullException>(() =>
            {
                fs.Spans[0] = null;
            });
        }

        [Fact]
        public void SpanChangeTriggersSpansPropertyChange()
        {
            var span = new Span();
            var fs = new FormattedString();
            fs.Spans.Add(span);

            bool spansChanged = false;
            fs.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "Spans")
                    spansChanged = true;
            };

            span.Text = "New text";

            Assert.True(spansChanged);
        }

        [Fact]
        public void SpanChangesUnsubscribes()
        {
            var span = new Span();
            var fs = new FormattedString();
            fs.Spans.Add(span);
            fs.Spans.Remove(span);

            bool spansChanged = false;
            fs.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "Spans")
                    spansChanged = true;
            };

            span.Text = "New text";

            Assert.False(spansChanged);
        }

        [Fact]
        public void AddingSpanTriggersSpansPropertyChange()
        {
            var span = new Span();
            var fs = new FormattedString();

            bool spansChanged = false;
            fs.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "Spans")
                    spansChanged = true;
            };

            fs.Spans.Add(span);

            Assert.True(spansChanged);
        }

        [Fact]
        public void ImplicitStringConversion()
        {
            string original = "fubar";
            FormattedString fs = original;
            Assert.NotNull(fs);
            Assert.Single(fs.Spans);
            Assert.NotNull(fs.Spans[0]);
            Assert.Equal(fs.Spans[0].Text, original);
        }

        [Fact]
        public void ImplicitStringConversionNull()
        {
            string original = null;
            FormattedString fs = original;
            Assert.NotNull(fs);
            Assert.Single(fs.Spans);
            Assert.NotNull(fs.Spans[0]);
            Assert.Equal(fs.Spans[0].Text, original);
        }

        /// <summary>
        /// Tests that ClearItems method removes all items from empty collection and handles event correctly.
        /// Verifies the collection remains empty and appropriate collection changed event behavior.
        /// </summary>
        [Fact]
        public void ClearItems_EmptyCollection_CollectionRemainsEmpty()
        {
            // Arrange
            var formattedString = new FormattedString();
            var eventFired = false;
            NotifyCollectionChangedEventArgs eventArgs = null;

            formattedString.Spans.CollectionChanged += (sender, e) =>
            {
                eventFired = true;
                eventArgs = e;
            };

            // Act
            formattedString.Spans.Clear();

            // Assert
            Assert.Empty(formattedString.Spans);
            Assert.False(eventFired); // No event should fire for clearing empty collection
        }

        /// <summary>
        /// Tests that ClearItems method removes single item from collection and fires correct collection changed event.
        /// Verifies the collection becomes empty and event contains the removed item with Remove action.
        /// </summary>
        [Fact]
        public void ClearItems_SingleItem_RemovesItemAndFiresEvent()
        {
            // Arrange
            var formattedString = new FormattedString();
            var span = new Span { Text = "Test" };
            formattedString.Spans.Add(span);

            var eventFired = false;
            NotifyCollectionChangedEventArgs eventArgs = null;

            formattedString.Spans.CollectionChanged += (sender, e) =>
            {
                eventFired = true;
                eventArgs = e;
            };

            // Act
            formattedString.Spans.Clear();

            // Assert
            Assert.Empty(formattedString.Spans);
            Assert.True(eventFired);
            Assert.NotNull(eventArgs);
            Assert.Equal(NotifyCollectionChangedAction.Remove, eventArgs.Action);
            Assert.NotNull(eventArgs.OldItems);
            Assert.Single(eventArgs.OldItems);
            Assert.Equal(span, eventArgs.OldItems[0]);
        }

        /// <summary>
        /// Tests that ClearItems method removes multiple items from collection and fires correct collection changed event.
        /// Verifies the collection becomes empty and event contains all removed items with Remove action.
        /// </summary>
        [Fact]
        public void ClearItems_MultipleItems_RemovesAllItemsAndFiresEvent()
        {
            // Arrange
            var formattedString = new FormattedString();
            var span1 = new Span { Text = "First" };
            var span2 = new Span { Text = "Second" };
            var span3 = new Span { Text = "Third" };

            formattedString.Spans.Add(span1);
            formattedString.Spans.Add(span2);
            formattedString.Spans.Add(span3);

            var eventFired = false;
            NotifyCollectionChangedEventArgs eventArgs = null;

            formattedString.Spans.CollectionChanged += (sender, e) =>
            {
                eventFired = true;
                eventArgs = e;
            };

            // Act
            formattedString.Spans.Clear();

            // Assert
            Assert.Empty(formattedString.Spans);
            Assert.True(eventFired);
            Assert.NotNull(eventArgs);
            Assert.Equal(NotifyCollectionChangedAction.Remove, eventArgs.Action);
            Assert.NotNull(eventArgs.OldItems);
            Assert.Equal(3, eventArgs.OldItems.Count);
            Assert.Contains(span1, eventArgs.OldItems.Cast<Span>());
            Assert.Contains(span2, eventArgs.OldItems.Cast<Span>());
            Assert.Contains(span3, eventArgs.OldItems.Cast<Span>());
        }

        /// <summary>
        /// Tests that ClearItems method preserves the order of items in the collection changed event.
        /// Verifies that removed items in the event maintain their original collection order.
        /// </summary>
        [Fact]
        public void ClearItems_MultipleItems_PreservesOrderInEvent()
        {
            // Arrange
            var formattedString = new FormattedString();
            var spans = new List<Span>
            {
                new Span { Text = "First" },
                new Span { Text = "Second" },
                new Span { Text = "Third" },
                new Span { Text = "Fourth" }
            };

            foreach (var span in spans)
            {
                formattedString.Spans.Add(span);
            }

            NotifyCollectionChangedEventArgs eventArgs = null;
            formattedString.Spans.CollectionChanged += (sender, e) => eventArgs = e;

            // Act
            formattedString.Spans.Clear();

            // Assert
            Assert.NotNull(eventArgs);
            Assert.Equal(NotifyCollectionChangedAction.Remove, eventArgs.Action);
            Assert.Equal(spans.Count, eventArgs.OldItems.Count);

            for (int i = 0; i < spans.Count; i++)
            {
                Assert.Equal(spans[i], eventArgs.OldItems[i]);
            }
        }

        /// <summary>
        /// Tests that the FormattedString constructor creates an instance with an empty Spans collection.
        /// Input conditions: No parameters (parameterless constructor).
        /// Expected result: FormattedString instance created with empty Spans collection.
        /// </summary>
        [Fact]
        public void Constructor_CreatesInstance_WithEmptySpansCollection()
        {
            // Act
            var formattedString = new FormattedString();

            // Assert
            Assert.NotNull(formattedString);
            Assert.NotNull(formattedString.Spans);
            Assert.Empty(formattedString.Spans);
            Assert.IsAssignableFrom<Element>(formattedString);
        }

        /// <summary>
        /// Tests that the FormattedString constructor properly subscribes to collection changed events.
        /// Input conditions: Constructor called, then span added to collection.
        /// Expected result: Collection changed events are triggered and handled properly.
        /// </summary>
        [Fact]
        public void Constructor_SubscribesToCollectionChangedEvent_PropertyChangedTriggered()
        {
            // Arrange
            var formattedString = new FormattedString();
            bool propertyChanged = false;
            formattedString.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(FormattedString.Spans))
                    propertyChanged = true;
            };

            // Act
            formattedString.Spans.Add(new Span { Text = "Test" });

            // Assert
            Assert.True(propertyChanged, "PropertyChanged event should be triggered when spans are added");
            Assert.Single(formattedString.Spans);
        }

        /// <summary>
        /// Tests that the FormattedString constructor creates spans collection that supports collection operations.
        /// Input conditions: Constructor called, multiple spans added and removed.
        /// Expected result: Collection operations work correctly and events are triggered.
        /// </summary>
        [Fact]
        public void Constructor_CreatesSpansCollection_SupportsCollectionOperations()
        {
            // Arrange
            var formattedString = new FormattedString();
            var span1 = new Span { Text = "First" };
            var span2 = new Span { Text = "Second" };

            // Act & Assert - Add operations
            formattedString.Spans.Add(span1);
            Assert.Single(formattedString.Spans);
            Assert.Equal(span1, formattedString.Spans[0]);

            formattedString.Spans.Add(span2);
            Assert.Equal(2, formattedString.Spans.Count);

            // Act & Assert - Remove operations  
            formattedString.Spans.Remove(span1);
            Assert.Single(formattedString.Spans);
            Assert.Equal(span2, formattedString.Spans[0]);

            formattedString.Spans.Clear();
            Assert.Empty(formattedString.Spans);
        }
    }


    public class FormattedStringConverterTests : BaseTestFixture
    {
        private TypeConverter GetFormattedStringConverter()
        {
            return TypeDescriptor.GetConverter(typeof(FormattedString));
        }

        /// <summary>
        /// Tests that CanConvertFrom returns true when sourceType is typeof(string).
        /// Verifies the primary positive case for string type conversion.
        /// </summary>
        [Fact]
        public void CanConvertFrom_StringType_ReturnsTrue()
        {
            // Arrange
            var converter = GetFormattedStringConverter();

            // Act
            var result = converter.CanConvertFrom(null, typeof(string));

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that CanConvertFrom returns false for various non-string types.
        /// Verifies that only string type conversion is supported.
        /// </summary>
        [Theory]
        [InlineData(typeof(int))]
        [InlineData(typeof(double))]
        [InlineData(typeof(bool))]
        [InlineData(typeof(char))]
        [InlineData(typeof(object))]
        [InlineData(typeof(FormattedString))]
        [InlineData(typeof(Span))]
        [InlineData(typeof(DateTime))]
        [InlineData(typeof(decimal))]
        [InlineData(typeof(float))]
        [InlineData(typeof(long))]
        [InlineData(typeof(byte))]
        [InlineData(typeof(short))]
        [InlineData(typeof(uint))]
        [InlineData(typeof(ulong))]
        [InlineData(typeof(ushort))]
        [InlineData(typeof(sbyte))]
        [InlineData(typeof(Guid))]
        [InlineData(typeof(TimeSpan))]
        [InlineData(typeof(IEnumerable))]
        [InlineData(typeof(IList))]
        [InlineData(typeof(Array))]
        [InlineData(typeof(Exception))]
        [InlineData(typeof(Type))]
        [InlineData(typeof(ValueType))]
        public void CanConvertFrom_NonStringTypes_ReturnsFalse(Type sourceType)
        {
            // Arrange
            var converter = GetFormattedStringConverter();

            // Act
            var result = converter.CanConvertFrom(null, sourceType);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that CanConvertFrom throws an exception when sourceType is null.
        /// Verifies proper null parameter handling.
        /// </summary>
        [Fact]
        public void CanConvertFrom_NullSourceType_ThrowsException()
        {
            // Arrange
            var converter = GetFormattedStringConverter();

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => converter.CanConvertFrom(null, null));
        }

        /// <summary>
        /// Tests that CanConvertFrom works correctly with a valid ITypeDescriptorContext.
        /// Verifies that context parameter doesn't affect the result when provided.
        /// </summary>
        [Fact]
        public void CanConvertFrom_ValidContext_ReturnsExpectedResult()
        {
            // Arrange
            var converter = GetFormattedStringConverter();
            var context = NSubstitute.Substitute.For<ITypeDescriptorContext>();

            // Act
            var resultForString = converter.CanConvertFrom(context, typeof(string));
            var resultForInt = converter.CanConvertFrom(context, typeof(int));

            // Assert
            Assert.True(resultForString);
            Assert.False(resultForInt);
        }

        /// <summary>
        /// Tests that CanConvertFrom returns false for generic types.
        /// Verifies behavior with complex generic type scenarios.
        /// </summary>
        [Theory]
        [InlineData(typeof(List<string>))]
        [InlineData(typeof(Dictionary<string, object>))]
        [InlineData(typeof(IEnumerable<int>))]
        [InlineData(typeof(ObservableCollection<Span>))]
        [InlineData(typeof(Nullable<int>))]
        [InlineData(typeof(Func<string, bool>))]
        [InlineData(typeof(Action<string>))]
        public void CanConvertFrom_GenericTypes_ReturnsFalse(Type sourceType)
        {
            // Arrange
            var converter = GetFormattedStringConverter();

            // Act
            var result = converter.CanConvertFrom(null, sourceType);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that CanConvertFrom returns false for array types.
        /// Verifies behavior with various array type scenarios.
        /// </summary>
        [Theory]
        [InlineData(typeof(string[]))]
        [InlineData(typeof(int[]))]
        [InlineData(typeof(object[]))]
        [InlineData(typeof(byte[]))]
        [InlineData(typeof(char[]))]
        public void CanConvertFrom_ArrayTypes_ReturnsFalse(Type sourceType)
        {
            // Arrange
            var converter = GetFormattedStringConverter();

            // Act
            var result = converter.CanConvertFrom(null, sourceType);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that CanConvertTo returns true when the destination type is string.
        /// Verifies the converter correctly identifies string as a supported conversion target.
        /// </summary>
        [Fact]
        public void CanConvertTo_StringDestinationType_ReturnsTrue()
        {
            // Arrange
            var converter = TypeDescriptor.GetConverter(typeof(FormattedString));
            var destinationType = typeof(string);
            ITypeDescriptorContext context = null;

            // Act
            var result = converter.CanConvertTo(context, destinationType);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that CanConvertTo returns false for various non-string destination types.
        /// Verifies the converter only supports conversion to string type.
        /// </summary>
        /// <param name="destinationType">The destination type to test conversion support for.</param>
        [Theory]
        [InlineData(typeof(int))]
        [InlineData(typeof(object))]
        [InlineData(typeof(FormattedString))]
        [InlineData(typeof(bool))]
        [InlineData(typeof(DateTime))]
        [InlineData(typeof(double))]
        [InlineData(typeof(float))]
        [InlineData(typeof(decimal))]
        [InlineData(typeof(long))]
        [InlineData(typeof(char))]
        public void CanConvertTo_NonStringDestinationType_ReturnsFalse(Type destinationType)
        {
            // Arrange
            var converter = TypeDescriptor.GetConverter(typeof(FormattedString));
            ITypeDescriptorContext context = null;

            // Act
            var result = converter.CanConvertTo(context, destinationType);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that CanConvertTo throws NullReferenceException when destination type is null.
        /// Verifies the method does not handle null destination type gracefully.
        /// </summary>
        [Fact]
        public void CanConvertTo_NullDestinationType_ThrowsNullReferenceException()
        {
            // Arrange
            var converter = TypeDescriptor.GetConverter(typeof(FormattedString));
            Type destinationType = null;
            ITypeDescriptorContext context = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => converter.CanConvertTo(context, destinationType));
        }

        /// <summary>
        /// Tests that CanConvertTo returns true for string type regardless of context parameter value.
        /// Verifies the context parameter does not affect the conversion capability check.
        /// </summary>
        [Fact]
        public void CanConvertTo_StringTypeWithNonNullContext_ReturnsTrue()
        {
            // Arrange
            var converter = TypeDescriptor.GetConverter(typeof(FormattedString));
            var destinationType = typeof(string);
            var context = NSubstitute.Substitute.For<ITypeDescriptorContext>();

            // Act
            var result = converter.CanConvertTo(context, destinationType);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that CanConvertTo returns false for non-string type regardless of context parameter value.
        /// Verifies the context parameter does not affect the conversion capability check for non-string types.
        /// </summary>
        [Fact]
        public void CanConvertTo_NonStringTypeWithNonNullContext_ReturnsFalse()
        {
            // Arrange
            var converter = TypeDescriptor.GetConverter(typeof(FormattedString));
            var destinationType = typeof(int);
            var context = NSubstitute.Substitute.For<ITypeDescriptorContext>();

            // Act
            var result = converter.CanConvertTo(context, destinationType);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that ConvertFrom successfully converts a regular string to FormattedString.
        /// Input: Regular non-empty string
        /// Expected: Returns FormattedString with single Span containing the input text
        /// </summary>
        [Fact]
        public void ConvertFrom_ValidString_ReturnsFormattedString()
        {
            // Arrange
            var converter = new FormattedString.FormattedStringConverter();
            var testText = "Hello World";
            ITypeDescriptorContext context = null;
            CultureInfo culture = null;

            // Act
            var result = converter.ConvertFrom(context, culture, testText);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<FormattedString>(result);
            var formattedString = (FormattedString)result;
            Assert.Single(formattedString.Spans);
            Assert.Equal(testText, formattedString.Spans[0].Text);
        }

        /// <summary>
        /// Tests that ConvertFrom successfully converts an empty string to FormattedString.
        /// Input: Empty string
        /// Expected: Returns FormattedString with single Span containing empty text
        /// </summary>
        [Fact]
        public void ConvertFrom_EmptyString_ReturnsFormattedString()
        {
            // Arrange
            var converter = new FormattedString.FormattedStringConverter();
            var testText = "";
            ITypeDescriptorContext context = null;
            CultureInfo culture = null;

            // Act
            var result = converter.ConvertFrom(context, culture, testText);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<FormattedString>(result);
            var formattedString = (FormattedString)result;
            Assert.Single(formattedString.Spans);
            Assert.Equal(testText, formattedString.Spans[0].Text);
        }

        /// <summary>
        /// Tests that ConvertFrom successfully converts a whitespace-only string to FormattedString.
        /// Input: String containing only whitespace characters
        /// Expected: Returns FormattedString with single Span containing the whitespace text
        /// </summary>
        [Fact]
        public void ConvertFrom_WhitespaceString_ReturnsFormattedString()
        {
            // Arrange
            var converter = new FormattedString.FormattedStringConverter();
            var testText = "   \t\n  ";
            ITypeDescriptorContext context = null;
            CultureInfo culture = null;

            // Act
            var result = converter.ConvertFrom(context, culture, testText);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<FormattedString>(result);
            var formattedString = (FormattedString)result;
            Assert.Single(formattedString.Spans);
            Assert.Equal(testText, formattedString.Spans[0].Text);
        }

        /// <summary>
        /// Tests that ConvertFrom successfully converts a string with special characters to FormattedString.
        /// Input: String containing special characters and Unicode
        /// Expected: Returns FormattedString with single Span containing the special character text
        /// </summary>
        [Fact]
        public void ConvertFrom_StringWithSpecialCharacters_ReturnsFormattedString()
        {
            // Arrange
            var converter = new FormattedString.FormattedStringConverter();
            var testText = "Hello! @#$%^&*()_+ 🙂 çñü";
            ITypeDescriptorContext context = null;
            CultureInfo culture = null;

            // Act
            var result = converter.ConvertFrom(context, culture, testText);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<FormattedString>(result);
            var formattedString = (FormattedString)result;
            Assert.Single(formattedString.Spans);
            Assert.Equal(testText, formattedString.Spans[0].Text);
        }

        /// <summary>
        /// Tests that ConvertFrom successfully converts a very long string to FormattedString.
        /// Input: String with thousands of characters
        /// Expected: Returns FormattedString with single Span containing the entire long text
        /// </summary>
        [Fact]
        public void ConvertFrom_VeryLongString_ReturnsFormattedString()
        {
            // Arrange
            var converter = new FormattedString.FormattedStringConverter();
            var testText = new string('A', 10000);
            ITypeDescriptorContext context = null;
            CultureInfo culture = null;

            // Act
            var result = converter.ConvertFrom(context, culture, testText);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<FormattedString>(result);
            var formattedString = (FormattedString)result;
            Assert.Single(formattedString.Spans);
            Assert.Equal(testText, formattedString.Spans[0].Text);
        }

        /// <summary>
        /// Tests that ConvertFrom works correctly with various context and culture parameters when converting string.
        /// Input: Valid string with mock context and specific culture
        /// Expected: Returns FormattedString regardless of context and culture values
        /// </summary>
        [Fact]
        public void ConvertFrom_ValidStringWithContextAndCulture_ReturnsFormattedString()
        {
            // Arrange
            var converter = new FormattedString.FormattedStringConverter();
            var testText = "Test Text";
            var context = Substitute.For<ITypeDescriptorContext>();
            var culture = new CultureInfo("en-US");

            // Act
            var result = converter.ConvertFrom(context, culture, testText);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<FormattedString>(result);
            var formattedString = (FormattedString)result;
            Assert.Single(formattedString.Spans);
            Assert.Equal(testText, formattedString.Spans[0].Text);
        }

        /// <summary>
        /// Tests that ConvertFrom throws NotSupportedException when value is null.
        /// Input: null value
        /// Expected: Throws NotSupportedException
        /// </summary>
        [Fact]
        public void ConvertFrom_NullValue_ThrowsNotSupportedException()
        {
            // Arrange
            var converter = new FormattedString.FormattedStringConverter();
            object value = null;
            ITypeDescriptorContext context = null;
            CultureInfo culture = null;

            // Act & Assert
            Assert.Throws<NotSupportedException>(() => converter.ConvertFrom(context, culture, value));
        }

        /// <summary>
        /// Tests that ConvertFrom throws NotSupportedException when value is an integer.
        /// Input: Integer value
        /// Expected: Throws NotSupportedException
        /// </summary>
        [Fact]
        public void ConvertFrom_IntegerValue_ThrowsNotSupportedException()
        {
            // Arrange
            var converter = new FormattedString.FormattedStringConverter();
            var value = 42;
            ITypeDescriptorContext context = null;
            CultureInfo culture = null;

            // Act & Assert
            Assert.Throws<NotSupportedException>(() => converter.ConvertFrom(context, culture, value));
        }

        /// <summary>
        /// Tests that ConvertFrom throws NotSupportedException when value is a boolean.
        /// Input: Boolean value
        /// Expected: Throws NotSupportedException
        /// </summary>
        [Fact]
        public void ConvertFrom_BooleanValue_ThrowsNotSupportedException()
        {
            // Arrange
            var converter = new FormattedString.FormattedStringConverter();
            var value = true;
            ITypeDescriptorContext context = null;
            CultureInfo culture = null;

            // Act & Assert
            Assert.Throws<NotSupportedException>(() => converter.ConvertFrom(context, culture, value));
        }

        /// <summary>
        /// Tests that ConvertFrom throws NotSupportedException when value is a custom object.
        /// Input: Custom object instance
        /// Expected: Throws NotSupportedException
        /// </summary>
        [Fact]
        public void ConvertFrom_CustomObject_ThrowsNotSupportedException()
        {
            // Arrange
            var converter = new FormattedString.FormattedStringConverter();
            var value = new object();
            ITypeDescriptorContext context = null;
            CultureInfo culture = null;

            // Act & Assert
            Assert.Throws<NotSupportedException>(() => converter.ConvertFrom(context, culture, value));
        }

        /// <summary>
        /// Tests that ConvertFrom throws NotSupportedException when value is a double.
        /// Input: Double value including special values
        /// Expected: Throws NotSupportedException
        /// </summary>
        [Theory]
        [InlineData(3.14)]
        [InlineData(double.MinValue)]
        [InlineData(double.MaxValue)]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void ConvertFrom_DoubleValues_ThrowsNotSupportedException(double value)
        {
            // Arrange
            var converter = new FormattedString.FormattedStringConverter();
            ITypeDescriptorContext context = null;
            CultureInfo culture = null;

            // Act & Assert
            Assert.Throws<NotSupportedException>(() => converter.ConvertFrom(context, culture, value));
        }

        /// <summary>
        /// Tests that ConvertFrom throws NotSupportedException when value is various collection types.
        /// Input: Different collection types
        /// Expected: Throws NotSupportedException
        /// </summary>
        [Theory]
        [InlineData(typeof(int[]))]
        [InlineData(typeof(List<string>))]
        [InlineData(typeof(Dictionary<string, object>))]
        public void ConvertFrom_CollectionTypes_ThrowsNotSupportedException(Type collectionType)
        {
            // Arrange
            var converter = new FormattedString.FormattedStringConverter();
            var value = Activator.CreateInstance(collectionType);
            ITypeDescriptorContext context = null;
            CultureInfo culture = null;

            // Act & Assert
            Assert.Throws<NotSupportedException>(() => converter.ConvertFrom(context, culture, value));
        }

        /// <summary>
        /// Tests that ConvertTo returns the string representation when given a valid FormattedString with single span.
        /// </summary>
        [Fact]
        public void ConvertTo_ValidFormattedStringWithSingleSpan_ReturnsStringRepresentation()
        {
            // Arrange
            var converter = new FormattedString.FormattedStringConverter();
            var formattedString = new FormattedString();
            formattedString.Spans.Add(new Span { Text = "Hello World" });

            // Act
            var result = converter.ConvertTo(null, null, formattedString, typeof(string));

            // Assert
            Assert.Equal("Hello World", result);
        }

        /// <summary>
        /// Tests that ConvertTo returns concatenated string when given a FormattedString with multiple spans.
        /// </summary>
        [Fact]
        public void ConvertTo_FormattedStringWithMultipleSpans_ReturnsConcatenatedString()
        {
            // Arrange
            var converter = new FormattedString.FormattedStringConverter();
            var formattedString = new FormattedString();
            formattedString.Spans.Add(new Span { Text = "Hello " });
            formattedString.Spans.Add(new Span { Text = "Beautiful " });
            formattedString.Spans.Add(new Span { Text = "World" });

            // Act
            var result = converter.ConvertTo(null, null, formattedString, typeof(string));

            // Assert
            Assert.Equal("Hello Beautiful World", result);
        }

        /// <summary>
        /// Tests that ConvertTo returns empty string when given a FormattedString with no spans.
        /// </summary>
        [Fact]
        public void ConvertTo_FormattedStringWithNoSpans_ReturnsEmptyString()
        {
            // Arrange
            var converter = new FormattedString.FormattedStringConverter();
            var formattedString = new FormattedString();

            // Act
            var result = converter.ConvertTo(null, null, formattedString, typeof(string));

            // Assert
            Assert.Equal(string.Empty, result);
        }

        /// <summary>
        /// Tests that ConvertTo returns empty string when given a FormattedString with spans containing null text.
        /// </summary>
        [Fact]
        public void ConvertTo_FormattedStringWithNullSpanText_ReturnsEmptyString()
        {
            // Arrange
            var converter = new FormattedString.FormattedStringConverter();
            var formattedString = new FormattedString();
            formattedString.Spans.Add(new Span { Text = null });

            // Act
            var result = converter.ConvertTo(null, null, formattedString, typeof(string));

            // Assert
            Assert.Equal(string.Empty, result);
        }

        /// <summary>
        /// Tests that ConvertTo returns correctly concatenated string when mixing null and non-null span texts.
        /// </summary>
        [Fact]
        public void ConvertTo_FormattedStringWithMixedNullAndNonNullSpanTexts_ReturnsConcatenatedNonNullTexts()
        {
            // Arrange
            var converter = new FormattedString.FormattedStringConverter();
            var formattedString = new FormattedString();
            formattedString.Spans.Add(new Span { Text = "Hello" });
            formattedString.Spans.Add(new Span { Text = null });
            formattedString.Spans.Add(new Span { Text = " World" });

            // Act
            var result = converter.ConvertTo(null, null, formattedString, typeof(string));

            // Assert
            Assert.Equal("Hello World", result);
        }

        /// <summary>
        /// Tests that ConvertTo throws NotSupportedException when given null value.
        /// </summary>
        [Fact]
        public void ConvertTo_NullValue_ThrowsNotSupportedException()
        {
            // Arrange
            var converter = new FormattedString.FormattedStringConverter();

            // Act & Assert
            Assert.Throws<NotSupportedException>(() => converter.ConvertTo(null, null, null, typeof(string)));
        }

        /// <summary>
        /// Tests that ConvertTo throws NotSupportedException when given non-FormattedString object.
        /// </summary>
        [Fact]
        public void ConvertTo_NonFormattedStringObject_ThrowsNotSupportedException()
        {
            // Arrange
            var converter = new FormattedString.FormattedStringConverter();
            var nonFormattedStringObject = "regular string";

            // Act & Assert
            Assert.Throws<NotSupportedException>(() => converter.ConvertTo(null, null, nonFormattedStringObject, typeof(string)));
        }

        /// <summary>
        /// Tests that ConvertTo throws NotSupportedException when given integer value.
        /// </summary>
        [Fact]
        public void ConvertTo_IntegerValue_ThrowsNotSupportedException()
        {
            // Arrange
            var converter = new FormattedString.FormattedStringConverter();
            var intValue = 42;

            // Act & Assert
            Assert.Throws<NotSupportedException>(() => converter.ConvertTo(null, null, intValue, typeof(string)));
        }

        /// <summary>
        /// Tests that ConvertTo throws NotSupportedException when given arbitrary object.
        /// </summary>
        [Fact]
        public void ConvertTo_ArbitraryObject_ThrowsNotSupportedException()
        {
            // Arrange
            var converter = new FormattedString.FormattedStringConverter();
            var arbitraryObject = new object();

            // Act & Assert
            Assert.Throws<NotSupportedException>(() => converter.ConvertTo(null, null, arbitraryObject, typeof(string)));
        }

        /// <summary>
        /// Tests that ConvertTo works correctly with different context values (context parameter is ignored).
        /// </summary>
        [Fact]
        public void ConvertTo_WithDifferentContextValues_IgnoresContextParameter()
        {
            // Arrange
            var converter = new FormattedString.FormattedStringConverter();
            var formattedString = new FormattedString();
            formattedString.Spans.Add(new Span { Text = "Test" });
            var mockContext = NSubstitute.Substitute.For<ITypeDescriptorContext>();

            // Act
            var result = converter.ConvertTo(mockContext, null, formattedString, typeof(string));

            // Assert
            Assert.Equal("Test", result);
        }

        /// <summary>
        /// Tests that ConvertTo works correctly with different culture values (culture parameter is ignored).
        /// </summary>
        [Fact]
        public void ConvertTo_WithDifferentCultureValues_IgnoresCultureParameter()
        {
            // Arrange
            var converter = new FormattedString.FormattedStringConverter();
            var formattedString = new FormattedString();
            formattedString.Spans.Add(new Span { Text = "Test" });
            var culture = new CultureInfo("fr-FR");

            // Act
            var result = converter.ConvertTo(null, culture, formattedString, typeof(string));

            // Assert
            Assert.Equal("Test", result);
        }

        /// <summary>
        /// Tests that ConvertTo works correctly with different destination type values (destinationType parameter is ignored).
        /// </summary>
        [Fact]
        public void ConvertTo_WithDifferentDestinationTypes_IgnoresDestinationTypeParameter()
        {
            // Arrange
            var converter = new FormattedString.FormattedStringConverter();
            var formattedString = new FormattedString();
            formattedString.Spans.Add(new Span { Text = "Test" });

            // Act
            var result = converter.ConvertTo(null, null, formattedString, typeof(int));

            // Assert
            Assert.Equal("Test", result);
        }

        /// <summary>
        /// Tests that ConvertTo handles FormattedString with empty string span text correctly.
        /// </summary>
        [Fact]
        public void ConvertTo_FormattedStringWithEmptySpanText_ReturnsEmptyString()
        {
            // Arrange
            var converter = new FormattedString.FormattedStringConverter();
            var formattedString = new FormattedString();
            formattedString.Spans.Add(new Span { Text = "" });

            // Act
            var result = converter.ConvertTo(null, null, formattedString, typeof(string));

            // Assert
            Assert.Equal("", result);
        }

        /// <summary>
        /// Tests that ConvertTo handles FormattedString with whitespace span text correctly.
        /// </summary>
        [Fact]
        public void ConvertTo_FormattedStringWithWhitespaceSpanText_ReturnsWhitespace()
        {
            // Arrange
            var converter = new FormattedString.FormattedStringConverter();
            var formattedString = new FormattedString();
            formattedString.Spans.Add(new Span { Text = "   " });

            // Act
            var result = converter.ConvertTo(null, null, formattedString, typeof(string));

            // Assert
            Assert.Equal("   ", result);
        }
    }
}