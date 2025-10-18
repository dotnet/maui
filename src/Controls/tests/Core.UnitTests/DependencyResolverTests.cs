#nullable disable

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    /// <summary>
    /// Unit tests for the DependencyResolver.ResolveUsing(Func<Type, object>) method.
    /// </summary>
    public partial class DependencyResolverTests
    {
        /// <summary>
        /// Tests that ResolveUsing with a null resolver sets the Resolver property to a lambda that invokes null.
        /// This should result in a NullReferenceException when the created lambda is invoked.
        /// </summary>
        [Fact]
        public void ResolveUsing_NullResolver_SetsResolverToLambdaThatThrowsWhenInvoked()
        {
            // Arrange
            var originalResolver = GetCurrentResolver();
            Func<Type, object> nullResolver = null;

            try
            {
                // Act
                DependencyResolver.ResolveUsing(nullResolver);

                // Assert
                var currentResolver = GetCurrentResolver();
                Assert.NotNull(currentResolver);

                // Verify the lambda throws when invoked since resolver is null
                Assert.Throws<NullReferenceException>(() => currentResolver.Invoke(typeof(string), new object[0]));
            }
            finally
            {
                // Restore original resolver
                SetResolver(originalResolver);
            }
        }

        /// <summary>
        /// Tests that ResolveUsing with a valid resolver creates a lambda that correctly adapts the single-parameter resolver
        /// to the two-parameter signature and ignores the objects parameter.
        /// </summary>
        [Fact]
        public void ResolveUsing_ValidResolver_CreatesCorrectLambdaAndIgnoresObjectsParameter()
        {
            // Arrange
            var originalResolver = GetCurrentResolver();
            var expectedResult = new object();
            var invokedType = (Type)null;

            Func<Type, object> testResolver = type =>
            {
                invokedType = type;
                return expectedResult;
            };

            try
            {
                // Act
                DependencyResolver.ResolveUsing(testResolver);

                // Assert
                var currentResolver = GetCurrentResolver();
                Assert.NotNull(currentResolver);

                // Test that the lambda correctly calls the original resolver and ignores objects parameter
                var testType = typeof(string);
                var objectsParameter = new object[] { "ignored", 123, DateTime.Now };
                var result = currentResolver.Invoke(testType, objectsParameter);

                Assert.Same(expectedResult, result);
                Assert.Equal(testType, invokedType);
            }
            finally
            {
                // Restore original resolver
                SetResolver(originalResolver);
            }
        }

        /// <summary>
        /// Tests that ResolveUsing preserves exceptions thrown by the original resolver when the created lambda is invoked.
        /// </summary>
        [Fact]
        public void ResolveUsing_ResolverThatThrows_ExceptionPropagatesWhenLambdaInvoked()
        {
            // Arrange
            var originalResolver = GetCurrentResolver();
            var expectedMessage = "Test exception from resolver";

            Func<Type, object> throwingResolver = type =>
            {
                throw new InvalidOperationException(expectedMessage);
            };

            try
            {
                // Act
                DependencyResolver.ResolveUsing(throwingResolver);

                // Assert
                var currentResolver = GetCurrentResolver();
                Assert.NotNull(currentResolver);

                var exception = Assert.Throws<InvalidOperationException>(() =>
                    currentResolver.Invoke(typeof(string), new object[0]));

                Assert.Equal(expectedMessage, exception.Message);
            }
            finally
            {
                // Restore original resolver
                SetResolver(originalResolver);
            }
        }

        /// <summary>
        /// Tests that ResolveUsing works correctly when the resolver returns null values.
        /// </summary>
        [Fact]
        public void ResolveUsing_ResolverReturningNull_LambdaReturnsNull()
        {
            // Arrange
            var originalResolver = GetCurrentResolver();

            Func<Type, object> nullReturningResolver = type => null;

            try
            {
                // Act
                DependencyResolver.ResolveUsing(nullReturningResolver);

                // Assert
                var currentResolver = GetCurrentResolver();
                Assert.NotNull(currentResolver);

                var result = currentResolver.Invoke(typeof(string), new object[] { "test" });
                Assert.Null(result);
            }
            finally
            {
                // Restore original resolver
                SetResolver(originalResolver);
            }
        }

        /// <summary>
        /// Tests that ResolveUsing correctly handles different types passed to the resolver.
        /// </summary>
        [Theory]
        [InlineData(typeof(string))]
        [InlineData(typeof(int))]
        [InlineData(typeof(DateTime))]
        [InlineData(typeof(object))]
        public void ResolveUsing_DifferentTypes_PassesCorrectTypeToResolver(Type testType)
        {
            // Arrange
            var originalResolver = GetCurrentResolver();
            var receivedType = (Type)null;

            Func<Type, object> typeCapturingResolver = type =>
            {
                receivedType = type;
                return new object();
            };

            try
            {
                // Act
                DependencyResolver.ResolveUsing(typeCapturingResolver);

                // Assert
                var currentResolver = GetCurrentResolver();
                currentResolver.Invoke(testType, new object[] { "ignored1", "ignored2" });

                Assert.Equal(testType, receivedType);
            }
            finally
            {
                // Restore original resolver
                SetResolver(originalResolver);
            }
        }

        private static Func<Type, object[], object> GetCurrentResolver()
        {
            var resolverField = typeof(DependencyResolver).GetField("Resolver",
                BindingFlags.Static | BindingFlags.NonPublic);
            return (Func<Type, object[], object>)resolverField.GetValue(null);
        }

        private static void SetResolver(Func<Type, object[], object> resolver)
        {
            var resolverField = typeof(DependencyResolver).GetField("Resolver",
                BindingFlags.Static | BindingFlags.NonPublic);
            resolverField.SetValue(null, resolver);
        }
        private readonly Func<Type, object[], object> _originalResolver;

        public DependencyResolverTests()
        {
            // Save original resolver state
            _originalResolver = GetResolver();
        }

        public void Dispose()
        {
            // Restore original resolver state
            SetResolver(_originalResolver);
        }

        /// <summary>
        /// Tests that when visualType parameter is null, the method uses the default visual type.
        /// </summary>
        [Fact]
        public void ResolveOrCreate_NullVisualType_UsesDefaultVisualType()
        {
            // Arrange
            SetResolver(null);
            var type = typeof(ParameterlessOnly);

            // Act
            var result = DependencyResolver.ResolveOrCreate(type, null, null);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ParameterlessOnly>(result);
        }

        /// <summary>
        /// Tests that when the resolver returns a non-null value, that value is returned immediately.
        /// </summary>
        [Fact]
        public void ResolveOrCreate_ResolverReturnsValue_ReturnsResolvedValue()
        {
            // Arrange
            var expectedResult = new ParameterlessOnly();
            SetResolver((type, args) => expectedResult);
            var type = typeof(ParameterlessOnly);

            // Act
            var result = DependencyResolver.ResolveOrCreate(type, null, null);

            // Assert
            Assert.Same(expectedResult, result);
        }

        /// <summary>
        /// Tests that when resolver returns null and args is empty, parameterless constructor is used.
        /// </summary>
        [Fact]
        public void ResolveOrCreate_EmptyArgs_UsesParameterlessConstructor()
        {
            // Arrange
            SetResolver(null);
            var type = typeof(ParameterlessOnly);

            // Act
            var result = DependencyResolver.ResolveOrCreate(type, null, null);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ParameterlessOnly>(result);
        }

        /// <summary>
        /// Tests that when visualType is not default and type has 2-parameter constructor, 
        /// it uses the 2-parameter constructor with args[0] and source.
        /// </summary>
        [Fact]
        public void ResolveOrCreate_NonDefaultVisualTypeWithTwoParameterConstructor_UsesSourceAndFirstArg()
        {
            // Arrange
            SetResolver(null);
            var type = typeof(TwoParameterConstructor);
            var customVisualType = typeof(string); // Any type different from default
            var source = "test-source";
            var args = new object[] { "first-arg", "second-arg" };

            // Act
            var result = DependencyResolver.ResolveOrCreate(type, source, customVisualType, args);

            // Assert
            Assert.NotNull(result);
            var instance = Assert.IsType<TwoParameterConstructor>(result);
            Assert.Equal("first-arg", instance.FirstParameter);
            Assert.Equal("test-source", instance.SecondParameter);
        }

        /// <summary>
        /// Tests that when args length matches available constructor, that constructor is used.
        /// This tests the uncovered lines 58-61 in the ResolveOrCreate method.
        /// </summary>
        [Fact]
        public void ResolveOrCreate_ArgsLengthMatchesConstructor_UsesMatchingConstructor()
        {
            // Arrange
            SetResolver(null);
            var type = typeof(MultipleConstructors);
            var defaultVisualType = GetDefaultVisualType();
            var args = new object[] { "param1", 42, true }; // 3 parameters

            // Act
            var result = DependencyResolver.ResolveOrCreate(type, null, defaultVisualType, args);

            // Assert
            Assert.NotNull(result);
            var instance = Assert.IsType<MultipleConstructors>(result);
            Assert.Equal("param1", instance.StringParam);
            Assert.Equal(42, instance.IntParam);
            Assert.Equal(true, instance.BoolParam);
        }

        /// <summary>
        /// Tests that when no constructor matches args length, parameterless constructor is used as fallback.
        /// </summary>
        [Fact]
        public void ResolveOrCreate_NoMatchingConstructor_UsesParameterlessConstructor()
        {
            // Arrange
            SetResolver(null);
            var type = typeof(ParameterlessOnly);
            var args = new object[] { "param1", "param2" }; // No matching constructor

            // Act
            var result = DependencyResolver.ResolveOrCreate(type, null, null, args);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ParameterlessOnly>(result);
        }

        /// <summary>
        /// Tests the single-parameter overload that delegates to the main method.
        /// </summary>
        [Fact]
        public void ResolveOrCreate_SingleTypeParameter_DelegatesToMainMethod()
        {
            // Arrange
            SetResolver(null);
            var type = typeof(ParameterlessOnly);

            // Act
            var result = DependencyResolver.ResolveOrCreate(type);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ParameterlessOnly>(result);
        }

        /// <summary>
        /// Tests behavior with null args array.
        /// </summary>
        [Fact]
        public void ResolveOrCreate_NullArgs_UsesParameterlessConstructor()
        {
            // Arrange
            SetResolver(null);
            var type = typeof(ParameterlessOnly);

            // Act
            var result = DependencyResolver.ResolveOrCreate(type, null, null, null);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ParameterlessOnly>(result);
        }

        /// <summary>
        /// Tests that when visualType equals default visual type, the args-matching constructor path is used.
        /// </summary>
        [Fact]
        public void ResolveOrCreate_DefaultVisualTypeWithArgsMatchingConstructor_UsesArgsMatchingPath()
        {
            // Arrange
            SetResolver(null);
            var type = typeof(SingleParameterConstructor);
            var defaultVisualType = GetDefaultVisualType();
            var args = new object[] { "test-param" };

            // Act
            var result = DependencyResolver.ResolveOrCreate(type, "source", defaultVisualType, args);

            // Assert
            Assert.NotNull(result);
            var instance = Assert.IsType<SingleParameterConstructor>(result);
            Assert.Equal("test-param", instance.Parameter);
        }

        private Func<Type, object[], object> GetResolver()
        {
            // Access private static field via reflection
            var field = typeof(DependencyResolver).GetField("Resolver", BindingFlags.NonPublic | BindingFlags.Static);
            return (Func<Type, object[], object>)field.GetValue(null);
        }

        private Type GetDefaultVisualType()
        {
            var field = typeof(DependencyResolver).GetField("_defaultVisualType", BindingFlags.NonPublic | BindingFlags.Static);
            return (Type)field.GetValue(null);
        }

        // Helper classes for testing different constructor patterns
        public class ParameterlessOnly
        {
            public ParameterlessOnly() { }
        }

        public class TwoParameterConstructor
        {
            public object FirstParameter { get; }
            public object SecondParameter { get; }

            public TwoParameterConstructor(object first, object second)
            {
                FirstParameter = first;
                SecondParameter = second;
            }
        }

        public class SingleParameterConstructor
        {
            public object Parameter { get; }

            public SingleParameterConstructor(object parameter)
            {
                Parameter = parameter;
            }
        }

        public class MultipleConstructors
        {
            public string StringParam { get; }
            public int IntParam { get; }
            public bool BoolParam { get; }

            public MultipleConstructors()
            {
                StringParam = "default";
                IntParam = 0;
                BoolParam = false;
            }

            public MultipleConstructors(string str)
            {
                StringParam = str;
                IntParam = 0;
                BoolParam = false;
            }

            public MultipleConstructors(string str, int num)
            {
                StringParam = str;
                IntParam = num;
                BoolParam = false;
            }

            public MultipleConstructors(string str, int num, bool flag)
            {
                StringParam = str;
                IntParam = num;
                BoolParam = flag;
            }
        }
    }
}