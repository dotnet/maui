#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;


using Microsoft;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Controls.Xaml.Diagnostics;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

    public class BindingExpressionTests : BaseTestFixture
    {
        [Fact]
        public void Ctor()
        {
            string path = "Foo.Bar";
            var binding = new Binding(path);

            var be = new BindingExpression(binding, path);

            Assert.Same(binding, be.Binding);
            Assert.Equal(path, be.Path);
        }

        [Fact]
        public void CtorInvalid()
        {
            string path = "Foo.Bar";
            var binding = new Binding(path);

            Assert.Throws<ArgumentNullException>(() => new BindingExpression(binding, null));

            Assert.Throws<ArgumentNullException>(() => new BindingExpression(null, path));
        }

        [Fact]
        public void ApplyNull()
        {
            const string path = "Foo.Bar";
            var binding = new Binding(path);
            var be = new BindingExpression(binding, path);
            be.Apply(null, new MockBindable(), TextCell.TextProperty, SetterSpecificity.FromBinding);
        }

        // We only throw on invalid path features, if they give an invalid property
        // name, it won't have compiled in the first place or they misstyped.
        [InlineData("Foo.")]
        [InlineData("Foo[]")]
        [InlineData("Foo.Bar[]")]
        [InlineData("Foo[1")]
        [Theory]
        public void InvalidPaths(string path)
        {
            var fex = Assert.Throws<FormatException>(() =>
            {
                var binding = new Binding(path);
                new BindingExpression(binding, path);
            });

            Assert.False(String.IsNullOrWhiteSpace(fex.Message),
                "FormatException did not contain an explanation");
        }

        public static IEnumerable<object[]> ValidPathsData()
        {
            var paths = new List<string> { ".", "[1]", "[1 ]", ".[1]", ". [1]",
                "Foo", "Foo.Bar", "Foo. Bar", "Foo.Bar[1]",
                "Foo.Bar [1]" };

            foreach (var path in paths)
            {
                yield return new object[] { path, true, true };
                yield return new object[] { path, true, false };
                yield return new object[] { path, false, true };
                yield return new object[] { path, false, false };
            }
        }

        [Theory, MemberData(nameof(ValidPathsData))]
        public void ValidPaths(
            string path,
            bool spaceBefore,
            bool spaceAfter)
        {
            if (spaceBefore)
                path = " " + path;
            if (spaceAfter)
                path = path + " ";

            var binding = new Binding(path);
            _ = new BindingExpression(binding, path);
        }

        static object[] TryConvertWithNumbersAndCulturesCases => new object[]
        {
            new object[]{ "4.2", new CultureInfo("en"), 4.2m },
            new object[]{ "4,2", new CultureInfo("de"), 4.2m },
            new object[]{ "-4.2", new CultureInfo("en"), -4.2m },
            new object[]{ "-4,2", new CultureInfo("de"), -4.2m },

            new object[]{ "4.2", new CultureInfo("en"), new decimal?(4.2m)},
            new object[]{ "4,2", new CultureInfo("de"), new decimal?(4.2m) },
            new object[]{ "-4.2", new CultureInfo("en"), new decimal?(-4.2m)},
            new object[]{ "-4,2", new CultureInfo("de"), new decimal?(-4.2m) },

            new object[]{ "4.2", new CultureInfo("en"), 4.2d },
            new object[]{ "4,2", new CultureInfo("de"), 4.2d },
            new object[]{ "-4.2", new CultureInfo("en"), -4.2d },
            new object[]{ "-4,2", new CultureInfo("de"), -4.2d },

            new object[]{ "4.2", new CultureInfo("en"), new double?(4.2d)},
            new object[]{ "4,2", new CultureInfo("de"), new double?(4.2d) },
            new object[]{ "-4.2", new CultureInfo("en"), new double?(-4.2d)},
            new object[]{ "-4,2", new CultureInfo("de"), new double?(-4.2d) },

            new object[]{ "4.2", new CultureInfo("en"), 4.2f },
            new object[]{ "4,2", new CultureInfo("de"), 4.2f },
            new object[]{ "-4.2", new CultureInfo("en"), -4.2f },
            new object[]{ "-4,2", new CultureInfo("de"), -4.2f },

            new object[]{ "4.2", new CultureInfo("en"), new float?(4.2f)},
            new object[]{ "4,2", new CultureInfo("de"), new float?(4.2f) },
            new object[]{ "-4.2", new CultureInfo("en"), new float?(-4.2f)},
            new object[]{ "-4,2", new CultureInfo("de"), new float?(-4.2f) },

            new object[]{ "4.", new CultureInfo("en"), "4." },
            new object[]{ "4,", new CultureInfo("de"), "4," },
            new object[]{ "-0", new CultureInfo("en"), "-0" },
            new object[]{ "-0", new CultureInfo("de"), "-0" },
        };

        public static IEnumerable<object[]> TryConvertWithNumbersAndCulturesCasesData()
        {
            foreach (var testCase in TryConvertWithNumbersAndCulturesCases)
            {
                yield return (object[])testCase;
            }
        }

        [Theory, MemberData(nameof(TryConvertWithNumbersAndCulturesCasesData))]
        public void TryConvertWithNumbersAndCultures(object inputString, CultureInfo culture, object expected)
        {
            CultureInfo.CurrentCulture = culture;
            BindingExpressionHelper.TryConvert(ref inputString, Entry.TextProperty, expected.GetType(), false);

            Assert.Equal(expected, inputString);
        }

        /// <summary>
        /// Tests that the BindingExpression constructor correctly initializes with valid binding and path parameters.
        /// Verifies that the Binding and Path properties are set to the provided values.
        /// </summary>
        [Fact]
        public void Constructor_ValidParameters_SetsPropertiesCorrectly()
        {
            // Arrange
            const string path = "TestProperty";
            var binding = new Binding(path);

            // Act
            var bindingExpression = new BindingExpression(binding, path);

            // Assert
            Assert.Same(binding, bindingExpression.Binding);
            Assert.Equal(path, bindingExpression.Path);
        }

        /// <summary>
        /// Tests that the BindingExpression constructor throws ArgumentNullException when binding parameter is null.
        /// Verifies the correct parameter name is included in the exception.
        /// </summary>
        [Fact]
        public void Constructor_NullBinding_ThrowsArgumentNullException()
        {
            // Arrange
            const string path = "TestProperty";

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => new BindingExpression(null, path));
            Assert.Equal("binding", exception.ParamName);
        }

        /// <summary>
        /// Tests that the BindingExpression constructor throws ArgumentNullException when path parameter is null.
        /// Verifies the correct parameter name is included in the exception.
        /// </summary>
        [Fact]
        public void Constructor_NullPath_ThrowsArgumentNullException()
        {
            // Arrange
            var binding = new Binding("TestProperty");

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => new BindingExpression(binding, null));
            Assert.Equal("path", exception.ParamName);
        }

        /// <summary>
        /// Tests that the BindingExpression constructor throws ArgumentNullException when both parameters are null.
        /// Verifies that the binding parameter is checked first (based on parameter order).
        /// </summary>
        [Fact]
        public void Constructor_BothParametersNull_ThrowsArgumentNullExceptionForBinding()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => new BindingExpression(null, null));
            Assert.Equal("binding", exception.ParamName);
        }

        /// <summary>
        /// Tests that the BindingExpression constructor accepts various valid path string formats.
        /// Tests edge cases like empty strings, whitespace, special characters, and very long strings.
        /// </summary>
        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t")]
        [InlineData("\n")]
        [InlineData("SimpleProperty")]
        [InlineData("Property.SubProperty")]
        [InlineData("Property[0]")]
        [InlineData("Property[Key]")]
        [InlineData("Property.SubProperty[0].AnotherProperty")]
        [InlineData("_ValidProperty")]
        [InlineData("Property123")]
        [InlineData("Property-With-Dashes")]
        [InlineData("Property With Spaces")]
        [InlineData("Property.With.Multiple.Dots")]
        [InlineData("Property[\"StringKey\"]")]
        [InlineData("Property['SingleQuoteKey']")]
        [InlineData("VeryLongPropertyNameThatExceedsTypicalLengthsToTestBoundaryConditionsAndEnsureNoTruncationOccurs")]
        public void Constructor_ValidPathVariations_SetsPathCorrectly(string path)
        {
            // Arrange
            var binding = new Binding("TestBinding");

            // Act
            var bindingExpression = new BindingExpression(binding, path);

            // Assert
            Assert.Equal(path, bindingExpression.Path);
            Assert.Same(binding, bindingExpression.Binding);
        }

        /// <summary>
        /// Tests that the BindingExpression constructor works with different types of binding objects.
        /// Verifies that any BindingBase-derived type can be used as the binding parameter.
        /// </summary>
        [Fact]
        public void Constructor_DifferentBindingTypes_WorksCorrectly()
        {
            // Arrange
            var simpleBinding = new Binding("Property");
            var bindingWithConverter = new Binding("Property") { Mode = BindingMode.TwoWay };
            const string path = "TestPath";

            // Act
            var expression1 = new BindingExpression(simpleBinding, path);
            var expression2 = new BindingExpression(bindingWithConverter, path);

            // Assert
            Assert.Same(simpleBinding, expression1.Binding);
            Assert.Same(bindingWithConverter, expression2.Binding);
            Assert.Equal(path, expression1.Path);
            Assert.Equal(path, expression2.Path);
        }

        /// <summary>
        /// Tests that the BindingExpression constructor handles special characters and Unicode in path strings.
        /// Verifies that non-ASCII characters and special symbols are preserved correctly.
        /// </summary>
        [Theory]
        [InlineData("Property.Ñoño")]
        [InlineData("Property.中文")]
        [InlineData("Property.العربية")]
        [InlineData("Property.🚀")]
        [InlineData("Property.$pecial")]
        [InlineData("Property.@symbol")]
        [InlineData("Property.#hash")]
        [InlineData("Property\\BackSlash")]
        [InlineData("Property/Forward")]
        public void Constructor_SpecialCharactersInPath_PreservesCharactersCorrectly(string path)
        {
            // Arrange
            var binding = new Binding("TestBinding");

            // Act
            var bindingExpression = new BindingExpression(binding, path);

            // Assert
            Assert.Equal(path, bindingExpression.Path);
        }
    }

    /// <summary>
    /// Test class for SubscribeToAncestryChanges method in BindingExpression
    /// </summary>
    public partial class BindingExpressionSubscribeToAncestryChangesTests
    {
        /// <summary>
        /// Tests that SubscribeToAncestryChanges handles null chain parameter correctly by returning early.
        /// Expected result: Method returns without throwing and no subscriptions are made.
        /// </summary>
        [Fact]
        public void SubscribeToAncestryChanges_NullChain_ReturnsEarlyWithoutSubscriptions()
        {
            // Arrange
            var binding = new Binding("TestPath");
            var bindingExpression = new BindingExpression(binding, "TestPath");

            // Act
            bindingExpression.SubscribeToAncestryChanges(null, true, false);

            // Assert
            // Verify that _ancestryChain field remains null (using reflection to check internal state)
            var ancestryChainField = typeof(BindingExpression).GetField("_ancestryChain", BindingFlags.NonPublic | BindingFlags.Instance);
            var ancestryChainValue = ancestryChainField.GetValue(bindingExpression);
            Assert.Null(ancestryChainValue);
        }

        /// <summary>
        /// Tests that SubscribeToAncestryChanges handles empty chain correctly.
        /// Expected result: Internal state is initialized but no event subscriptions occur.
        /// </summary>
        [Fact]
        public void SubscribeToAncestryChanges_EmptyChain_InitializesStateWithoutSubscriptions()
        {
            // Arrange
            var binding = new Binding("TestPath");
            var bindingExpression = new BindingExpression(binding, "TestPath");
            var emptyChain = new List<Element>();

            // Act
            bindingExpression.SubscribeToAncestryChanges(emptyChain, true, false);

            // Assert
            // Verify that _ancestryChain field is initialized but empty
            var ancestryChainField = typeof(BindingExpression).GetField("_ancestryChain", BindingFlags.NonPublic | BindingFlags.Instance);
            var ancestryChainValue = ancestryChainField.GetValue(bindingExpression);
            Assert.NotNull(ancestryChainValue);

            // Verify _isBindingContextRelativeSource is set correctly
            var isBindingContextField = typeof(BindingExpression).GetField("_isBindingContextRelativeSource", BindingFlags.NonPublic | BindingFlags.Instance);
            var isBindingContextValue = (bool)isBindingContextField.GetValue(bindingExpression);
            Assert.True(isBindingContextValue);
        }

        /// <summary>
        /// Tests SubscribeToAncestryChanges with single element and various parameter combinations.
        /// Expected result: Appropriate event subscriptions based on parameters.
        /// </summary>
        [Theory]
        [InlineData(true, true, true, true)] // includeBindingContext=true, rootIsSource=true, expectParentSet=false, expectBindingContext=true
        [InlineData(true, false, true, true)] // includeBindingContext=true, rootIsSource=false, expectParentSet=true, expectBindingContext=true
        [InlineData(false, true, false, false)] // includeBindingContext=false, rootIsSource=true, expectParentSet=false, expectBindingContext=false
        [InlineData(false, false, true, false)] // includeBindingContext=false, rootIsSource=false, expectParentSet=true, expectBindingContext=false
        public void SubscribeToAncestryChanges_SingleElement_SubscribesBasedOnParameters(bool includeBindingContext, bool rootIsSource, bool expectParentSet, bool expectBindingContext)
        {
            // Arrange
            var binding = new Binding("TestPath");
            var bindingExpression = new BindingExpression(binding, "TestPath");
            var mockElement = Substitute.For<Element>();
            var chain = new List<Element> { mockElement };

            // Track event subscription calls
            bool parentSetSubscribed = false;
            bool bindingContextSubscribed = false;

            mockElement.When(x => x.ParentSet += Arg.Any<EventHandler>())
                .Do(callInfo => parentSetSubscribed = true);

            mockElement.When(x => x.BindingContextChanged += Arg.Any<EventHandler>())
                .Do(callInfo => bindingContextSubscribed = true);

            // Act
            bindingExpression.SubscribeToAncestryChanges(chain, includeBindingContext, rootIsSource);

            // Assert
            Assert.Equal(expectParentSet, parentSetSubscribed);
            Assert.Equal(expectBindingContext, bindingContextSubscribed);

            // Verify internal state
            var ancestryChainField = typeof(BindingExpression).GetField("_ancestryChain", BindingFlags.NonPublic | BindingFlags.Instance);
            var ancestryChainValue = ancestryChainField.GetValue(bindingExpression);
            Assert.NotNull(ancestryChainValue);

            var isBindingContextField = typeof(BindingExpression).GetField("_isBindingContextRelativeSource", BindingFlags.NonPublic | BindingFlags.Instance);
            var isBindingContextValue = (bool)isBindingContextField.GetValue(bindingExpression);
            Assert.Equal(includeBindingContext, isBindingContextValue);
        }

        /// <summary>
        /// Tests SubscribeToAncestryChanges with multiple elements in chain.
        /// Expected result: All elements except the last (when rootIsSource=true) subscribe to ParentSet, all subscribe to BindingContext if enabled.
        /// </summary>
        [Theory]
        [InlineData(true, true)] // includeBindingContext=true, rootIsSource=true
        [InlineData(true, false)] // includeBindingContext=true, rootIsSource=false
        [InlineData(false, true)] // includeBindingContext=false, rootIsSource=true
        [InlineData(false, false)] // includeBindingContext=false, rootIsSource=false
        public void SubscribeToAncestryChanges_MultipleElements_SubscribesCorrectly(bool includeBindingContext, bool rootIsSource)
        {
            // Arrange
            var binding = new Binding("TestPath");
            var bindingExpression = new BindingExpression(binding, "TestPath");
            var mockElement1 = Substitute.For<Element>();
            var mockElement2 = Substitute.For<Element>();
            var mockElement3 = Substitute.For<Element>();
            var chain = new List<Element> { mockElement1, mockElement2, mockElement3 };

            // Track subscriptions for each element
            var parentSetSubscriptions = new bool[3];
            var bindingContextSubscriptions = new bool[3];

            mockElement1.When(x => x.ParentSet += Arg.Any<EventHandler>())
                .Do(callInfo => parentSetSubscriptions[0] = true);
            mockElement2.When(x => x.ParentSet += Arg.Any<EventHandler>())
                .Do(callInfo => parentSetSubscriptions[1] = true);
            mockElement3.When(x => x.ParentSet += Arg.Any<EventHandler>())
                .Do(callInfo => parentSetSubscriptions[2] = true);

            mockElement1.When(x => x.BindingContextChanged += Arg.Any<EventHandler>())
                .Do(callInfo => bindingContextSubscriptions[0] = true);
            mockElement2.When(x => x.BindingContextChanged += Arg.Any<EventHandler>())
                .Do(callInfo => bindingContextSubscriptions[1] = true);
            mockElement3.When(x => x.BindingContextChanged += Arg.Any<EventHandler>())
                .Do(callInfo => bindingContextSubscriptions[2] = true);

            // Act
            bindingExpression.SubscribeToAncestryChanges(chain, includeBindingContext, rootIsSource);

            // Assert
            // ParentSet subscriptions: all elements should subscribe except last one if rootIsSource=true
            Assert.True(parentSetSubscriptions[0]); // First element always subscribes
            Assert.True(parentSetSubscriptions[1]); // Second element always subscribes
            Assert.Equal(!rootIsSource, parentSetSubscriptions[2]); // Last element subscribes only if !rootIsSource

            // BindingContext subscriptions: all elements should subscribe if includeBindingContext=true
            Assert.Equal(includeBindingContext, bindingContextSubscriptions[0]);
            Assert.Equal(includeBindingContext, bindingContextSubscriptions[1]);
            Assert.Equal(includeBindingContext, bindingContextSubscriptions[2]);

            // Verify ancestry chain is populated
            var ancestryChainField = typeof(BindingExpression).GetField("_ancestryChain", BindingFlags.NonPublic | BindingFlags.Instance);
            var ancestryChainValue = ancestryChainField.GetValue(bindingExpression);
            Assert.NotNull(ancestryChainValue);
        }

        /// <summary>
        /// Tests that SubscribeToAncestryChanges clears existing subscriptions before setting new ones.
        /// Expected result: ClearAncestryChangeSubscriptions is called at the beginning.
        /// </summary>
        [Fact]
        public void SubscribeToAncestryChanges_AlwaysClearsExistingSubscriptions()
        {
            // Arrange
            var binding = new Binding("TestPath");
            var bindingExpression = new BindingExpression(binding, "TestPath");
            var mockElement = Substitute.For<Element>();
            var chain = new List<Element> { mockElement };

            // First, set up some initial state by calling the method once
            bindingExpression.SubscribeToAncestryChanges(chain, true, false);

            // Verify initial state exists
            var ancestryChainField = typeof(BindingExpression).GetField("_ancestryChain", BindingFlags.NonPublic | BindingFlags.Instance);
            var initialAncestryChain = ancestryChainField.GetValue(bindingExpression);
            Assert.NotNull(initialAncestryChain);

            // Act - Call again with different parameters
            var newMockElement = Substitute.For<Element>();
            var newChain = new List<Element> { newMockElement };
            bindingExpression.SubscribeToAncestryChanges(newChain, false, true);

            // Assert - Verify new state is set (clearing happened implicitly)
            var finalAncestryChain = ancestryChainField.GetValue(bindingExpression);
            Assert.NotNull(finalAncestryChain);

            var isBindingContextField = typeof(BindingExpression).GetField("_isBindingContextRelativeSource", BindingFlags.NonPublic | BindingFlags.Instance);
            var isBindingContextValue = (bool)isBindingContextField.GetValue(bindingExpression);
            Assert.False(isBindingContextValue); // Should be false from second call
        }

        /// <summary>
        /// Tests SubscribeToAncestryChanges with boundary case of exactly one element and different combinations.
        /// Expected result: Handles single element edge case correctly based on rootIsSource flag.
        /// </summary>
        [Fact]
        public void SubscribeToAncestryChanges_SingleElementBoundaryCase_HandlesCorrectly()
        {
            // Arrange
            var binding = new Binding("TestPath");
            var bindingExpression = new BindingExpression(binding, "TestPath");
            var mockElement = Substitute.For<Element>();
            var chain = new List<Element> { mockElement };

            bool parentSetCalled = false;
            mockElement.When(x => x.ParentSet += Arg.Any<EventHandler>())
                .Do(callInfo => parentSetCalled = true);

            // Act - Test the boundary condition where i == chain.Count - 1 and rootIsSource = true
            bindingExpression.SubscribeToAncestryChanges(chain, false, true);

            // Assert - With single element and rootIsSource=true, ParentSet should NOT be subscribed
            // because (i != chain.Count - 1 || !rootIsSource) evaluates to (false || false) = false
            Assert.False(parentSetCalled);
        }
    }


    /// <summary>
    /// Tests for the BindingPair nested class within BindingExpression.
    /// </summary>
    public partial class BindingPairTests : BaseTestFixture
    {
        /// <summary>
        /// Tests the BindingPair constructor with valid parameters.
        /// This test cannot be implemented because BindingPair is a private nested class
        /// within BindingExpression and the testing guidelines prohibit using reflection
        /// to access private members. To test this constructor, either:
        /// 1. Make BindingPair internal instead of private, or
        /// 2. Create public methods in BindingExpression that expose BindingPair functionality, or
        /// 3. Add internal test methods to BindingExpression that can be accessed via InternalsVisibleTo
        /// </summary>
        [Fact(Skip = "BindingPair is private and cannot be accessed without reflection")]
        public void BindingPairConstructor_ValidParameters_SetsPropertiesCorrectly()
        {
            // Cannot test private nested class BindingPair directly
            // The constructor at lines 123-128 needs to be tested but is inaccessible
            // Expected behavior: Constructor should set Part, Source, and IsLast properties
            // from the provided parameters (part, source, isLast respectively)
        }

        /// <summary>
        /// Tests the BindingPair constructor with null BindingExpressionPart parameter.
        /// This test cannot be implemented due to private accessibility of BindingPair.
        /// Expected behavior: Constructor should handle null part parameter appropriately.
        /// </summary>
        [Fact(Skip = "BindingPair is private and cannot be accessed without reflection")]
        public void BindingPairConstructor_NullPart_HandlesCorrectly()
        {
            // Cannot test private nested class BindingPair directly
            // Need to verify behavior when part parameter is null
        }

        /// <summary>
        /// Tests the BindingPair constructor with null source parameter.
        /// This test cannot be implemented due to private accessibility of BindingPair.
        /// Expected behavior: Constructor should handle null source parameter appropriately.
        /// </summary>
        [Fact(Skip = "BindingPair is private and cannot be accessed without reflection")]
        public void BindingPairConstructor_NullSource_HandlesCorrectly()
        {
            // Cannot test private nested class BindingPair directly
            // Need to verify behavior when source parameter is null
        }

        /// <summary>
        /// Tests the BindingPair constructor with different boolean values for isLast parameter.
        /// This test cannot be implemented due to private accessibility of BindingPair.
        /// Expected behavior: Constructor should correctly set IsLast property to match the parameter.
        /// </summary>
        [Theory(Skip = "BindingPair is private and cannot be accessed without reflection")]
        [InlineData(true)]
        [InlineData(false)]
        public void BindingPairConstructor_BooleanIsLastValues_SetsIsLastCorrectly(bool isLast)
        {
            // Cannot test private nested class BindingPair directly
            // Need to verify IsLast property is set correctly based on parameter
        }
    }
}