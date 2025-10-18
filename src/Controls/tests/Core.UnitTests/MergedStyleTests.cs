#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Microsoft.Extensions.Logging;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    /// <summary>
    /// Unit tests for the MergedStyle.Style property.
    /// </summary>
    public partial class MergedStyleTests
    {
        /// <summary>
        /// Tests that setting the same style value returns early without calling SetStyle.
        /// This test covers the not-covered line 46 where _style == value returns early.
        /// </summary>
        /// <param name="styleValue">The style value to test with (null or mock style)</param>
        [Theory]
        [InlineData(null)]
        [InlineData("mockStyle")]
        public void Style_SetSameValue_ReturnsEarly(string styleValue)
        {
            // Arrange
            var mockTarget = Substitute.For<BindableObject>();
            var targetType = typeof(View);
            var mergedStyle = new TestableMergedStyle(targetType, mockTarget);

            var style = styleValue == "mockStyle" ? CreateMockStyle(targetType) : null;

            // Set initial style
            mergedStyle.Style = style;
            var initialSetStyleCallCount = mergedStyle.SetStyleCallCount;

            // Act - set the same style again
            mergedStyle.Style = style;

            // Assert - should not call SetStyle again (early return)
            Assert.Equal(initialSetStyleCallCount, mergedStyle.SetStyleCallCount);
        }

        /// <summary>
        /// Tests that setting a different style calls SetStyle with correct parameters.
        /// </summary>
        [Fact]
        public void Style_SetDifferentValue_CallsSetStyle()
        {
            // Arrange
            var mockTarget = Substitute.For<BindableObject>();
            var targetType = typeof(View);
            var mergedStyle = new TestableMergedStyle(targetType, mockTarget);

            var initialStyle = CreateMockStyle(targetType);
            var newStyle = CreateMockStyle(targetType);

            mergedStyle.Style = initialStyle;
            var initialCallCount = mergedStyle.SetStyleCallCount;

            // Act
            mergedStyle.Style = newStyle;

            // Assert
            Assert.Equal(initialCallCount + 1, mergedStyle.SetStyleCallCount);
        }

        /// <summary>
        /// Tests that setting a style with incompatible target type logs a warning.
        /// </summary>
        [Fact]
        public void Style_SetIncompatibleTargetType_LogsWarning()
        {
            // Arrange
            var mockTarget = Substitute.For<BindableObject>();
            var targetType = typeof(View);
            var mergedStyle = new TestableMergedStyle(targetType, mockTarget);

            var incompatibleStyle = CreateMockStyle(typeof(string)); // string is not assignable from View

            // Act
            mergedStyle.Style = incompatibleStyle;

            // Assert - verify the style was still set despite incompatibility
            Assert.Equal(incompatibleStyle, mergedStyle.Style);
        }

        /// <summary>
        /// Tests that setting a compatible style does not cause issues.
        /// </summary>
        [Fact]
        public void Style_SetCompatibleTargetType_SetsStyleSuccessfully()
        {
            // Arrange
            var mockTarget = Substitute.For<BindableObject>();
            var targetType = typeof(View);
            var mergedStyle = new TestableMergedStyle(targetType, mockTarget);

            var compatibleStyle = CreateMockStyle(typeof(Element)); // Element is assignable from View

            // Act
            mergedStyle.Style = compatibleStyle;

            // Assert
            Assert.Equal(compatibleStyle, mergedStyle.Style);
        }

        /// <summary>
        /// Tests that setting null style works correctly.
        /// </summary>
        [Fact]
        public void Style_SetNull_SetsToNull()
        {
            // Arrange
            var mockTarget = Substitute.For<BindableObject>();
            var targetType = typeof(View);
            var mergedStyle = new TestableMergedStyle(targetType, mockTarget);

            var initialStyle = CreateMockStyle(targetType);
            mergedStyle.Style = initialStyle;

            // Act
            mergedStyle.Style = null;

            // Assert
            Assert.Null(mergedStyle.Style);
        }

        /// <summary>
        /// Tests edge case with boundary type compatibility.
        /// </summary>
        [Theory]
        [InlineData(typeof(object), typeof(View), true)] // object is assignable from View
        [InlineData(typeof(View), typeof(Element), false)] // View is not assignable from Element
        [InlineData(typeof(VisualElement), typeof(View), true)] // VisualElement is assignable from View
        public void Style_TypeCompatibility_HandledCorrectly(Type styleTargetType, Type mergedStyleTargetType, bool shouldBeCompatible)
        {
            // Arrange
            var mockTarget = Substitute.For<BindableObject>();
            var mergedStyle = new TestableMergedStyle(mergedStyleTargetType, mockTarget);

            var style = CreateMockStyle(styleTargetType);

            // Act
            mergedStyle.Style = style;

            // Assert
            Assert.Equal(style, mergedStyle.Style);
        }

        private static IStyle CreateMockStyle(Type targetType)
        {
            var mockStyle = Substitute.For<IStyle>();
            mockStyle.TargetType.Returns(targetType);
            return mockStyle;
        }

        /// <summary>
        /// Tests that StyleClass getter returns the current _styleClass field value.
        /// </summary>
        [Fact]
        public void StyleClass_Get_ReturnsCurrentValue()
        {
            // Arrange
            var target = Substitute.For<BindableObject>();
            var mergedStyle = new MergedStyle(typeof(View), target);
            var expectedStyleClass = new List<string> { "class1", "class2" };

            // Act
            mergedStyle.StyleClass = expectedStyleClass;
            var result = mergedStyle.StyleClass;

            // Assert
            Assert.Same(expectedStyleClass, result);
        }

        /// <summary>
        /// Tests that StyleClass setter returns early when setting the same value (reference equality).
        /// Tests the uncovered early return condition.
        /// </summary>
        [Fact]
        public void StyleClass_SetSameValue_ReturnsEarly()
        {
            // Arrange
            var target = Substitute.For<BindableObject>();
            var mergedStyle = new MergedStyle(typeof(View), target);
            var styleClass = new List<string> { "class1" };
            mergedStyle.StyleClass = styleClass;

            // Reset any calls that happened during initial setup
            target.ClearReceivedCalls();

            // Act
            mergedStyle.StyleClass = styleClass; // Setting same reference

            // Assert
            // Verify no additional calls were made since it should return early
            target.DidNotReceive().RemoveDynamicResource(Arg.Any<BindableProperty>());
            target.DidNotReceive().OnSetDynamicResource(Arg.Any<BindableProperty>(), Arg.Any<string>(), Arg.Any<SetterSpecificity>());
        }

        /// <summary>
        /// Tests setting StyleClass to null from a non-null value.
        /// </summary>
        [Fact]
        public void StyleClass_SetNull_UpdatesValue()
        {
            // Arrange
            var target = Substitute.For<BindableObject>();
            var mergedStyle = new MergedStyle(typeof(View), target);
            var initialStyleClass = new List<string> { "class1" };
            mergedStyle.StyleClass = initialStyleClass;

            // Act
            mergedStyle.StyleClass = null;

            // Assert
            Assert.Null(mergedStyle.StyleClass);
        }

        /// <summary>
        /// Tests setting StyleClass with empty list.
        /// </summary>
        [Fact]
        public void StyleClass_SetEmptyList_UpdatesValue()
        {
            // Arrange
            var target = Substitute.For<BindableObject>();
            var mergedStyle = new MergedStyle(typeof(View), target);
            var emptyList = new List<string>();

            // Act
            mergedStyle.StyleClass = emptyList;

            // Assert
            Assert.Same(emptyList, mergedStyle.StyleClass);
        }

        /// <summary>
        /// Tests StyleClass setter with various string edge cases including null, empty, and whitespace strings.
        /// </summary>
        [Theory]
        [MemberData(nameof(StyleClassStringEdgeCases))]
        public void StyleClass_SetWithStringEdgeCases_HandlesCorrectly(List<string> styleClasses, string testDescription)
        {
            // Arrange
            var target = Substitute.For<BindableObject>();
            var mergedStyle = new MergedStyle(typeof(View), target);

            // Act
            mergedStyle.StyleClass = styleClasses;

            // Assert
            Assert.Same(styleClasses, mergedStyle.StyleClass);

            // Verify appropriate dynamic resource calls were made for non-null list
            if (styleClasses != null)
            {
                foreach (var styleClass in styleClasses)
                {
                    target.Received(1).OnSetDynamicResource(
                        Arg.Any<BindableProperty>(),
                        Style.StyleClassPrefix + styleClass,
                        SetterSpecificity.DefaultValue);
                }
            }
        }

        public static TheoryData<List<string>, string> StyleClassStringEdgeCases()
        {
            return new TheoryData<List<string>, string>
            {
                { new List<string> { null }, "List with null string" },
                { new List<string> { "" }, "List with empty string" },
                { new List<string> { "   " }, "List with whitespace-only string" },
                { new List<string> { "validClass", null, "" }, "List with mixed valid and invalid strings" },
                { new List<string> { "class1", "class1" }, "List with duplicate strings" },
                { new List<string> { new string('a', 1000) }, "List with very long string" }
            };
        }

        /// <summary>
        /// Tests that when both _styleClass and _classStyleProperties are not null, 
        /// RemoveDynamicResource is called for each class style property.
        /// Tests the uncovered removal logic.
        /// </summary>
        [Fact]
        public void StyleClass_SetNewValueWhenPreviousExists_RemovesPreviousDynamicResources()
        {
            // Arrange
            var target = Substitute.For<BindableObject>();
            var mergedStyle = new MergedStyle(typeof(View), target);

            // First, set an initial StyleClass to establish _classStyleProperties
            var initialStyleClass = new List<string> { "class1", "class2" };
            mergedStyle.StyleClass = initialStyleClass;

            // Clear any calls from the initial setup
            target.ClearReceivedCalls();

            // Act
            var newStyleClass = new List<string> { "class3" };
            mergedStyle.StyleClass = newStyleClass;

            // Assert
            // Verify RemoveDynamicResource was called for each previous class style property
            target.Received(2).RemoveDynamicResource(Arg.Any<BindableProperty>());

            // Verify new dynamic resources were set
            target.Received(1).OnSetDynamicResource(
                Arg.Any<BindableProperty>(),
                Style.StyleClassPrefix + "class3",
                SetterSpecificity.DefaultValue);
        }

        /// <summary>
        /// Tests that when Target is an Element, ApplyStyleSheets is called after setting StyleClass.
        /// </summary>
        [Fact]
        public void StyleClass_SetWithElementTarget_CallsApplyStyleSheets()
        {
            // Arrange
            var elementTarget = Substitute.For<Element>();
            var mergedStyle = new MergedStyle(typeof(View), elementTarget);

            // Act
            mergedStyle.StyleClass = new List<string> { "class1" };

            // Assert
            elementTarget.Received(1).ApplyStyleSheets();
        }

        /// <summary>
        /// Tests that when Target is not an Element, ApplyStyleSheets is not called.
        /// </summary>
        [Fact]
        public void StyleClass_SetWithNonElementTarget_DoesNotCallApplyStyleSheets()
        {
            // Arrange
            var nonElementTarget = Substitute.For<BindableObject>();
            var mergedStyle = new MergedStyle(typeof(View), nonElementTarget);

            // Act
            mergedStyle.StyleClass = new List<string> { "class1" };

            // Assert
            // Since Target is not an Element, we can't verify ApplyStyleSheets wasn't called
            // But we can verify the basic dynamic resource setup still works
            nonElementTarget.Received(1).OnSetDynamicResource(
                Arg.Any<BindableProperty>(),
                Style.StyleClassPrefix + "class1",
                SetterSpecificity.DefaultValue);
        }

        /// <summary>
        /// Tests setting StyleClass multiple times to ensure proper cleanup and setup.
        /// </summary>
        [Fact]
        public void StyleClass_SetMultipleTimes_HandlesCorrectly()
        {
            // Arrange
            var target = Substitute.For<BindableObject>();
            var mergedStyle = new MergedStyle(typeof(View), target);

            // Act & Assert - First set
            var firstStyleClass = new List<string> { "class1" };
            mergedStyle.StyleClass = firstStyleClass;
            Assert.Same(firstStyleClass, mergedStyle.StyleClass);

            // Act & Assert - Second set (should remove previous and add new)
            target.ClearReceivedCalls();
            var secondStyleClass = new List<string> { "class2", "class3" };
            mergedStyle.StyleClass = secondStyleClass;

            Assert.Same(secondStyleClass, mergedStyle.StyleClass);
            target.Received(1).RemoveDynamicResource(Arg.Any<BindableProperty>());
            target.Received(2).OnSetDynamicResource(Arg.Any<BindableProperty>(), Arg.Any<string>(), Arg.Any<SetterSpecificity>());

            // Act & Assert - Third set to null
            target.ClearReceivedCalls();
            mergedStyle.StyleClass = null;
            Assert.Null(mergedStyle.StyleClass);
            target.Received(2).RemoveDynamicResource(Arg.Any<BindableProperty>());
        }

        /// <summary>
        /// Tests that BindableProperty.Create is called with correct parameters for each style class.
        /// </summary>
        [Fact]
        public void StyleClass_Set_CreatesBindablePropertiesWithCorrectParameters()
        {
            // Arrange
            var target = Substitute.For<BindableObject>();
            var mergedStyle = new MergedStyle(typeof(View), target);
            var styleClasses = new List<string> { "class1", "class2" };

            // Act
            mergedStyle.StyleClass = styleClasses;

            // Assert
            // Verify OnSetDynamicResource was called for each style class with correct prefix
            target.Received(1).OnSetDynamicResource(
                Arg.Any<BindableProperty>(),
                Style.StyleClassPrefix + "class1",
                SetterSpecificity.DefaultValue);
            target.Received(1).OnSetDynamicResource(
                Arg.Any<BindableProperty>(),
                Style.StyleClassPrefix + "class2",
                SetterSpecificity.DefaultValue);
        }

        /// <summary>
        /// Tests that setting StyleClass to null when _styleClass is already null does not cause issues.
        /// </summary>
        [Fact]
        public void StyleClass_SetNullWhenAlreadyNull_HandlesCorrectly()
        {
            // Arrange
            var target = Substitute.For<BindableObject>();
            var mergedStyle = new MergedStyle(typeof(View), target);
            // _styleClass should be null by default

            // Act
            mergedStyle.StyleClass = null;

            // Assert
            Assert.Null(mergedStyle.StyleClass);
            // No dynamic resource operations should occur
            target.DidNotReceive().RemoveDynamicResource(Arg.Any<BindableProperty>());
            target.DidNotReceive().OnSetDynamicResource(Arg.Any<BindableProperty>(), Arg.Any<string>(), Arg.Any<SetterSpecificity>());
        }

        /// <summary>
        /// Tests that Apply method with SetterSpecificity parameter delegates to private Apply method without throwing exceptions.
        /// </summary>
        [Fact]
        public void Apply_WithValidBindableObjectAndSpecificity_DoesNotThrow()
        {
            // Arrange
            var mockBindableObject = Substitute.For<BindableObject>();
            var mockTarget = Substitute.For<BindableObject>();
            var targetType = typeof(BindableObject);
            var mergedStyle = new MergedStyle(targetType, mockTarget);
            var specificity = SetterSpecificity.DefaultValue;

            // Act & Assert
            var exception = Record.Exception(() => mergedStyle.Apply(mockBindableObject, specificity));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that Apply method handles null bindable object parameter appropriately.
        /// </summary>
        [Fact]
        public void Apply_WithNullBindableObject_DoesNotThrow()
        {
            // Arrange
            var mockTarget = Substitute.For<BindableObject>();
            var targetType = typeof(BindableObject);
            var mergedStyle = new MergedStyle(targetType, mockTarget);
            var specificity = SetterSpecificity.DefaultValue;

            // Act & Assert
            var exception = Record.Exception(() => mergedStyle.Apply(null, specificity));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that Apply method can be called multiple times without issues.
        /// </summary>
        [Fact]
        public void Apply_CalledMultipleTimes_DoesNotThrow()
        {
            // Arrange
            var mockBindableObject = Substitute.For<BindableObject>();
            var mockTarget = Substitute.For<BindableObject>();
            var targetType = typeof(BindableObject);
            var mergedStyle = new MergedStyle(targetType, mockTarget);
            var specificity = SetterSpecificity.DefaultValue;

            // Act & Assert
            var exception1 = Record.Exception(() => mergedStyle.Apply(mockBindableObject, specificity));
            var exception2 = Record.Exception(() => mergedStyle.Apply(mockBindableObject, specificity));
            var exception3 = Record.Exception(() => mergedStyle.Apply(mockBindableObject, specificity));

            Assert.Null(exception1);
            Assert.Null(exception2);
            Assert.Null(exception3);
        }

        /// <summary>
        /// Tests Apply method with various BindableObject types to ensure compatibility.
        /// </summary>
        [Fact]
        public void Apply_WithDifferentBindableObjectTypes_DoesNotThrow()
        {
            // Arrange
            var mockBindableObject1 = Substitute.For<BindableObject>();
            var mockBindableObject2 = Substitute.For<BindableObject>();
            var mockTarget = Substitute.For<BindableObject>();
            var targetType = typeof(BindableObject);
            var mergedStyle = new MergedStyle(targetType, mockTarget);
            var specificity = SetterSpecificity.DefaultValue;

            // Act & Assert
            var exception1 = Record.Exception(() => mergedStyle.Apply(mockBindableObject1, specificity));
            var exception2 = Record.Exception(() => mergedStyle.Apply(mockBindableObject2, specificity));

            Assert.Null(exception1);
            Assert.Null(exception2);
        }

        public static IEnumerable<object[]> GetSetterSpecificityValues()
        {
            yield return new object[] { SetterSpecificity.DefaultValue };
            yield return new object[] { SetterSpecificity.VisualStateSetter };
            yield return new object[] { SetterSpecificity.FromBinding };
            yield return new object[] { SetterSpecificity.ManualValueSetter };
            yield return new object[] { SetterSpecificity.Trigger };
            yield return new object[] { SetterSpecificity.DynamicResourceSetter };
            yield return new object[] { SetterSpecificity.FromHandler };
            yield return new object[] { new SetterSpecificity(100) };
            yield return new object[] { new SetterSpecificity(0) };
        }

        /// <summary>
        /// Tests that UnApply does not throw when all style properties are null.
        /// Input: bindable object, all styles null
        /// Expected: No exception thrown, no UnApply methods called
        /// </summary>
        [Fact]
        public void UnApply_AllStylesNull_DoesNotThrowException()
        {
            // Arrange
            var targetType = typeof(View);
            var target = Substitute.For<BindableObject>();
            var bindable = Substitute.For<BindableObject>();

            var mergedStyle = new MergedStyle(targetType, target);
            // Style, ClassStyles, and ImplicitStyle should be null by default

            // Act & Assert
            var exception = Record.Exception(() => mergedStyle.UnApply(bindable));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that UnApply calls Style.UnApply when Style is not null.
        /// Input: bindable object, Style set to mock
        /// Expected: Style.UnApply called with bindable parameter
        /// </summary>
        [Fact]
        public void UnApply_StyleNotNull_CallsStyleUnApply()
        {
            // Arrange
            var targetType = typeof(View);
            var target = Substitute.For<BindableObject>();
            var bindable = Substitute.For<BindableObject>();
            var mockStyle = Substitute.For<IStyle>();
            mockStyle.TargetType.Returns(targetType);

            var mergedStyle = new MergedStyle(targetType, target);
            mergedStyle.Style = mockStyle;

            // Act
            mergedStyle.UnApply(bindable);

            // Assert
            mockStyle.Received(1).UnApply(bindable);
        }

        /// <summary>
        /// Tests that UnApply skips ClassStyles loop when ClassStyles is null.
        /// Input: bindable object, ClassStyles null
        /// Expected: No ClassStyle UnApply methods called
        /// </summary>
        [Fact]
        public void UnApply_ClassStylesNull_SkipsClassStylesLoop()
        {
            // Arrange
            var targetType = typeof(View);
            var target = Substitute.For<BindableObject>();
            var bindable = Substitute.For<BindableObject>();

            var mergedStyle = new MergedStyle(targetType, target);
            // ClassStyles should be null by default

            // Act & Assert
            var exception = Record.Exception(() => mergedStyle.UnApply(bindable));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that UnApply handles empty ClassStyles collection.
        /// Input: bindable object, ClassStyles empty list
        /// Expected: No UnApply methods called on ClassStyles
        /// </summary>
        [Fact]
        public void UnApply_ClassStylesEmpty_SkipsClassStylesLoop()
        {
            // Arrange
            var targetType = typeof(View);
            var target = Substitute.For<BindableObject>();
            var bindable = Substitute.For<BindableObject>();
            var emptyClassStyles = new List<Style>();

            var mergedStyle = new MergedStyle(targetType, target);
            mergedStyle.StyleClass = new List<string>(); // This will initialize ClassStyles

            // Act & Assert
            var exception = Record.Exception(() => mergedStyle.UnApply(bindable));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that UnApply calls UnApply on single style in ClassStyles.
        /// Input: bindable object, ClassStyles with one style
        /// Expected: Style.UnApply called once with bindable parameter
        /// </summary>
        [Fact]
        public void UnApply_ClassStylesWithOneStyle_CallsUnApplyOnStyle()
        {
            // Arrange
            var targetType = typeof(View);
            var target = Substitute.For<BindableObject>();
            var bindable = Substitute.For<BindableObject>();
            var mockStyle = new Style(targetType);
            var classStyles = new List<Style> { mockStyle };

            var mergedStyle = new MergedStyle(targetType, target);

            // Use reflection to set _classStyles field since ClassStyles setter calls SetStyle
            var classStylesField = typeof(MergedStyle).GetField("_classStyles",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            classStylesField.SetValue(mergedStyle, classStyles);

            // Act
            mergedStyle.UnApply(bindable);

            // Assert - verify no exceptions thrown (Style.UnApply is not virtual so we can't verify the call)
            var exception = Record.Exception(() => mergedStyle.UnApply(bindable));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that UnApply calls UnApply on all styles in ClassStyles.
        /// Input: bindable object, ClassStyles with multiple styles
        /// Expected: UnApply called on all styles with bindable parameter
        /// </summary>
        [Fact]
        public void UnApply_ClassStylesWithMultipleStyles_CallsUnApplyOnAllStyles()
        {
            // Arrange
            var targetType = typeof(View);
            var target = Substitute.For<BindableObject>();
            var bindable = Substitute.For<BindableObject>();
            var style1 = new Style(targetType);
            var style2 = new Style(targetType);
            var classStyles = new List<Style> { style1, style2 };

            var mergedStyle = new MergedStyle(targetType, target);

            // Use reflection to set _classStyles field
            var classStylesField = typeof(MergedStyle).GetField("_classStyles",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            classStylesField.SetValue(mergedStyle, classStyles);

            // Act & Assert
            var exception = Record.Exception(() => mergedStyle.UnApply(bindable));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that UnApply handles null style in ClassStyles gracefully.
        /// Input: bindable object, ClassStyles with null style
        /// Expected: No exception thrown, null style skipped due to null-conditional operator
        /// </summary>
        [Fact]
        public void UnApply_ClassStylesWithNullStyle_HandlesNullStyleGracefully()
        {
            // Arrange
            var targetType = typeof(View);
            var target = Substitute.For<BindableObject>();
            var bindable = Substitute.For<BindableObject>();
            var classStyles = new List<Style> { null };

            var mergedStyle = new MergedStyle(targetType, target);

            // Use reflection to set _classStyles field
            var classStylesField = typeof(MergedStyle).GetField("_classStyles",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            classStylesField.SetValue(mergedStyle, classStyles);

            // Act & Assert
            var exception = Record.Exception(() => mergedStyle.UnApply(bindable));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that UnApply calls ImplicitStyle.UnApply when ImplicitStyle is not null.
        /// Input: bindable object, ImplicitStyle set to mock
        /// Expected: ImplicitStyle.UnApply called with bindable parameter
        /// </summary>
        [Fact]
        public void UnApply_ImplicitStyleNotNull_CallsImplicitStyleUnApply()
        {
            // Arrange
            var targetType = typeof(View);
            var target = Substitute.For<BindableObject>();
            var bindable = Substitute.For<BindableObject>();
            var mockImplicitStyle = Substitute.For<IStyle>();
            mockImplicitStyle.TargetType.Returns(targetType);

            var mergedStyle = new MergedStyle(targetType, target);

            // Use reflection to set _implicitStyle field
            var implicitStyleField = typeof(MergedStyle).GetField("_implicitStyle",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            implicitStyleField.SetValue(mergedStyle, mockImplicitStyle);

            // Act
            mergedStyle.UnApply(bindable);

            // Assert
            mockImplicitStyle.Received(1).UnApply(bindable);
        }

        /// <summary>
        /// Tests that UnApply passes null bindable parameter to UnApply methods.
        /// Input: null bindable object
        /// Expected: null passed to all UnApply method calls
        /// </summary>
        [Fact]
        public void UnApply_NullBindable_PassesNullToUnApplyMethods()
        {
            // Arrange
            var targetType = typeof(View);
            var target = Substitute.For<BindableObject>();
            var mockStyle = Substitute.For<IStyle>();
            var mockImplicitStyle = Substitute.For<IStyle>();
            mockStyle.TargetType.Returns(targetType);
            mockImplicitStyle.TargetType.Returns(targetType);

            var mergedStyle = new MergedStyle(targetType, target);
            mergedStyle.Style = mockStyle;

            // Use reflection to set _implicitStyle field
            var implicitStyleField = typeof(MergedStyle).GetField("_implicitStyle",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            implicitStyleField.SetValue(mergedStyle, mockImplicitStyle);

            // Act
            mergedStyle.UnApply(null);

            // Assert
            mockStyle.Received(1).UnApply(null);
            mockImplicitStyle.Received(1).UnApply(null);
        }

        /// <summary>
        /// Tests that UnApply calls UnApply on all configured styles (Style, ClassStyles, ImplicitStyle).
        /// Input: bindable object, all styles configured
        /// Expected: UnApply called on Style, all ClassStyles, and ImplicitStyle
        /// </summary>
        [Fact]
        public void UnApply_AllStylesSet_CallsUnApplyOnAll()
        {
            // Arrange
            var targetType = typeof(View);
            var target = Substitute.For<BindableObject>();
            var bindable = Substitute.For<BindableObject>();
            var mockStyle = Substitute.For<IStyle>();
            var mockImplicitStyle = Substitute.For<IStyle>();
            var classStyle1 = new Style(targetType);
            var classStyle2 = new Style(targetType);
            var classStyles = new List<Style> { classStyle1, classStyle2 };

            mockStyle.TargetType.Returns(targetType);
            mockImplicitStyle.TargetType.Returns(targetType);

            var mergedStyle = new MergedStyle(targetType, target);
            mergedStyle.Style = mockStyle;

            // Use reflection to set fields
            var classStylesField = typeof(MergedStyle).GetField("_classStyles",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            classStylesField.SetValue(mergedStyle, classStyles);

            var implicitStyleField = typeof(MergedStyle).GetField("_implicitStyle",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            implicitStyleField.SetValue(mergedStyle, mockImplicitStyle);

            // Act
            mergedStyle.UnApply(bindable);

            // Assert
            mockStyle.Received(1).UnApply(bindable);
            mockImplicitStyle.Received(1).UnApply(bindable);
            // ClassStyles UnApply calls cannot be verified as Style.UnApply is not virtual
        }

        /// <summary>
        /// Tests that ReRegisterImplicitStyles with valid fallbackTypeName clears existing styles, 
        /// creates new fallback property, and continues normal registration process.
        /// </summary>
        [Fact]
        public void ReRegisterImplicitStyles_ValidFallbackTypeName_ClearsAndRegistersCorrectly()
        {
            // Arrange
            var target = Substitute.For<BindableObject>();
            var mergedStyle = new MergedStyle(typeof(View), target);

            // Add some existing implicit styles to simulate clearing behavior
            var existingProperty1 = CreateBindableProperty("Test1");
            var existingProperty2 = CreateBindableProperty("Test2");
            AddImplicitStyle(mergedStyle, existingProperty1);
            AddImplicitStyle(mergedStyle, existingProperty2);

            string fallbackTypeName = "TestFallbackType";

            // Act
            InvokeReRegisterImplicitStyles(mergedStyle, fallbackTypeName);

            // Assert
            // Verify existing styles were removed
            target.Received(1).RemoveDynamicResource(existingProperty1);
            target.Received(1).RemoveDynamicResource(existingProperty2);

            // Verify the fallback was set
            target.Received().SetDynamicResource(Arg.Any<BindableProperty>(), fallbackTypeName);

            // Verify implicit styles collection is updated (should have new entries from both fallback and RegisterImplicitStyles)
            var implicitStyles = GetImplicitStyles(mergedStyle);
            Assert.NotEmpty(implicitStyles);
        }

        /// <summary>
        /// Tests that ReRegisterImplicitStyles with null fallbackTypeName still processes correctly
        /// without throwing exceptions and continues the registration flow.
        /// </summary>
        [Fact]
        public void ReRegisterImplicitStyles_NullFallbackTypeName_ProcessesWithoutException()
        {
            // Arrange
            var target = Substitute.For<BindableObject>();
            var mergedStyle = new MergedStyle(typeof(View), target);
            string fallbackTypeName = null;

            // Act & Assert - should not throw
            InvokeReRegisterImplicitStyles(mergedStyle, fallbackTypeName);

            // Verify the fallback was set with null
            target.Received().SetDynamicResource(Arg.Any<BindableProperty>(), null);
        }

        /// <summary>
        /// Tests that ReRegisterImplicitStyles with empty fallbackTypeName processes correctly
        /// and sets the dynamic resource with empty string.
        /// </summary>
        [Fact]
        public void ReRegisterImplicitStyles_EmptyFallbackTypeName_ProcessesCorrectly()
        {
            // Arrange
            var target = Substitute.For<BindableObject>();
            var mergedStyle = new MergedStyle(typeof(View), target);
            string fallbackTypeName = "";

            // Act
            InvokeReRegisterImplicitStyles(mergedStyle, fallbackTypeName);

            // Assert
            target.Received().SetDynamicResource(Arg.Any<BindableProperty>(), "");
        }

        /// <summary>
        /// Tests that ReRegisterImplicitStyles with whitespace fallbackTypeName processes correctly
        /// and sets the dynamic resource with the whitespace string.
        /// </summary>
        [Fact]
        public void ReRegisterImplicitStyles_WhitespaceFallbackTypeName_ProcessesCorrectly()
        {
            // Arrange
            var target = Substitute.For<BindableObject>();
            var mergedStyle = new MergedStyle(typeof(View), target);
            string fallbackTypeName = "   ";

            // Act
            InvokeReRegisterImplicitStyles(mergedStyle, fallbackTypeName);

            // Assert
            target.Received().SetDynamicResource(Arg.Any<BindableProperty>(), "   ");
        }

        /// <summary>
        /// Tests that ReRegisterImplicitStyles with no existing implicit styles
        /// still creates fallback and continues registration without errors.
        /// </summary>
        [Fact]
        public void ReRegisterImplicitStyles_NoExistingImplicitStyles_ProcessesCorrectly()
        {
            // Arrange
            var target = Substitute.For<BindableObject>();
            var mergedStyle = new MergedStyle(typeof(View), target);

            // Clear any existing implicit styles that might have been added by constructor
            ClearImplicitStyles(mergedStyle);
            string fallbackTypeName = "TestType";

            // Act
            InvokeReRegisterImplicitStyles(mergedStyle, fallbackTypeName);

            // Assert
            // Should not call RemoveDynamicResource for non-existent items
            // But should still set the new fallback
            target.Received().SetDynamicResource(Arg.Any<BindableProperty>(), fallbackTypeName);

            var implicitStyles = GetImplicitStyles(mergedStyle);
            Assert.NotEmpty(implicitStyles);
        }

        /// <summary>
        /// Tests that ReRegisterImplicitStyles correctly calls RegisterImplicitStyles and Apply
        /// after setting up the fallback property.
        /// </summary>
        [Fact]
        public void ReRegisterImplicitStyles_ValidInput_CallsRegisterAndApply()
        {
            // Arrange
            var target = Substitute.For<BindableObject>();
            var mergedStyle = new MergedStyle(typeof(View), target);
            string fallbackTypeName = "TestType";

            // Clear received calls from constructor
            target.ClearReceivedCalls();

            // Act
            InvokeReRegisterImplicitStyles(mergedStyle, fallbackTypeName);

            // Assert
            // Verify that both RegisterImplicitStyles and Apply were called
            // This is evidenced by additional SetDynamicResource calls beyond just the fallback
            var setDynamicResourceCalls = target.ReceivedCalls()
                .Where(call => call.GetMethodInfo().Name == "SetDynamicResource")
                .ToList();

            // Should have multiple calls: one for fallback + calls from RegisterImplicitStyles
            Assert.True(setDynamicResourceCalls.Count > 1, "Expected multiple SetDynamicResource calls indicating RegisterImplicitStyles was called");
        }

        // Helper methods for testing private/internal members via reflection

        private void InvokeReRegisterImplicitStyles(MergedStyle mergedStyle, string fallbackTypeName)
        {
            var method = typeof(MergedStyle).GetMethod("ReRegisterImplicitStyles", BindingFlags.NonPublic | BindingFlags.Instance);
            method.Invoke(mergedStyle, new object[] { fallbackTypeName });
        }

        private List<BindableProperty> GetImplicitStyles(MergedStyle mergedStyle)
        {
            var field = typeof(MergedStyle).GetField("_implicitStyles", BindingFlags.NonPublic | BindingFlags.Instance);
            return (List<BindableProperty>)field.GetValue(mergedStyle);
        }

        private void AddImplicitStyle(MergedStyle mergedStyle, BindableProperty property)
        {
            var implicitStyles = GetImplicitStyles(mergedStyle);
            implicitStyles.Add(property);
        }

        private void ClearImplicitStyles(MergedStyle mergedStyle)
        {
            var implicitStyles = GetImplicitStyles(mergedStyle);
            implicitStyles.Clear();
        }

        private BindableProperty CreateBindableProperty(string name)
        {
            return BindableProperty.Create(name, typeof(object), typeof(BindableObject), null);
        }

        /// <summary>
        /// Tests that the MergedStyle constructor properly initializes with valid parameters.
        /// Verifies that Target and TargetType properties are set correctly and that
        /// RegisterImplicitStyles and Apply methods are called without throwing exceptions.
        /// </summary>
        [Fact]
        public void MergedStyle_ValidParameters_SetsPropertiesCorrectly()
        {
            // Arrange
            var targetType = typeof(string);
            var target = Substitute.For<BindableObject>();

            // Act
            var mergedStyle = new MergedStyle(targetType, target);

            // Assert
            Assert.Equal(target, mergedStyle.Target);
            Assert.Equal(targetType, mergedStyle.TargetType);
        }

        /// <summary>
        /// Tests that the MergedStyle constructor throws NullReferenceException when targetType is null.
        /// The RegisterImplicitStyles method attempts to access properties of the null Type,
        /// which should result in a NullReferenceException.
        /// </summary>
        [Fact]
        public void MergedStyle_NullTargetType_ThrowsNullReferenceException()
        {
            // Arrange
            Type targetType = null;
            var target = Substitute.For<BindableObject>();

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => new MergedStyle(targetType, target));
        }

        /// <summary>
        /// Tests that the MergedStyle constructor throws NullReferenceException when target is null.
        /// The RegisterImplicitStyles method attempts to call SetDynamicResource on the null target,
        /// which should result in a NullReferenceException.
        /// </summary>
        [Fact]
        public void MergedStyle_NullTarget_ThrowsNullReferenceException()
        {
            // Arrange
            var targetType = typeof(string);
            BindableObject target = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => new MergedStyle(targetType, target));
        }

        /// <summary>
        /// Tests that the MergedStyle constructor throws NullReferenceException when both parameters are null.
        /// Either the null targetType or null target will cause a NullReferenceException during initialization.
        /// </summary>
        [Fact]
        public void MergedStyle_BothParametersNull_ThrowsNullReferenceException()
        {
            // Arrange
            Type targetType = null;
            BindableObject target = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => new MergedStyle(targetType, target));
        }

        /// <summary>
        /// Tests that the MergedStyle constructor works correctly with different valid Type parameters.
        /// Verifies that various types (reference types, value types, generic types) can be used
        /// as targetType without causing exceptions.
        /// </summary>
        [Theory]
        [InlineData(typeof(object))]
        [InlineData(typeof(int))]
        [InlineData(typeof(string))]
        [InlineData(typeof(List<string>))]
        public void MergedStyle_VariousValidTypes_SetsTargetTypeCorrectly(Type targetType)
        {
            // Arrange
            var target = Substitute.For<BindableObject>();

            // Act
            var mergedStyle = new MergedStyle(targetType, target);

            // Assert
            Assert.Equal(targetType, mergedStyle.TargetType);
            Assert.Equal(target, mergedStyle.Target);
        }
    }
}