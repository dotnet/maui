using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Maui.Controls;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public partial class StyleableElementTests
    {
        /// <summary>
        /// Tests that the @class property getter returns null when the underlying MergedStyle.StyleClass is null.
        /// Input: null value assigned to @class property.
        /// Expected: Getter returns null.
        /// </summary>
        [Fact]
        public void Class_Getter_ReturnsNull_WhenMergedStyleStyleClassIsNull()
        {
            // Arrange
            var element = new TestStyleableElement();

            // Act
            element.@class = null;
            var result = element.@class;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that the @class property setter can assign null value.
        /// Input: null value.
        /// Expected: The property accepts null and getter returns null.
        /// </summary>
        [Fact]
        public void Class_Setter_AcceptsNull()
        {
            // Arrange
            var element = new TestStyleableElement();
            element.@class = new List<string> { "initial-class" };

            // Act
            element.@class = null;

            // Assert
            Assert.Null(element.@class);
        }

        /// <summary>
        /// Tests that the @class property handles edge case string values including empty strings, whitespace, and special characters.
        /// Input: List containing edge case string values.
        /// Expected: All values are preserved correctly.
        /// </summary>
        [Fact]
        public void Class_Setter_HandlesEdgeCaseStringValues()
        {
            // Arrange
            var element = new TestStyleableElement();
            var edgeCaseList = new List<string>
            {
                "",                           // Empty string
                " ",                          // Single space
                "   ",                        // Multiple spaces
                "\t",                         // Tab
                "\n",                         // Newline
                "\r\n",                       // CRLF
                "normal-class",               // Normal class name
                "class-with-numbers123",      // With numbers
                "class_with_underscores",     // With underscores
                "class-with-hyphens",         // With hyphens
                "UPPERCASE_CLASS",            // Uppercase
                "MiXeD_cAsE_ClAsS",          // Mixed case
                "special!@#$%^&*()_+{}|:<>?[]\\;'\",./" // Special characters
            };

            // Act
            element.@class = edgeCaseList;

            // Assert
            Assert.Equal(edgeCaseList, element.@class);
            Assert.Equal(edgeCaseList.Count, element.@class.Count);
        }

        /// <summary>
        /// Tests that the @class property handles duplicate class names correctly.
        /// Input: List containing duplicate string values.
        /// Expected: Duplicates are preserved as-is.
        /// </summary>
        [Fact]
        public void Class_Setter_HandlesDuplicateClassNames()
        {
            // Arrange
            var element = new TestStyleableElement();
            var duplicateList = new List<string> { "class1", "class2", "class1", "class3", "class2", "class1" };

            // Act
            element.@class = duplicateList;

            // Assert
            Assert.Equal(duplicateList, element.@class);
            Assert.Equal(6, element.@class.Count);
        }

        /// <summary>
        /// Tests that the @class property handles very long string values.
        /// Input: List containing a very long string.
        /// Expected: Long string is handled correctly without truncation.
        /// </summary>
        [Fact]
        public void Class_Setter_HandlesVeryLongStringValues()
        {
            // Arrange
            var element = new TestStyleableElement();
            var longString = new string('a', 10000); // 10,000 character string
            var longStringList = new List<string> { "short-class", longString, "another-short-class" };

            // Act
            element.@class = longStringList;

            // Assert
            Assert.Equal(longStringList, element.@class);
            Assert.Equal(longString, element.@class[1]);
            Assert.Equal(10000, element.@class[1].Length);
        }

        /// <summary>
        /// Tests that modifying the list after assignment affects the property value (reference behavior).
        /// Input: Modifiable list that is changed after assignment.
        /// Expected: Changes to the original list are reflected in the property value.
        /// </summary>
        [Fact]
        public void Class_Setter_PreservesListReference()
        {
            // Arrange
            var element = new TestStyleableElement();
            var classList = new List<string> { "initial-class" };

            // Act
            element.@class = classList;
            classList.Add("added-class");

            // Assert
            Assert.Contains("added-class", element.@class);
            Assert.Equal(2, element.@class.Count);
        }

        /// <summary>
        /// Tests that the @class property setter works correctly when called multiple times in succession.
        /// Input: Multiple different IList&lt;string&gt; values assigned sequentially.
        /// Expected: Each assignment overwrites the previous value correctly.
        /// </summary>
        [Fact]
        public void Class_Setter_HandlesMultipleConsecutiveAssignments()
        {
            // Arrange
            var element = new TestStyleableElement();
            var firstList = new List<string> { "first", "list" };
            var secondList = new List<string> { "second", "different", "list" };
            var thirdList = new List<string>();

            // Act & Assert
            element.@class = firstList;
            Assert.Equal(firstList, element.@class);

            element.@class = secondList;
            Assert.Equal(secondList, element.@class);
            Assert.Equal(3, element.@class.Count);

            element.@class = thirdList;
            Assert.Equal(thirdList, element.@class);
            Assert.Empty(element.@class);

            element.@class = null;
            Assert.Null(element.@class);
        }

        /// <summary>
        /// Concrete implementation of StyleableElement for testing purposes.
        /// This class provides the minimal implementation needed to test the @class property.
        /// </summary>
        private class TestStyleableElement : StyleableElement
        {
            // Minimal concrete implementation for testing
        }
    }
}
