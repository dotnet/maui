#nullable disable

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using Microsoft.Maui.Controls;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public partial class RoutingTests
    {
        /// <summary>
        /// Tests that IsDefault(BindableObject) throws ArgumentNullException when source is null.
        /// </summary>
        [Fact]
        public void IsDefault_BindableObject_NullSource_ThrowsNullReferenceException()
        {
            // Arrange
            BindableObject source = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => Routing.IsDefault(source));
        }

        /// <summary>
        /// Tests that IsDefault(BindableObject) returns true when the BindableObject has a route starting with default prefix.
        /// </summary>
        [Fact]
        public void IsDefault_BindableObject_DefaultRoute_ReturnsTrue()
        {
            // Arrange
            var source = Substitute.For<BindableObject>();
            source.GetValue(Routing.RouteProperty).Returns("D_FAULT_TestRoute");

            // Act
            var result = Routing.IsDefault(source);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that IsDefault(BindableObject) returns false when the BindableObject has a route not starting with default prefix.
        /// </summary>
        [Fact]
        public void IsDefault_BindableObject_NonDefaultRoute_ReturnsFalse()
        {
            // Arrange
            var source = Substitute.For<BindableObject>();
            source.GetValue(Routing.RouteProperty).Returns("SomeRegularRoute");

            // Act
            var result = Routing.IsDefault(source);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that IsDefault(BindableObject) returns false when the BindableObject has an empty route.
        /// </summary>
        [Fact]
        public void IsDefault_BindableObject_EmptyRoute_ReturnsFalse()
        {
            // Arrange
            var source = Substitute.For<BindableObject>();
            source.GetValue(Routing.RouteProperty).Returns(string.Empty);

            // Act
            var result = Routing.IsDefault(source);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that IsDefault(BindableObject) returns false when the BindableObject has an implicit route.
        /// </summary>
        [Fact]
        public void IsDefault_BindableObject_ImplicitRoute_ReturnsFalse()
        {
            // Arrange
            var source = Substitute.For<BindableObject>();
            source.GetValue(Routing.RouteProperty).Returns("IMPL_TestRoute");

            // Act
            var result = Routing.IsDefault(source);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that IsDefault(BindableObject) throws NullReferenceException when the BindableObject has a null route.
        /// </summary>
        [Fact]
        public void IsDefault_BindableObject_NullRoute_ThrowsNullReferenceException()
        {
            // Arrange
            var source = Substitute.For<BindableObject>();
            source.GetValue(Routing.RouteProperty).Returns(null);

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => Routing.IsDefault(source));
        }

        /// <summary>
        /// Tests IsDefault(BindableObject) with various route values using parameterized test data.
        /// </summary>
        /// <param name="route">The route value to test</param>
        /// <param name="expectedResult">The expected result from IsDefault</param>
        [Theory]
        [InlineData("D_FAULT_", true)]
        [InlineData("D_FAULT_Page1", true)]
        [InlineData("D_FAULT_SomeVeryLongPageName123", true)]
        [InlineData("UserDefinedRoute", false)]
        [InlineData("IMPL_Route", false)]
        [InlineData("SomeRoute", false)]
        [InlineData("d_fault_", false)] // Case sensitive
        [InlineData("D_FAULTRoute", false)] // No underscore after prefix
        [InlineData("PrefixD_FAULT_", false)] // Prefix in middle
        [InlineData(" D_FAULT_", false)] // Leading whitespace
        [InlineData("D_FAULT_ ", true)] // Trailing whitespace after prefix is valid
        public void IsDefault_BindableObject_VariousRoutes_ReturnsExpectedResult(string route, bool expectedResult)
        {
            // Arrange
            var source = Substitute.For<BindableObject>();
            source.GetValue(Routing.RouteProperty).Returns(route);

            // Act
            var result = Routing.IsDefault(source);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        /// <summary>
        /// Tests that GetRoutePathIfNotImplicit throws ArgumentNullException when passed a null Element object.
        /// Input: null Element object.
        /// Expected result: ArgumentNullException is thrown.
        /// </summary>
        [Fact]
        public void GetRoutePathIfNotImplicit_NullElement_ThrowsArgumentNullException()
        {
            // Arrange
            Element nullElement = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => Routing.GetRoutePathIfNotImplicit(nullElement));
        }

        /// <summary>
        /// Tests that GetRoutePathIfNotImplicit throws ArgumentNullException when Element returns null route.
        /// Input: Element object with null route value.
        /// Expected result: ArgumentNullException is thrown when checking if route is implicit.
        /// </summary>
        [Fact]
        public void GetRoutePathIfNotImplicit_ElementWithNullRoute_ThrowsArgumentNullException()
        {
            // Arrange
            var mockElement = Substitute.For<Element>();
            mockElement.GetValue(Routing.RouteProperty).Returns((string)null);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => Routing.GetRoutePathIfNotImplicit(mockElement));
        }

        /// <summary>
        /// Tests that GetRoutePathIfNotImplicit returns empty string when Element has implicit route.
        /// Input: Element object with route starting with "IMPL_".
        /// Expected result: Empty string is returned.
        /// </summary>
        [Theory]
        [InlineData("IMPL_")]
        [InlineData("IMPL_test")]
        [InlineData("IMPL_MyPage")]
        [InlineData("IMPL_123")]
        public void GetRoutePathIfNotImplicit_ElementWithImplicitRoute_ReturnsEmptyString(string implicitRoute)
        {
            // Arrange
            var mockElement = Substitute.For<Element>();
            mockElement.GetValue(Routing.RouteProperty).Returns(implicitRoute);

            // Act
            var result = Routing.GetRoutePathIfNotImplicit(mockElement);

            // Assert
            Assert.Equal(string.Empty, result);
        }

        /// <summary>
        /// Tests that GetRoutePathIfNotImplicit returns route with trailing slash when Element has non-implicit route.
        /// Input: Element object with route not starting with "IMPL_".
        /// Expected result: Route string with trailing "/" is returned.
        /// </summary>
        [Theory]
        [InlineData("", "/")]
        [InlineData("myroute", "myroute/")]
        [InlineData("test", "test/")]
        [InlineData("MyPage", "MyPage/")]
        [InlineData("123", "123/")]
        [InlineData("route/with/slashes", "route/with/slashes/")]
        [InlineData("route-with-dashes", "route-with-dashes/")]
        [InlineData("route_with_underscores", "route_with_underscores/")]
        [InlineData("route.with.dots", "route.with.dots/")]
        [InlineData("route with spaces", "route with spaces/")]
        [InlineData("IMPLEMENTATION", "IMPLEMENTATION/")]
        [InlineData("impl_lowercase", "impl_lowercase/")]
        [InlineData("D_FAULT_test", "D_FAULT_test/")]
        public void GetRoutePathIfNotImplicit_ElementWithNonImplicitRoute_ReturnsRouteWithTrailingSlash(string route, string expectedResult)
        {
            // Arrange
            var mockElement = Substitute.For<Element>();
            mockElement.GetValue(Routing.RouteProperty).Returns(route);

            // Act
            var result = Routing.GetRoutePathIfNotImplicit(mockElement);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        /// <summary>
        /// Tests that GetRoutePathIfNotImplicit returns route with trailing slash for whitespace-only routes.
        /// Input: Element object with whitespace-only route.
        /// Expected result: Whitespace route string with trailing "/" is returned.
        /// </summary>
        [Theory]
        [InlineData(" ", " /")]
        [InlineData("  ", "  /")]
        [InlineData("\t", "\t/")]
        [InlineData("\n", "\n/")]
        [InlineData("\r\n", "\r\n/")]
        public void GetRoutePathIfNotImplicit_ElementWithWhitespaceRoute_ReturnsWhitespaceRouteWithTrailingSlash(string whitespaceRoute, string expectedResult)
        {
            // Arrange
            var mockElement = Substitute.For<Element>();
            mockElement.GetValue(Routing.RouteProperty).Returns(whitespaceRoute);

            // Act
            var result = Routing.GetRoutePathIfNotImplicit(mockElement);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        /// <summary>
        /// Tests that GetRoutePathIfNotImplicit returns route with trailing slash for routes with special characters.
        /// Input: Element object with route containing special characters.
        /// Expected result: Route string with special characters and trailing "/" is returned.
        /// </summary>
        [Theory]
        [InlineData("!@#$%^&*()", "!@#$%^&*()/")]
        [InlineData("route+test", "route+test/")]
        [InlineData("route=test", "route=test/")]
        [InlineData("route?test", "route?test/")]
        [InlineData("route&test", "route&test/")]
        [InlineData("route[test]", "route[test]/")]
        [InlineData("route{test}", "route{test}/")]
        [InlineData("route|test", "route|test/")]
        [InlineData("route\\test", "route\\test/")]
        public void GetRoutePathIfNotImplicit_ElementWithSpecialCharacterRoute_ReturnsRouteWithTrailingSlash(string specialRoute, string expectedResult)
        {
            // Arrange
            var mockElement = Substitute.For<Element>();
            mockElement.GetValue(Routing.RouteProperty).Returns(specialRoute);

            // Act
            var result = Routing.GetRoutePathIfNotImplicit(mockElement);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        /// <summary>
        /// Tests UnRegisterRoute with null route parameter.
        /// Should handle null input gracefully without throwing exceptions.
        /// </summary>
        [Fact]
        public void UnRegisterRoute_NullRoute_DoesNotThrow()
        {
            // Arrange
            Routing.Clear();

            // Act & Assert - Should not throw
            Routing.UnRegisterRoute(null);
        }

        /// <summary>
        /// Tests UnRegisterRoute with empty string route parameter.
        /// Should handle empty string input gracefully.
        /// </summary>
        [Fact]
        public void UnRegisterRoute_EmptyRoute_DoesNotThrow()
        {
            // Arrange
            Routing.Clear();

            // Act & Assert - Should not throw
            Routing.UnRegisterRoute(string.Empty);
        }

        /// <summary>
        /// Tests UnRegisterRoute with a route that exists in the registry.
        /// Should remove the route and reset the route keys cache to null.
        /// </summary>
        [Fact]
        public void UnRegisterRoute_ExistingRoute_RemovesRouteAndNullifiesCache()
        {
            // Arrange
            Routing.Clear();
            string testRoute = "testRoute";
            Routing.RegisterRoute(testRoute, typeof(ContentPage));

            // Force route keys to be populated
            var routeKeys = Routing.GetRouteKeys();
            Assert.Contains(testRoute, routeKeys);

            // Act
            Routing.UnRegisterRoute(testRoute);

            // Assert - Route should be removed and cache should be reset
            var content = Routing.GetOrCreateContent(testRoute);
            Assert.Null(content);

            // Verify that route keys cache was reset by checking it doesn't contain the removed route
            var newRouteKeys = Routing.GetRouteKeys();
            Assert.DoesNotContain(testRoute, newRouteKeys);
        }

        /// <summary>
        /// Tests UnRegisterRoute with a route that does not exist in the registry.
        /// Should not affect the route keys cache or throw exceptions.
        /// </summary>
        [Fact]
        public void UnRegisterRoute_NonExistentRoute_DoesNotAffectCache()
        {
            // Arrange
            Routing.Clear();
            string existingRoute = "existingRoute";
            string nonExistentRoute = "nonExistentRoute";
            Routing.RegisterRoute(existingRoute, typeof(ContentPage));

            // Force route keys to be populated
            var initialRouteKeys = Routing.GetRouteKeys();
            Assert.Contains(existingRoute, initialRouteKeys);

            // Act
            Routing.UnRegisterRoute(nonExistentRoute);

            // Assert - Existing route should still be there
            var content = Routing.GetOrCreateContent(existingRoute);
            Assert.NotNull(content);

            // Route keys should still contain the existing route
            var finalRouteKeys = Routing.GetRouteKeys();
            Assert.Contains(existingRoute, finalRouteKeys);
            Assert.DoesNotContain(nonExistentRoute, finalRouteKeys);
        }

        /// <summary>
        /// Tests UnRegisterRoute with various edge case string values.
        /// Should handle special characters, whitespace, and formatting edge cases gracefully.
        /// </summary>
        [Theory]
        [InlineData("   ")]                    // Whitespace only
        [InlineData("route with spaces")]      // Route with spaces
        [InlineData("route/with/slashes")]     // Route with path separators
        [InlineData("UPPERCASE_ROUTE")]        // Uppercase route
        [InlineData("route_with_underscores")] // Route with underscores
        [InlineData("route-with-dashes")]      // Route with dashes
        [InlineData("route.with.dots")]        // Route with dots
        [InlineData("very_long_route_name_that_exceeds_normal_expected_length_for_testing_purposes")] // Very long route
        public void UnRegisterRoute_EdgeCaseStrings_HandledGracefully(string route)
        {
            // Arrange
            Routing.Clear();

            // Act & Assert - Should not throw regardless of whether route exists
            Routing.UnRegisterRoute(route);
        }

        /// <summary>
        /// Tests UnRegisterRoute multiple times with the same route.
        /// Should handle multiple removal attempts gracefully without side effects.
        /// </summary>
        [Fact]
        public void UnRegisterRoute_MultipleRemovalAttempts_HandledGracefully()
        {
            // Arrange
            Routing.Clear();
            string testRoute = "testRoute";
            Routing.RegisterRoute(testRoute, typeof(ContentPage));

            // Act - Remove the same route multiple times
            Routing.UnRegisterRoute(testRoute);
            Routing.UnRegisterRoute(testRoute);
            Routing.UnRegisterRoute(testRoute);

            // Assert - Route should be gone and no exceptions thrown
            var content = Routing.GetOrCreateContent(testRoute);
            Assert.Null(content);
        }

        /// <summary>
        /// Test helper class that extends Element for testing purposes.
        /// </summary>
        public class TestElement : Element
        {
            public TestElement() { }
        }

        /// <summary>
        /// Test helper class that extends Element but has no parameterless constructor.
        /// </summary>
        public class ElementWithoutParameterlessConstructor : Element
        {
            public ElementWithoutParameterlessConstructor(string parameter) { }
        }

        /// <summary>
        /// Test helper class that does not inherit from Element.
        /// </summary>
        public class NonElementType
        {
            public NonElementType() { }
        }

        /// <summary>
        /// Abstract test class that extends Element.
        /// </summary>
        public abstract class AbstractTestElement : Element
        {
            public AbstractTestElement() { }
        }

        /// <summary>
        /// Tests that GetOrCreate successfully creates and returns an instance when given a valid concrete Element type.
        /// Input: Valid concrete Element type with parameterless constructor.
        /// Expected: Returns new instance of the Element type.
        /// </summary>
        [Fact]
        public void GetOrCreate_ValidConcreteElementType_ReturnsCreatedElement()
        {
            // Arrange
            var factory = new Routing.TypeRouteFactory(typeof(TestElement));

            // Act
            var result = factory.GetOrCreate();

            // Assert
            Assert.NotNull(result);
            Assert.IsType<TestElement>(result);
        }

        /// <summary>
        /// Tests that GetOrCreate throws an exception when given an abstract Element type.
        /// Input: Abstract Element type.
        /// Expected: Throws MemberAccessException or similar activation exception.
        /// </summary>
        [Fact]
        public void GetOrCreate_AbstractElementType_ThrowsException()
        {
            // Arrange
            var factory = new Routing.TypeRouteFactory(typeof(Element));

            // Act & Assert
            Assert.Throws<MemberAccessException>(() => factory.GetOrCreate());
        }

        /// <summary>
        /// Tests that GetOrCreate throws an exception when given an abstract test Element type.
        /// Input: Custom abstract Element type.
        /// Expected: Throws MemberAccessException or similar activation exception.
        /// </summary>
        [Fact]
        public void GetOrCreate_CustomAbstractElementType_ThrowsException()
        {
            // Arrange
            var factory = new Routing.TypeRouteFactory(typeof(AbstractTestElement));

            // Act & Assert
            Assert.Throws<MemberAccessException>(() => factory.GetOrCreate());
        }

        /// <summary>
        /// Tests that GetOrCreate throws an exception when given a type without a parameterless constructor.
        /// Input: Element type that only has parameterized constructors.
        /// Expected: Throws MissingMethodException.
        /// </summary>
        [Fact]
        public void GetOrCreate_TypeWithoutParameterlessConstructor_ThrowsException()
        {
            // Arrange
            var factory = new Routing.TypeRouteFactory(typeof(ElementWithoutParameterlessConstructor));

            // Act & Assert
            Assert.Throws<MissingMethodException>(() => factory.GetOrCreate());
        }

        /// <summary>
        /// Tests that GetOrCreate throws InvalidCastException when given a type that doesn't inherit from Element.
        /// Input: Type that does not inherit from Element.
        /// Expected: Throws InvalidCastException during cast to Element.
        /// </summary>
        [Fact]
        public void GetOrCreate_NonElementType_ThrowsInvalidCastException()
        {
            // Arrange
            var factory = new Routing.TypeRouteFactory(typeof(NonElementType));

            // Act & Assert
            Assert.Throws<InvalidCastException>(() => factory.GetOrCreate());
        }

        /// <summary>
        /// Tests that GetOrCreate throws ArgumentNullException when given a null type.
        /// Input: Null type passed to Activator.CreateInstance.
        /// Expected: Throws ArgumentNullException.
        /// </summary>
        [Fact]
        public void GetOrCreate_NullType_ThrowsArgumentNullException()
        {
            // Arrange
            // Note: This test uses reflection to bypass the constructor validation
            // since TypeRouteFactory constructor would normally prevent null types
            var factory = new Routing.TypeRouteFactory(typeof(TestElement));
            var typeField = typeof(Routing.TypeRouteFactory).GetField("_type",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            typeField.SetValue(factory, null);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => factory.GetOrCreate());
        }

        /// <summary>
        /// Tests that GetOrCreate works with different concrete Element types.
        /// Input: Various concrete Element types.
        /// Expected: Returns instances of the respective types.
        /// </summary>
        [Theory]
        [InlineData(typeof(TestElement))]
        public void GetOrCreate_VariousConcreteElementTypes_ReturnsCorrectType(Type elementType)
        {
            // Arrange
            var factory = new Routing.TypeRouteFactory(elementType);

            // Act
            var result = factory.GetOrCreate();

            // Assert
            Assert.NotNull(result);
            Assert.IsType(elementType, result);
            Assert.IsAssignableFrom<Element>(result);
        }

        /// <summary>
        /// Tests that TypeRouteFactory.Equals returns true when comparing the same instance with itself.
        /// </summary>
        [Fact]
        public void TypeRouteFactory_Equals_SameInstance_ReturnsTrue()
        {
            // Arrange
            var factory = new Routing.TypeRouteFactory(typeof(string));

            // Act
            bool result = factory.Equals(factory);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that TypeRouteFactory.Equals returns true when comparing two instances with the same type.
        /// </summary>
        [Fact]
        public void TypeRouteFactory_Equals_SameType_ReturnsTrue()
        {
            // Arrange
            var factory1 = new Routing.TypeRouteFactory(typeof(string));
            var factory2 = new Routing.TypeRouteFactory(typeof(string));

            // Act
            bool result = factory1.Equals(factory2);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that TypeRouteFactory.Equals returns false when comparing instances with different types.
        /// </summary>
        [Fact]
        public void TypeRouteFactory_Equals_DifferentTypes_ReturnsFalse()
        {
            // Arrange
            var factory1 = new Routing.TypeRouteFactory(typeof(string));
            var factory2 = new Routing.TypeRouteFactory(typeof(int));

            // Act
            bool result = factory1.Equals(factory2);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that TypeRouteFactory.Equals returns false when comparing with null.
        /// </summary>
        [Fact]
        public void TypeRouteFactory_Equals_Null_ReturnsFalse()
        {
            // Arrange
            var factory = new Routing.TypeRouteFactory(typeof(string));

            // Act
            bool result = factory.Equals(null);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that TypeRouteFactory.Equals returns false when comparing with a different object type.
        /// </summary>
        [Fact]
        public void TypeRouteFactory_Equals_DifferentObjectType_ReturnsFalse()
        {
            // Arrange
            var factory = new Routing.TypeRouteFactory(typeof(string));
            var differentObject = new object();

            // Act
            bool result = factory.Equals(differentObject);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that TypeRouteFactory.Equals returns false when comparing with a string object.
        /// </summary>
        [Fact]
        public void TypeRouteFactory_Equals_StringObject_ReturnsFalse()
        {
            // Arrange
            var factory = new Routing.TypeRouteFactory(typeof(string));
            string stringObject = "test";

            // Act
            bool result = factory.Equals(stringObject);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that TypeRouteFactory.Equals works correctly with various built-in types.
        /// Input conditions: Different built-in types including primitive types, reference types, and special types.
        /// Expected result: Returns true for same types, false for different types.
        /// </summary>
        [Theory]
        [InlineData(typeof(int), typeof(int), true)]
        [InlineData(typeof(string), typeof(string), true)]
        [InlineData(typeof(object), typeof(object), true)]
        [InlineData(typeof(double), typeof(double), true)]
        [InlineData(typeof(bool), typeof(bool), true)]
        [InlineData(typeof(int), typeof(string), false)]
        [InlineData(typeof(string), typeof(object), false)]
        [InlineData(typeof(double), typeof(float), false)]
        [InlineData(typeof(bool), typeof(int), false)]
        public void TypeRouteFactory_Equals_VariousTypes_ReturnsExpectedResult(Type type1, Type type2, bool expected)
        {
            // Arrange
            var factory1 = new Routing.TypeRouteFactory(type1);
            var factory2 = new Routing.TypeRouteFactory(type2);

            // Act
            bool result = factory1.Equals(factory2);

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests that TypeRouteFactory.Equals works correctly with generic types.
        /// Input conditions: Generic types with same and different type parameters.
        /// Expected result: Returns true only when both generic types are identical.
        /// </summary>
        [Theory]
        [InlineData(typeof(System.Collections.Generic.List<string>), typeof(System.Collections.Generic.List<string>), true)]
        [InlineData(typeof(System.Collections.Generic.List<int>), typeof(System.Collections.Generic.List<int>), true)]
        [InlineData(typeof(System.Collections.Generic.List<string>), typeof(System.Collections.Generic.List<int>), false)]
        [InlineData(typeof(System.Collections.Generic.Dictionary<string, int>), typeof(System.Collections.Generic.Dictionary<string, int>), true)]
        [InlineData(typeof(System.Collections.Generic.Dictionary<string, int>), typeof(System.Collections.Generic.Dictionary<int, string>), false)]
        public void TypeRouteFactory_Equals_GenericTypes_ReturnsExpectedResult(Type type1, Type type2, bool expected)
        {
            // Arrange
            var factory1 = new Routing.TypeRouteFactory(type1);
            var factory2 = new Routing.TypeRouteFactory(type2);

            // Act
            bool result = factory1.Equals(factory2);

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests that TypeRouteFactory.Equals works correctly with array types.
        /// Input conditions: Array types with same and different element types.
        /// Expected result: Returns true only when both array types have identical element types.
        /// </summary>
        [Theory]
        [InlineData(typeof(string[]), typeof(string[]), true)]
        [InlineData(typeof(int[]), typeof(int[]), true)]
        [InlineData(typeof(string[]), typeof(int[]), false)]
        [InlineData(typeof(string[][]), typeof(string[][]), true)]
        [InlineData(typeof(string[][]), typeof(int[][]), false)]
        public void TypeRouteFactory_Equals_ArrayTypes_ReturnsExpectedResult(Type type1, Type type2, bool expected)
        {
            // Arrange
            var factory1 = new Routing.TypeRouteFactory(type1);
            var factory2 = new Routing.TypeRouteFactory(type2);

            // Act
            bool result = factory1.Equals(factory2);

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests that TypeRouteFactory.Equals works correctly with nullable value types.
        /// Input conditions: Nullable and non-nullable versions of the same value type.
        /// Expected result: Returns false when comparing nullable vs non-nullable types.
        /// </summary>
        [Theory]
        [InlineData(typeof(int?), typeof(int?), true)]
        [InlineData(typeof(bool?), typeof(bool?), true)]
        [InlineData(typeof(int?), typeof(int), false)]
        [InlineData(typeof(bool?), typeof(bool), false)]
        public void TypeRouteFactory_Equals_NullableTypes_ReturnsExpectedResult(Type type1, Type type2, bool expected)
        {
            // Arrange
            var factory1 = new Routing.TypeRouteFactory(type1);
            var factory2 = new Routing.TypeRouteFactory(type2);

            // Act
            bool result = factory1.Equals(factory2);

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests IsUserDefined method with null input.
        /// Should return false when route is null.
        /// </summary>
        [Fact]
        public void IsUserDefined_NullRoute_ReturnsFalse()
        {
            // Arrange
            string route = null;

            // Act
            bool result = Routing.IsUserDefined(route);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests IsUserDefined method with various string inputs.
        /// Verifies correct identification of user-defined vs system-generated routes.
        /// </summary>
        /// <param name="route">The route string to test</param>
        /// <param name="expected">The expected result</param>
        [Theory]
        [InlineData("", true)] // Empty string is user defined
        [InlineData("   ", true)] // Whitespace-only is user defined
        [InlineData("\t\n", true)] // Tab and newline are user defined
        [InlineData("IMPL_", false)] // Exactly the implicit prefix
        [InlineData("IMPL_test", false)] // Starts with implicit prefix
        [InlineData("IMPL_route123", false)] // Longer implicit route
        [InlineData("D_FAULT_", false)] // Exactly the default prefix
        [InlineData("D_FAULT_test", false)] // Starts with default prefix  
        [InlineData("D_FAULT_route456", false)] // Longer default route
        [InlineData("D_FAULT_IMPL_test", false)] // Starts with default (first prefix wins)
        [InlineData("IMPL_D_FAULT_test", false)] // Starts with implicit (first prefix wins)
        [InlineData("userroute", true)] // Regular user route
        [InlineData("MyPage", true)] // Typical user route
        [InlineData("test/IMPL_page", true)] // Contains but doesn't start with implicit prefix
        [InlineData("test/D_FAULT_page", true)] // Contains but doesn't start with default prefix
        [InlineData("impl_test", true)] // Wrong case for implicit prefix
        [InlineData("d_fault_test", true)] // Wrong case for default prefix
        [InlineData("Impl_test", true)] // Wrong case for implicit prefix
        [InlineData("D_fault_test", true)] // Wrong case for default prefix
        [InlineData("route with spaces", true)] // Route with spaces
        [InlineData("route@#$%^&*()", true)] // Route with special characters
        public void IsUserDefined_VariousRoutes_ReturnsExpectedResult(string route, bool expected)
        {
            // Arrange & Act
            bool result = Routing.IsUserDefined(route);

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests IsUserDefined method with very long strings.
        /// Verifies behavior with edge case string lengths.
        /// </summary>
        [Theory]
        [InlineData("IMPL_" + "a", false)] // Long implicit route
        [InlineData("D_FAULT_" + "b", false)] // Long default route  
        public void IsUserDefined_LongRoutes_ReturnsExpectedResult(string route, bool expected)
        {
            // Arrange
            string longRoute = route + new string('x', 1000);

            // Act
            bool result = Routing.IsUserDefined(longRoute);

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests IsUserDefined method with very long user-defined route.
        /// Should return true for long strings that don't start with system prefixes.
        /// </summary>
        [Fact]
        public void IsUserDefined_VeryLongUserRoute_ReturnsTrue()
        {
            // Arrange
            string longUserRoute = "user" + new string('x', 10000);

            // Act
            bool result = Routing.IsUserDefined(longUserRoute);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests IsUserDefined method with Unicode characters.
        /// Verifies behavior with international and special Unicode characters.
        /// </summary>
        [Theory]
        [InlineData("IMPL_测试", false)] // Implicit prefix with Chinese characters
        [InlineData("D_FAULT_🚀", false)] // Default prefix with emoji
        [InlineData("测试路由", true)] // Chinese characters only
        [InlineData("🚀🎉", true)] // Emoji only
        [InlineData("café", true)] // Accented characters
        public void IsUserDefined_UnicodeCharacters_ReturnsExpectedResult(string route, bool expected)
        {
            // Arrange & Act
            bool result = Routing.IsUserDefined(route);

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests IsUserDefined method with control characters.
        /// Verifies behavior with various control and non-printable characters.
        /// </summary>
        [Theory]
        [InlineData("\0test", true)] // Null character at start
        [InlineData("IMPL_\0", false)] // Implicit prefix with null character
        [InlineData("D_FAULT_\r\n", false)] // Default prefix with CRLF
        [InlineData("\u0001\u0002", true)] // Control characters only
        public void IsUserDefined_ControlCharacters_ReturnsExpectedResult(string route, bool expected)
        {
            // Arrange & Act
            bool result = Routing.IsUserDefined(route);

            // Assert
            Assert.Equal(expected, result);
        }
    }

    /// <summary>
    /// Unit tests for the TypeRouteFactory.GetHashCode method.
    /// </summary>
    public partial class TypeRouteFactoryTests
    {
        /// <summary>
        /// Tests that GetHashCode returns the same hash code as the underlying Type's GetHashCode.
        /// Input: TypeRouteFactory instance with a specific Type.
        /// Expected: Hash code matches the Type's hash code.
        /// </summary>
        [Fact]
        public void GetHashCode_WithSpecificType_ReturnsSameHashCodeAsType()
        {
            // Arrange
            var type = typeof(string);
            var factory = new Routing.TypeRouteFactory(type);
            var expectedHashCode = type.GetHashCode();

            // Act
            var actualHashCode = factory.GetHashCode();

            // Assert
            Assert.Equal(expectedHashCode, actualHashCode);
        }

        /// <summary>
        /// Tests that GetHashCode returns consistent results when called multiple times.
        /// Input: TypeRouteFactory instance called multiple times.
        /// Expected: Same hash code value on each call.
        /// </summary>
        [Fact]
        public void GetHashCode_CalledMultipleTimes_ReturnsConsistentValue()
        {
            // Arrange
            var factory = new Routing.TypeRouteFactory(typeof(int));

            // Act
            var hashCode1 = factory.GetHashCode();
            var hashCode2 = factory.GetHashCode();
            var hashCode3 = factory.GetHashCode();

            // Assert
            Assert.Equal(hashCode1, hashCode2);
            Assert.Equal(hashCode2, hashCode3);
        }

        /// <summary>
        /// Tests that TypeRouteFactory instances with the same Type have the same hash code.
        /// Input: Two TypeRouteFactory instances with identical Type.
        /// Expected: Both instances return the same hash code.
        /// </summary>
        [Theory]
        [InlineData(typeof(string))]
        [InlineData(typeof(int))]
        [InlineData(typeof(Element))]
        [InlineData(typeof(Page))]
        public void GetHashCode_WithSameType_ReturnsSameHashCode(Type type)
        {
            // Arrange
            var factory1 = new Routing.TypeRouteFactory(type);
            var factory2 = new Routing.TypeRouteFactory(type);

            // Act
            var hashCode1 = factory1.GetHashCode();
            var hashCode2 = factory2.GetHashCode();

            // Assert
            Assert.Equal(hashCode1, hashCode2);
        }

        /// <summary>
        /// Tests that TypeRouteFactory instances with different Types typically have different hash codes.
        /// Input: Two TypeRouteFactory instances with different Types.
        /// Expected: Different hash code values (note: hash collisions are possible but unlikely for these types).
        /// </summary>
        [Fact]
        public void GetHashCode_WithDifferentTypes_ReturnsDifferentHashCodes()
        {
            // Arrange
            var factory1 = new Routing.TypeRouteFactory(typeof(string));
            var factory2 = new Routing.TypeRouteFactory(typeof(int));

            // Act
            var hashCode1 = factory1.GetHashCode();
            var hashCode2 = factory2.GetHashCode();

            // Assert
            Assert.NotEqual(hashCode1, hashCode2);
        }

        /// <summary>
        /// Tests GetHashCode with various common framework types.
        /// Input: TypeRouteFactory instances with different framework types.
        /// Expected: Hash code matches the underlying Type's hash code for each type.
        /// </summary>
        [Theory]
        [InlineData(typeof(object))]
        [InlineData(typeof(Exception))]
        [InlineData(typeof(IServiceProvider))]
        [InlineData(typeof(System.Collections.Generic.List<string>))]
        public void GetHashCode_WithVariousTypes_DelegatesToTypeHashCode(Type type)
        {
            // Arrange
            var factory = new Routing.TypeRouteFactory(type);
            var expectedHashCode = type.GetHashCode();

            // Act
            var actualHashCode = factory.GetHashCode();

            // Assert
            Assert.Equal(expectedHashCode, actualHashCode);
        }
    }
}