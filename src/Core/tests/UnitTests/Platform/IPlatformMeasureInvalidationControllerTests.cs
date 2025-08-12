using System;
using Xunit;

#if IOS
using Microsoft.Maui.Platform;

namespace Microsoft.Maui.UnitTests.Platform
{
	public class IPlatformMeasureInvalidationControllerTests
	{
		[Fact]
		public void InterfaceCanBeImplemented()
		{
			// Verify that the interface is public and can be implemented by external code
			var implementation = new TestPlatformMeasureInvalidationController();
			
			Assert.NotNull(implementation);
			Assert.True(implementation is IPlatformMeasureInvalidationController);
		}

		[Fact]
		public void InvalidateMeasureMethod_CanBeCalledWithDefaultParameter()
		{
			var implementation = new TestPlatformMeasureInvalidationController();
			
			// Should be able to call with default parameter (isPropagating = false)
			bool result = implementation.InvalidateMeasure();
			
			Assert.True(result); // Default implementation returns true
		}

		[Fact]
		public void InvalidateMeasureMethod_CanBeCalledWithExplicitParameter()
		{
			var implementation = new TestPlatformMeasureInvalidationController();
			
			// Should be able to call with explicit parameter
			bool result1 = implementation.InvalidateMeasure(isPropagating: false);
			bool result2 = implementation.InvalidateMeasure(isPropagating: true);
			
			Assert.True(result1);
			Assert.False(result2); // Test implementation returns false when propagating
		}

		[Fact]
		public void InvalidateAncestorsMeasuresWhenMovedToWindowMethod_CanBeCalled()
		{
			var implementation = new TestPlatformMeasureInvalidationController();
			
			// Should be able to call without exceptions
			implementation.InvalidateAncestorsMeasuresWhenMovedToWindow();
			
			Assert.True(implementation.WasInvalidateAncestorsCalled);
		}

		/// <summary>
		/// Test implementation of IPlatformMeasureInvalidationController for testing purposes.
		/// Demonstrates how external customers can implement the interface to control propagation.
		/// </summary>
		private class TestPlatformMeasureInvalidationController : IPlatformMeasureInvalidationController
		{
			public bool WasInvalidateAncestorsCalled { get; private set; }

			public void InvalidateAncestorsMeasuresWhenMovedToWindow()
			{
				WasInvalidateAncestorsCalled = true;
			}

			public bool InvalidateMeasure(bool isPropagating = false)
			{
				// Return false when propagating to test stopping propagation scenario
				// This demonstrates how customers can control propagation behavior
				return !isPropagating;
			}
		}
	}
}
#endif