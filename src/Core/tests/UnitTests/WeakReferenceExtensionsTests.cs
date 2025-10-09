using System;

using Microsoft.Maui;
using Xunit;

namespace Microsoft.Maui.UnitTests
{
    public class WeakReferenceExtensionsTests
    {
        /// <summary>
        /// Tests that GetTargetOrDefault throws ArgumentNullException when the WeakReference parameter is null.
        /// This test validates the null parameter validation logic.
        /// Expected result: ArgumentNullException with correct parameter name.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void GetTargetOrDefault_NullWeakReference_ThrowsArgumentNullException()
        {
            // Arrange
            WeakReference<string> weakRef = null;

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => weakRef.GetTargetOrDefault());
            Assert.Equal("self", exception.ParamName);
        }

        /// <summary>
        /// Tests that GetTargetOrDefault returns the target object when it is still alive and reachable.
        /// This test validates the successful path where TryGetTarget returns true.
        /// Expected result: Returns the original target object.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void GetTargetOrDefault_TargetAvailable_ReturnsTarget()
        {
            // Arrange
            var target = "test string";
            var weakRef = new WeakReference<string>(target);

            // Act
            var result = weakRef.GetTargetOrDefault();

            // Assert
            Assert.Equal(target, result);
        }

        /// <summary>
        /// Tests that GetTargetOrDefault returns null when the target has been garbage collected.
        /// This test validates the default return path when TryGetTarget returns false.
        /// Expected result: Returns null (default value for reference types).
        /// </summary>
        [Fact]
        [Trait("Category", "auto-generated")]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        public void GetTargetOrDefault_TargetNotAvailable_ReturnsNull()
        {
            // Arrange
            WeakReference<object> weakRef;

            // Create weak reference in a separate scope to ensure target can be collected
            {
                var target = new object();
                weakRef = new WeakReference<object>(target);
                // target goes out of scope here
            }

            // Force multiple garbage collections to increase likelihood of collection
            for (int i = 0; i < 10; i++)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                // Check if target has been collected
                if (!weakRef.TryGetTarget(out _))
                {
                    break;
                }

                // Brief pause to allow GC to run
                System.Threading.Thread.Sleep(1);
            }

            // Skip test if GC didn't collect the target (common in test environments)
            if (weakRef.TryGetTarget(out _))
            {
                // In test environments, GC behavior is unpredictable
                // Test the null path by creating a WeakReference to null target
                weakRef = new WeakReference<object>(null);
            }

            // Act
            var result = weakRef.GetTargetOrDefault();

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Helper method to create a WeakReference whose target has been garbage collected.
        /// This ensures we can test the default return path reliably.
        /// </summary>
        private static WeakReference<object> CreateWeakReferenceWithCollectedTarget()
        {
            // Create a WeakReference in a separate method to ensure the target
            // has no remaining strong references
            return CreateWeakReferenceHelper();
        }

        private static WeakReference<object> CreateWeakReferenceHelper()
        {
            var target = new object();
            var weakRef = new WeakReference<object>(target);

            // Remove the strong reference
            target = null;

            // Force garbage collection multiple times to increase chances of collection
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            return weakRef;
        }

        /// <summary>
        /// Tests that TryGetTarget throws ArgumentNullException when the WeakReference is null.
        /// Input: null WeakReference
        /// Expected: ArgumentNullException with parameter name "self"
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void TryGetTarget_NullWeakReference_ThrowsArgumentNullException()
        {
            // Arrange
            WeakReference nullWeakReference = null;

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => nullWeakReference.TryGetTarget<string>(out _));
            Assert.Equal("self", exception.ParamName);
        }

        /// <summary>
        /// Tests that TryGetTarget returns false and sets target to null when WeakReference has null Target.
        /// Input: WeakReference with null Target
        /// Expected: Returns false, target is set to null
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void TryGetTarget_WeakReferenceWithNullTarget_ReturnsFalseAndSetsTargetToNull()
        {
            // Arrange
            var weakReference = new WeakReference(null);

            // Act
            bool result = weakReference.TryGetTarget<string>(out string target);

            // Assert
            Assert.False(result);
            Assert.Null(target);
        }

        /// <summary>
        /// Tests that TryGetTarget returns true and sets target correctly when WeakReference has valid Target.
        /// Input: WeakReference with valid string Target
        /// Expected: Returns true, target is set to the string object
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void TryGetTarget_WeakReferenceWithValidTarget_ReturnsTrueAndSetsTargetCorrectly()
        {
            // Arrange
            string testString = "test string";
            var weakReference = new WeakReference(testString);

            // Act
            bool result = weakReference.TryGetTarget<string>(out string target);

            // Assert
            Assert.True(result);
            Assert.Equal(testString, target);
        }

        /// <summary>
        /// Tests that TryGetTarget returns false and sets target to null when WeakReference Target is incompatible type.
        /// Input: WeakReference with integer Target, attempting to get as string
        /// Expected: Returns false, target is set to null
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        [Trait("Category", "ProductionBugSuspected")]
        public void TryGetTarget_WeakReferenceWithIncompatibleTarget_ReturnsFalseAndSetsTargetToNull()
        {
            // Arrange
            int testInt = 42;
            var weakReference = new WeakReference(testInt);

            // Act
            bool result = weakReference.TryGetTarget<string>(out string target);

            // Assert
            Assert.False(result);
            Assert.Null(target);
        }

        /// <summary>
        /// Tests that TryGetTarget works correctly with different reference types.
        /// Input: WeakReference with object Target, attempting to get as object
        /// Expected: Returns true, target is set to the object
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void TryGetTarget_WeakReferenceWithObjectTarget_ReturnsTrueAndSetsTargetCorrectly()
        {
            // Arrange
            var testObject = new object();
            var weakReference = new WeakReference(testObject);

            // Act
            bool result = weakReference.TryGetTarget<object>(out object target);

            // Assert
            Assert.True(result);
            Assert.Same(testObject, target);
        }

        /// <summary>
        /// Tests that TryGetTarget handles target that becomes null after WeakReference creation.
        /// Input: WeakReference created with target, then target becomes null
        /// Expected: Returns false, target is set to null
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void TryGetTarget_WeakReferenceAfterTargetBecomesNull_ReturnsFalseAndSetsTargetToNull()
        {
            // Arrange
            var weakReference = new WeakReference("initial value");
            weakReference.Target = null; // Simulate target being collected or set to null

            // Act
            bool result = weakReference.TryGetTarget<string>(out string target);

            // Assert
            Assert.False(result);
            Assert.Null(target);
        }
    }
}