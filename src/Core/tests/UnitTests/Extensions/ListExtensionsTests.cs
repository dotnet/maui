using System;
using System.Collections.Generic;
using Microsoft.Maui;
using NSubstitute;
using Xunit;


namespace Microsoft.Maui.UnitTests
{
    public partial class ListExtensionsTests
    {
        /// <summary>
        /// Tests that TryRemove returns true when the item is successfully removed from the list.
        /// Input: A mocked list where Remove operation succeeds.
        /// Expected: Method returns true.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void TryRemove_ItemSuccessfullyRemoved_ReturnsTrue()
        {
            // Arrange
            var mockList = Substitute.For<IList<string>>();
            var item = "test-item";
            mockList.Remove(item).Returns(true);

            // Act
            var result = ListExtensions.TryRemove(mockList, item);

            // Assert
            Assert.True(result);
            mockList.Received(1).Remove(item);
        }

        /// <summary>
        /// Tests that TryRemove returns false when Remove throws any exception.
        /// Input: A mocked list where Remove operation throws an exception.
        /// Expected: Method returns false and does not rethrow the exception.
        /// </summary>
        [Theory]
        [InlineData(typeof(NotSupportedException))]
        [InlineData(typeof(ArgumentException))]
        [InlineData(typeof(InvalidOperationException))]
        [InlineData(typeof(Exception))]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void TryRemove_RemoveThrowsException_ReturnsFalse(Type exceptionType)
        {
            // Arrange
            var mockList = Substitute.For<IList<string>>();
            var item = "test-item";
            var exception = (Exception)Activator.CreateInstance(exceptionType);
            mockList.When(x => x.Remove(item)).Do(x => throw exception);

            // Act
            var result = ListExtensions.TryRemove(mockList, item);

            // Assert
            Assert.False(result);
            mockList.Received(1).Remove(item);
        }

        /// <summary>
        /// Tests that TryRemove handles null list parameter by catching NullReferenceException.
        /// Input: Null list reference.
        /// Expected: Method returns false without throwing.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void TryRemove_NullList_ReturnsFalse()
        {
            // Arrange
            IList<string> nullList = null;
            var item = "test-item";

            // Act
            var result = ListExtensions.TryRemove(nullList, item);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that TryRemove works correctly with null items.
        /// Input: A mocked list and null item where Remove succeeds.
        /// Expected: Method returns true.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void TryRemove_NullItem_RemoveSucceeds_ReturnsTrue()
        {
            // Arrange
            var mockList = Substitute.For<IList<string>>();
            string nullItem = null;
            mockList.Remove(nullItem).Returns(true);

            // Act
            var result = ListExtensions.TryRemove(mockList, nullItem);

            // Assert
            Assert.True(result);
            mockList.Received(1).Remove(nullItem);
        }

        /// <summary>
        /// Tests that TryRemove works correctly with null items when Remove throws exception.
        /// Input: A mocked list and null item where Remove throws exception.
        /// Expected: Method returns false.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void TryRemove_NullItem_RemoveThrowsException_ReturnsFalse()
        {
            // Arrange
            var mockList = Substitute.For<IList<string>>();
            string nullItem = null;
            mockList.When(x => x.Remove(nullItem)).Do(x => throw new ArgumentNullException());

            // Act
            var result = ListExtensions.TryRemove(mockList, nullItem);

            // Assert
            Assert.False(result);
            mockList.Received(1).Remove(nullItem);
        }

        /// <summary>
        /// Tests that TryRemove works with different generic types.
        /// Input: A mocked list of integers where Remove succeeds.
        /// Expected: Method returns true.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void TryRemove_IntegerType_RemoveSucceeds_ReturnsTrue()
        {
            // Arrange
            var mockList = Substitute.For<IList<int>>();
            var item = 42;
            mockList.Remove(item).Returns(true);

            // Act
            var result = ListExtensions.TryRemove(mockList, item);

            // Assert
            Assert.True(result);
            mockList.Received(1).Remove(item);
        }

        /// <summary>
        /// Tests that TryRemove works with different generic types when exception occurs.
        /// Input: A mocked list of integers where Remove throws exception.
        /// Expected: Method returns false.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void TryRemove_IntegerType_RemoveThrowsException_ReturnsFalse()
        {
            // Arrange
            var mockList = Substitute.For<IList<int>>();
            var item = 42;
            mockList.When(x => x.Remove(item)).Do(x => throw new NotSupportedException("Read-only collection"));

            // Act
            var result = ListExtensions.TryRemove(mockList, item);

            // Assert
            Assert.False(result);
            mockList.Received(1).Remove(item);
        }

        /// <summary>
        /// Tests that TryRemove handles extreme values correctly.
        /// Input: A mocked list with extreme integer values.
        /// Expected: Method returns true when Remove succeeds.
        /// </summary>
        [Theory]
        [InlineData(int.MinValue)]
        [InlineData(int.MaxValue)]
        [InlineData(0)]
        [InlineData(-1)]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void TryRemove_ExtremeIntegerValues_RemoveSucceeds_ReturnsTrue(int item)
        {
            // Arrange
            var mockList = Substitute.For<IList<int>>();
            mockList.Remove(item).Returns(true);

            // Act
            var result = ListExtensions.TryRemove(mockList, item);

            // Assert
            Assert.True(result);
            mockList.Received(1).Remove(item);
        }

        /// <summary>
        /// Tests that TryRemove handles edge case string values correctly.
        /// Input: A mocked list with various edge case string values.
        /// Expected: Method returns true when Remove succeeds.
        /// </summary>
        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("very long string that might cause issues in some implementations")]
        [InlineData("\n\r\t")]
        [InlineData("🚀🎉")]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void TryRemove_EdgeCaseStringValues_RemoveSucceeds_ReturnsTrue(string item)
        {
            // Arrange
            var mockList = Substitute.For<IList<string>>();
            mockList.Remove(item).Returns(true);

            // Act
            var result = ListExtensions.TryRemove(mockList, item);

            // Assert
            Assert.True(result);
            mockList.Received(1).Remove(item);
        }
    }
}
