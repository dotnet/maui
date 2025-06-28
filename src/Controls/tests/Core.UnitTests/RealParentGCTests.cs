using System;
using Microsoft.Maui.Controls.Shapes;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class RealParentGCTests : BaseTestFixture
	{
		[Fact]
		public void RealParent_WhenWeakReferenceGarbageCollected_DoesNotThrowAndReturnsNull()
		{
			// Arrange
			var child = new RoundRectangle();
			var parent = new Grid();
			
			// Set up parent-child relationship
			child.Parent = parent;
			
			// Verify initial state
			Assert.NotNull(child.RealParent);
			Assert.Same(parent, child.RealParent);
			
			// Simulate the weak reference being garbage collected by setting to null
			// This is done using reflection to simulate the GC scenario
			var realParentField = typeof(Element).GetField("_realParent", 
				System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			Assert.NotNull(realParentField);
			
			// Create a weak reference to a collected object to simulate GC
			var weakRef = new WeakReference<Element>(null);
			realParentField.SetValue(child, weakRef);
			
			// Act & Assert - This should not throw and should return null
			var result = child.RealParent;
			Assert.Null(result);
			
			// Verify the weak reference was cleared
			var clearedWeakRef = realParentField.GetValue(child);
			Assert.Null(clearedWeakRef);
		}

		[Fact]
		public void RealParent_WithValidReference_ReturnsParent()
		{
			// Arrange
			var child = new RoundRectangle();
			var parent = new Grid();
			
			// Act
			child.Parent = parent;
			
			// Assert
			Assert.NotNull(child.RealParent);
			Assert.Same(parent, child.RealParent);
		}

		[Fact]
		public void RealParent_WhenSetToNull_ReturnsNull()
		{
			// Arrange
			var child = new RoundRectangle();
			var parent = new Grid();
			child.Parent = parent;
			
			// Act
			child.Parent = null;
			
			// Assert
			Assert.Null(child.RealParent);
		}

		[Fact]
		public void RealParent_MultipleAccessAfterGC_DoesNotContinueLogging()
		{
			// Arrange
			var child = new RoundRectangle();
			var parent = new Grid();
			child.Parent = parent;
			
			// Simulate GC'd weak reference
			var realParentField = typeof(Element).GetField("_realParent", 
				System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			Assert.NotNull(realParentField);
			
			var weakRef = new WeakReference<Element>(null);
			realParentField.SetValue(child, weakRef);
			
			// Act - Multiple accesses should not cause issues
			var result1 = child.RealParent;
			var result2 = child.RealParent;
			var result3 = child.RealParent;
			
			// Assert
			Assert.Null(result1);
			Assert.Null(result2);
			Assert.Null(result3);
			
			// The weak reference should be cleared after first access
			var clearedWeakRef = realParentField.GetValue(child);
			Assert.Null(clearedWeakRef);
		}
	}
}