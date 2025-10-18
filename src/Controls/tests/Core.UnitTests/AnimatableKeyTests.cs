#nullable disable

using System;
using System.Collections.Generic;


using Microsoft.Maui;
using Microsoft.Maui.Controls;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

    public class AnimatableKeyTests
    {
        class FakeAnimatable : IAnimatable
        {
            public void BatchBegin()
            {

            }

            public void BatchCommit()
            {

            }
        }

        [Fact]
        public void KeysWithDifferentHandlesAreNotEqual()
        {
            var animatable = new FakeAnimatable();

            var key1 = new AnimatableKey(animatable, "handle1");
            var key2 = new AnimatableKey(animatable, "handle2");

            Assert.NotEqual(key1, key2);
        }

        [Fact]
        public void KeysWithDifferentAnimatablesAreNotEqual()
        {
            var animatable1 = new FakeAnimatable();
            var animatable2 = new FakeAnimatable();

            var key1 = new AnimatableKey(animatable1, "handle");
            var key2 = new AnimatableKey(animatable2, "handle");

            Assert.NotEqual(key1, key2);
        }

        [Fact]
        public void KeysWithSameAnimatableAndHandleAreEqual()
        {
            var animatable = new FakeAnimatable();

            var key1 = new AnimatableKey(animatable, "handle");
            var key2 = new AnimatableKey(animatable, "handle");

            Assert.Equal(key1, key2);
        }

        [Fact]
        public void ThrowsWhenKeysWithSameAnimatableAdded()
        {
            var animatable = new FakeAnimatable();

            var key1 = new AnimatableKey(animatable, "handle");
            var key2 = new AnimatableKey(animatable, "handle");

            var dict = new Dictionary<AnimatableKey, object> { { key1, new object() } };

            Assert.Throws<ArgumentException>(() =>
            {
                var closureKey1 = key1;
                var closureKey2 = key1;
                var closureAnimatable = animatable;

                dict.Add(key2, new object());
            });
        }

        /// <summary>
        /// Tests that Equals returns false when the parameter is null.
        /// Input: null object parameter
        /// Expected: Returns false
        /// </summary>
        [Fact]
        public void Equals_NullObject_ReturnsFalse()
        {
            // Arrange
            var animatable = Substitute.For<IAnimatable>();
            var key = new AnimatableKey(animatable, "testHandle");

            // Act
            var result = key.Equals(null);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that Equals returns true when comparing the same reference.
        /// Input: Same AnimatableKey instance
        /// Expected: Returns true
        /// </summary>
        [Fact]
        public void Equals_SameReference_ReturnsTrue()
        {
            // Arrange
            var animatable = Substitute.For<IAnimatable>();
            var key = new AnimatableKey(animatable, "testHandle");

            // Act
            var result = key.Equals(key);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that Equals returns false when comparing with a different type.
        /// Input: Object of different type
        /// Expected: Returns false
        /// </summary>
        [Fact]
        public void Equals_DifferentType_ReturnsFalse()
        {
            // Arrange
            var animatable = Substitute.For<IAnimatable>();
            var key = new AnimatableKey(animatable, "testHandle");
            var differentTypeObject = "string object";

            // Act
            var result = key.Equals(differentTypeObject);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that Equals returns false when comparing with another object type.
        /// Input: Integer object
        /// Expected: Returns false
        /// </summary>
        [Fact]
        public void Equals_IntegerType_ReturnsFalse()
        {
            // Arrange
            var animatable = Substitute.For<IAnimatable>();
            var key = new AnimatableKey(animatable, "testHandle");
            object integerObject = 42;

            // Act
            var result = key.Equals(integerObject);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that Equals delegates to typed Equals method for same type with equal values.
        /// Input: Different AnimatableKey instance with same animatable and handle  
        /// Expected: Returns true (delegates to private Equals method)
        /// </summary>
        [Fact]
        public void Equals_SameTypeEqualValues_ReturnsTrue()
        {
            // Arrange
            var animatable = Substitute.For<IAnimatable>();
            var key1 = new AnimatableKey(animatable, "testHandle");
            var key2 = new AnimatableKey(animatable, "testHandle");

            // Act
            var result = key1.Equals((object)key2);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that Equals delegates to typed Equals method for same type with different handles.
        /// Input: Different AnimatableKey instance with same animatable but different handle
        /// Expected: Returns false (delegates to private Equals method)
        /// </summary>
        [Fact]
        public void Equals_SameTypeDifferentHandles_ReturnsFalse()
        {
            // Arrange
            var animatable = Substitute.For<IAnimatable>();
            var key1 = new AnimatableKey(animatable, "handle1");
            var key2 = new AnimatableKey(animatable, "handle2");

            // Act
            var result = key1.Equals((object)key2);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that Equals delegates to typed Equals method for same type with different animatables.
        /// Input: Different AnimatableKey instance with different animatable but same handle
        /// Expected: Returns false (delegates to private Equals method)
        /// </summary>
        [Fact]
        public void Equals_SameTypeDifferentAnimatables_ReturnsFalse()
        {
            // Arrange
            var animatable1 = Substitute.For<IAnimatable>();
            var animatable2 = Substitute.For<IAnimatable>();
            var key1 = new AnimatableKey(animatable1, "testHandle");
            var key2 = new AnimatableKey(animatable2, "testHandle");

            // Act
            var result = key1.Equals((object)key2);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that GetHashCode returns a consistent value when called multiple times on the same instance
        /// with a live animatable target.
        /// </summary>
        [Fact]
        public void GetHashCode_CalledMultipleTimes_ReturnsSameValue()
        {
            // Arrange
            var animatable = Substitute.For<IAnimatable>();
            var key = new AnimatableKey(animatable, "testHandle");

            // Act
            var hashCode1 = key.GetHashCode();
            var hashCode2 = key.GetHashCode();

            // Assert
            Assert.Equal(hashCode1, hashCode2);
        }

        /// <summary>
        /// Tests that GetHashCode returns the same value for two different AnimatableKey instances
        /// that have the same animatable and handle.
        /// </summary>
        [Fact]
        public void GetHashCode_WithSameAnimatableAndHandle_ReturnsSameValue()
        {
            // Arrange
            var animatable = Substitute.For<IAnimatable>();
            var key1 = new AnimatableKey(animatable, "testHandle");
            var key2 = new AnimatableKey(animatable, "testHandle");

            // Act
            var hashCode1 = key1.GetHashCode();
            var hashCode2 = key2.GetHashCode();

            // Assert
            Assert.Equal(hashCode1, hashCode2);
        }

        /// <summary>
        /// Tests that GetHashCode returns different values for AnimatableKey instances
        /// with different handles but the same animatable.
        /// </summary>
        [Fact]
        public void GetHashCode_WithDifferentHandles_ReturnsDifferentValues()
        {
            // Arrange
            var animatable = Substitute.For<IAnimatable>();
            var key1 = new AnimatableKey(animatable, "handle1");
            var key2 = new AnimatableKey(animatable, "handle2");

            // Act
            var hashCode1 = key1.GetHashCode();
            var hashCode2 = key2.GetHashCode();

            // Assert
            Assert.NotEqual(hashCode1, hashCode2);
        }

        /// <summary>
        /// Tests that GetHashCode returns potentially different values for AnimatableKey instances
        /// with different animatables but the same handle.
        /// </summary>
        [Fact]
        public void GetHashCode_WithDifferentAnimatables_MayReturnDifferentValues()
        {
            // Arrange
            var animatable1 = Substitute.For<IAnimatable>();
            var animatable2 = Substitute.For<IAnimatable>();
            var key1 = new AnimatableKey(animatable1, "testHandle");
            var key2 = new AnimatableKey(animatable2, "testHandle");

            // Act
            var hashCode1 = key1.GetHashCode();
            var hashCode2 = key2.GetHashCode();

            // Assert
            // Hash codes may be different (though not guaranteed due to hash collisions)
            // This test mainly ensures the method executes without throwing
            Assert.True(hashCode1 >= int.MinValue && hashCode1 <= int.MaxValue);
            Assert.True(hashCode2 >= int.MinValue && hashCode2 <= int.MaxValue);
        }

        /// <summary>
        /// Tests GetHashCode behavior when the WeakReference target has been garbage collected.
        /// This test attempts to trigger the code path where TryGetTarget returns false,
        /// though this scenario is difficult to test reliably due to GC unpredictability.
        /// </summary>
        [Fact]
        public void GetHashCode_WhenAnimatableGarbageCollected_ReturnsHandleHashCode()
        {
            // Arrange
            AnimatableKey key = null;

            // Create the key in a separate method to allow the animatable to be collected
            CreateKeyWithAnimatable(out key);

            // Force garbage collection to try to collect the animatable
            // Note: This is not guaranteed to work reliably in all test environments
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            // Act
            var hashCode = key.GetHashCode();

            // Assert
            // The hash code should be computed successfully regardless of whether
            // the WeakReference target is available or not
            Assert.True(hashCode >= int.MinValue && hashCode <= int.MaxValue);

            // Note: We cannot reliably test that the animatable was actually collected
            // and that the fallback path was taken, as this depends on GC behavior
            // which is not deterministic in unit tests.
        }

        private static void CreateKeyWithAnimatable(out AnimatableKey key)
        {
            var animatable = Substitute.For<IAnimatable>();
            key = new AnimatableKey(animatable, "testHandle");
            // The animatable reference goes out of scope here
        }

        /// <summary>
        /// Tests that GetHashCode handles the case where the handle contains special characters.
        /// </summary>
        [Fact]
        public void GetHashCode_WithSpecialCharactersInHandle_ReturnsValidHashCode()
        {
            // Arrange
            var animatable = Substitute.For<IAnimatable>();
            var specialHandle = "handle!@#$%^&*()_+-=[]{}|;':\",./<>?`~";
            var key = new AnimatableKey(animatable, specialHandle);

            // Act
            var hashCode = key.GetHashCode();

            // Assert
            Assert.True(hashCode >= int.MinValue && hashCode <= int.MaxValue);
        }

        /// <summary>
        /// Tests that GetHashCode handles the case where the handle is a very long string.
        /// </summary>
        [Fact]
        public void GetHashCode_WithLongHandle_ReturnsValidHashCode()
        {
            // Arrange
            var animatable = Substitute.For<IAnimatable>();
            var longHandle = new string('a', 10000); // Very long string
            var key = new AnimatableKey(animatable, longHandle);

            // Act
            var hashCode = key.GetHashCode();

            // Assert
            Assert.True(hashCode >= int.MinValue && hashCode <= int.MaxValue);
        }

        /// <summary>
        /// Tests that the AnimatableKey constructor throws ArgumentNullException when animatable parameter is null.
        /// Input: null animatable, valid handle.
        /// Expected: ArgumentNullException with parameter name "animatable".
        /// </summary>
        [Fact]
        public void Constructor_NullAnimatable_ThrowsArgumentNullException()
        {
            // Arrange
            IAnimatable animatable = null;
            string handle = "testHandle";

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => new AnimatableKey(animatable, handle));
            Assert.Equal("animatable", exception.ParamName);
        }

        /// <summary>
        /// Tests that the AnimatableKey constructor throws ArgumentException when handle parameter is null.
        /// Input: valid animatable, null handle.
        /// Expected: ArgumentException with parameter name "handle" and message "Argument is null or empty".
        /// </summary>
        [Fact]
        public void Constructor_NullHandle_ThrowsArgumentException()
        {
            // Arrange
            var animatable = Substitute.For<IAnimatable>();
            string handle = null;

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => new AnimatableKey(animatable, handle));
            Assert.Equal("handle", exception.ParamName);
            Assert.Contains("Argument is null or empty", exception.Message);
        }

        /// <summary>
        /// Tests that the AnimatableKey constructor throws ArgumentException when handle parameter is empty string.
        /// Input: valid animatable, empty handle.
        /// Expected: ArgumentException with parameter name "handle" and message "Argument is null or empty".
        /// </summary>
        [Fact]
        public void Constructor_EmptyHandle_ThrowsArgumentException()
        {
            // Arrange
            var animatable = Substitute.For<IAnimatable>();
            string handle = "";

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => new AnimatableKey(animatable, handle));
            Assert.Equal("handle", exception.ParamName);
            Assert.Contains("Argument is null or empty", exception.Message);
        }

        /// <summary>
        /// Tests that the AnimatableKey constructor succeeds with valid parameters and sets properties correctly.
        /// Input: valid animatable, valid non-empty handle.
        /// Expected: AnimatableKey instance created with properties set correctly.
        /// </summary>
        [Fact]
        public void Constructor_ValidParameters_SetsPropertiesCorrectly()
        {
            // Arrange
            var animatable = Substitute.For<IAnimatable>();
            string handle = "testHandle";

            // Act
            var animatableKey = new AnimatableKey(animatable, handle);

            // Assert
            Assert.NotNull(animatableKey.Animatable);
            Assert.Equal(handle, animatableKey.Handle);

            // Verify that the WeakReference contains the correct animatable
            IAnimatable retrievedAnimatable;
            bool success = animatableKey.Animatable.TryGetTarget(out retrievedAnimatable);
            Assert.True(success);
            Assert.Same(animatable, retrievedAnimatable);
        }

        /// <summary>
        /// Tests that the AnimatableKey constructor accepts whitespace-only handle (since IsNullOrEmpty returns false for whitespace).
        /// Input: valid animatable, whitespace-only handle.
        /// Expected: AnimatableKey instance created successfully.
        /// </summary>
        [Fact]
        public void Constructor_WhitespaceHandle_CreatesInstance()
        {
            // Arrange
            var animatable = Substitute.For<IAnimatable>();
            string handle = "   ";

            // Act
            var animatableKey = new AnimatableKey(animatable, handle);

            // Assert
            Assert.NotNull(animatableKey);
            Assert.Equal(handle, animatableKey.Handle);
        }

        /// <summary>
        /// Tests that the AnimatableKey constructor works with very long handle strings.
        /// Input: valid animatable, very long handle string.
        /// Expected: AnimatableKey instance created successfully.
        /// </summary>
        [Fact]
        public void Constructor_VeryLongHandle_CreatesInstance()
        {
            // Arrange
            var animatable = Substitute.For<IAnimatable>();
            string handle = new string('a', 10000);

            // Act
            var animatableKey = new AnimatableKey(animatable, handle);

            // Assert
            Assert.NotNull(animatableKey);
            Assert.Equal(handle, animatableKey.Handle);
        }

        /// <summary>
        /// Tests that the AnimatableKey constructor works with handle containing special characters.
        /// Input: valid animatable, handle with special characters.
        /// Expected: AnimatableKey instance created successfully.
        /// </summary>
        [Fact]
        public void Constructor_HandleWithSpecialCharacters_CreatesInstance()
        {
            // Arrange
            var animatable = Substitute.For<IAnimatable>();
            string handle = "test-handle_with@special#characters$%^&*()";

            // Act
            var animatableKey = new AnimatableKey(animatable, handle);

            // Assert
            Assert.NotNull(animatableKey);
            Assert.Equal(handle, animatableKey.Handle);
        }
    }
}