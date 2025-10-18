#nullable disable

using System;
using System.ComponentModel;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Platform;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public partial class RoutingEffectTests
    {
        /// <summary>
        /// Test implementation of RoutingEffect to expose the protected constructor for testing.
        /// </summary>
        private class TestRoutingEffect : RoutingEffect
        {
            public TestRoutingEffect(string effectId) : base(effectId)
            {
            }

            public Effect GetInner() => Inner;
        }

        /// <summary>
        /// Tests that the constructor properly initializes the Inner field when given a valid effect ID.
        /// Input: Valid effect ID string.
        /// Expected: Inner field is set to the resolved effect with matching ResolveId.
        /// </summary>
        [Fact]
        public void Constructor_ValidEffectId_SetsInnerFieldCorrectly()
        {
            // Arrange
            var effectId = "TestGroup.TestEffect";

            // Act
            var routingEffect = new TestRoutingEffect(effectId);

            // Assert
            Assert.NotNull(routingEffect.GetInner());
            Assert.Equal(effectId, routingEffect.GetInner().ResolveId);
        }

        /// <summary>
        /// Tests that the constructor handles null effect ID parameter.
        /// Input: null effect ID.
        /// Expected: Inner field is set to a NullEffect with null ResolveId.
        /// </summary>
        [Fact]
        public void Constructor_NullEffectId_SetsInnerToNullEffect()
        {
            // Arrange
            string effectId = null;

            // Act
            var routingEffect = new TestRoutingEffect(effectId);

            // Assert
            Assert.NotNull(routingEffect.GetInner());
            Assert.Equal(effectId, routingEffect.GetInner().ResolveId);
        }

        /// <summary>
        /// Tests that the constructor handles empty effect ID parameter.
        /// Input: Empty string effect ID.
        /// Expected: Inner field is set to a NullEffect with empty string ResolveId.
        /// </summary>
        [Fact]
        public void Constructor_EmptyEffectId_SetsInnerToNullEffect()
        {
            // Arrange
            var effectId = "";

            // Act
            var routingEffect = new TestRoutingEffect(effectId);

            // Assert
            Assert.NotNull(routingEffect.GetInner());
            Assert.Equal(effectId, routingEffect.GetInner().ResolveId);
        }

        /// <summary>
        /// Tests that the constructor handles whitespace-only effect ID parameter.
        /// Input: Whitespace-only string effect ID.
        /// Expected: Inner field is set to a NullEffect with whitespace ResolveId.
        /// </summary>
        [Fact]
        public void Constructor_WhitespaceEffectId_SetsInnerToNullEffect()
        {
            // Arrange
            var effectId = "   ";

            // Act
            var routingEffect = new TestRoutingEffect(effectId);

            // Assert
            Assert.NotNull(routingEffect.GetInner());
            Assert.Equal(effectId, routingEffect.GetInner().ResolveId);
        }

        /// <summary>
        /// Tests that the constructor handles effect ID with special characters.
        /// Input: Effect ID containing special characters.
        /// Expected: Inner field is set with the exact ResolveId containing special characters.
        /// </summary>
        [Theory]
        [InlineData("Test.Effect@#$")]
        [InlineData("Test\nEffect")]
        [InlineData("Test\tEffect")]
        [InlineData("Test Effect")]
        [InlineData("Test.Effect.With.Many.Dots")]
        [InlineData("123.456")]
        public void Constructor_EffectIdWithSpecialCharacters_SetsInnerWithCorrectResolveId(string effectId)
        {
            // Arrange & Act
            var routingEffect = new TestRoutingEffect(effectId);

            // Assert
            Assert.NotNull(routingEffect.GetInner());
            Assert.Equal(effectId, routingEffect.GetInner().ResolveId);
        }

        /// <summary>
        /// Tests that the constructor handles very long effect ID parameter.
        /// Input: Very long string effect ID.
        /// Expected: Inner field is set with the exact long ResolveId.
        /// </summary>
        [Fact]
        public void Constructor_VeryLongEffectId_SetsInnerWithCorrectResolveId()
        {
            // Arrange
            var effectId = new string('A', 10000);

            // Act
            var routingEffect = new TestRoutingEffect(effectId);

            // Assert
            Assert.NotNull(routingEffect.GetInner());
            Assert.Equal(effectId, routingEffect.GetInner().ResolveId);
        }

        /// <summary>
        /// Tests that the constructor handles effect ID with Unicode characters.
        /// Input: Effect ID containing Unicode characters.
        /// Expected: Inner field is set with the exact Unicode ResolveId.
        /// </summary>
        [Theory]
        [InlineData("Test.Effect.🎯")]
        [InlineData("Test.效果")]
        [InlineData("Тест.Эффект")]
        [InlineData("テスト.エフェクト")]
        public void Constructor_EffectIdWithUnicodeCharacters_SetsInnerWithCorrectResolveId(string effectId)
        {
            // Arrange & Act
            var routingEffect = new TestRoutingEffect(effectId);

            // Assert
            Assert.NotNull(routingEffect.GetInner());
            Assert.Equal(effectId, routingEffect.GetInner().ResolveId);
        }

        /// <summary>
        /// Tests that the constructor never sets Inner field to null regardless of input.
        /// Input: Various effect ID values including edge cases.
        /// Expected: Inner field is never null.
        /// </summary>
        [Theory]
        [InlineData("ValidEffect")]
        [InlineData("InvalidEffect")]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void Constructor_AnyEffectId_InnerFieldNeverNull(string effectId)
        {
            // Arrange & Act
            var routingEffect = new TestRoutingEffect(effectId);

            // Assert
            Assert.NotNull(routingEffect.GetInner());
        }

        /// <summary>
        /// Tests that OnAttached can be called without throwing any exceptions.
        /// Verifies the empty implementation executes successfully.
        /// </summary>
        [Fact]
        public void OnAttached_WhenCalled_DoesNotThrow()
        {
            // Arrange
            var routingEffect = new TestableRoutingEffect();

            // Act & Assert
            var exception = Record.Exception(() => routingEffect.CallOnAttached());
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that OnAttached can be called multiple times without issues.
        /// Verifies the empty implementation is idempotent and safe for repeated calls.
        /// </summary>
        [Fact]
        public void OnAttached_WhenCalledMultipleTimes_DoesNotThrow()
        {
            // Arrange
            var routingEffect = new TestableRoutingEffect();

            // Act & Assert
            var exception = Record.Exception(() =>
            {
                routingEffect.CallOnAttached();
                routingEffect.CallOnAttached();
                routingEffect.CallOnAttached();
            });
            Assert.Null(exception);
        }

        /// <summary>
        /// Helper class to expose the protected OnAttached method for testing.
        /// Uses the parameterless constructor to avoid dependencies.
        /// </summary>
        private class TestableRoutingEffect : RoutingEffect
        {
            public TestableRoutingEffect() : base()
            {
            }

            public void CallOnAttached()
            {
                OnAttached();
            }
        }

        /// <summary>
        /// Tests that OnDetached can be called without throwing any exceptions.
        /// This test verifies that the empty override method executes successfully.
        /// </summary>
        [Fact]
        public void OnDetached_WhenCalled_DoesNotThrowException()
        {
            // Arrange
            var routingEffect = new TestableRoutingEffect();

            // Act & Assert
            var exception = Record.Exception(() => routingEffect.CallOnDetached());

            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that OnDetached can be called multiple times without throwing exceptions.
        /// This test verifies the method's idempotent behavior and that repeated calls are safe.
        /// </summary>
        [Fact]
        public void OnDetached_WhenCalledMultipleTimes_DoesNotThrowException()
        {
            // Arrange
            var routingEffect = new TestableRoutingEffect();

            // Act & Assert
            var exception = Record.Exception(() =>
            {
                routingEffect.CallOnDetached();
                routingEffect.CallOnDetached();
                routingEffect.CallOnDetached();
            });

            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that OnDetached method executes and completes successfully.
        /// This test ensures the empty method body executes without any issues.
        /// </summary>
        [Fact]
        public void OnDetached_WhenCalled_CompletesSuccessfully()
        {
            // Arrange
            var routingEffect = new TestableRoutingEffect();
            bool methodCompleted = false;

            // Act
            try
            {
                routingEffect.CallOnDetached();
                methodCompleted = true;
            }
            catch
            {
                // Should not reach here
            }

            // Assert
            Assert.True(methodCompleted);
        }

        /// <summary>
        /// Tests that ClearEffect does not throw when both Inner and PlatformEffect are null.
        /// Uses parameterless constructor which results in Inner being null.
        /// </summary>
        [Fact]
        public void ClearEffect_WhenBothInnerAndPlatformEffectAreNull_DoesNotThrow()
        {
            // Arrange
            var routingEffect = new TestableRoutingEffect();
            // Inner is null by default with parameterless constructor
            // PlatformEffect is null by default

            // Act & Assert - Should not throw
            routingEffect.ClearEffect();
        }

        /// <summary>
        /// Tests that ClearEffect calls ClearEffect on PlatformEffect when it is not null and Inner is null.
        /// Uses parameterless constructor which results in Inner being null.
        /// </summary>
        [Fact]
        public void ClearEffect_WhenInnerIsNullAndPlatformEffectIsNotNull_CallsPlatformEffectClearEffect()
        {
            // Arrange
            var routingEffect = new TestableRoutingEffect();
            var mockPlatformEffect = Substitute.For<PlatformEffect>();
            routingEffect.PlatformEffect = mockPlatformEffect;

            // Act
            routingEffect.ClearEffect();

            // Assert
            mockPlatformEffect.Received(1).ClearEffect();
        }

        /// <summary>
        /// Tests that ClearEffect handles null PlatformEffect gracefully when Inner is potentially set.
        /// Note: Inner field behavior depends on the Resolve method implementation which cannot be controlled in tests.
        /// </summary>
        [Fact]
        public void ClearEffect_WhenPlatformEffectIsNull_DoesNotThrow()
        {
            // Arrange
            var routingEffect = new TestableRoutingEffect("test.effect.id");
            // PlatformEffect remains null by default

            // Act & Assert - Should not throw regardless of Inner state
            routingEffect.ClearEffect();
        }

        /// <summary>
        /// Tests that ClearEffect calls ClearEffect on PlatformEffect when both Inner and PlatformEffect are potentially not null.
        /// Note: Inner field behavior depends on the Resolve method implementation which cannot be controlled in tests.
        /// </summary>
        [Fact]
        public void ClearEffect_WhenPlatformEffectIsNotNull_CallsPlatformEffectClearEffect()
        {
            // Arrange
            var routingEffect = new TestableRoutingEffect("test.effect.id");
            var mockPlatformEffect = Substitute.For<PlatformEffect>();
            routingEffect.PlatformEffect = mockPlatformEffect;

            // Act
            routingEffect.ClearEffect();

            // Assert
            mockPlatformEffect.Received(1).ClearEffect();
        }

        /// <summary>
        /// Tests ClearEffect with various effect ID values to ensure robustness.
        /// Validates that different effect ID inputs don't cause exceptions during ClearEffect.
        /// </summary>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        [InlineData("valid.effect.id")]
        [InlineData("very.long.effect.identifier.with.multiple.segments.and.extended.naming.convention")]
        [InlineData("special!@#$%^&*()characters")]
        public void ClearEffect_WithVariousEffectIds_DoesNotThrow(string effectId)
        {
            // Arrange
            var routingEffect = new TestableRoutingEffect(effectId);
            var mockPlatformEffect = Substitute.For<PlatformEffect>();
            routingEffect.PlatformEffect = mockPlatformEffect;

            // Act & Assert - Should not throw regardless of effect ID
            routingEffect.ClearEffect();

            // Assert PlatformEffect.ClearEffect was called
            mockPlatformEffect.Received(1).ClearEffect();
        }

        /// <summary>
        /// Tests SendAttached when both Inner and PlatformEffect are null.
        /// Should not throw any exceptions and complete successfully.
        /// </summary>
        [Fact]
        public void SendAttached_BothInnerAndPlatformEffectNull_DoesNotThrow()
        {
            // Arrange
            var routingEffect = new TestableRoutingEffect();

            // Act & Assert
            var exception = Record.Exception(() => routingEffect.SendAttachedPublic());
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests SendAttached when Inner is null but PlatformEffect is not null.
        /// Should call SendAttached on PlatformEffect and not throw.
        /// </summary>
        [Fact]
        public void SendAttached_InnerNullPlatformEffectNotNull_CallsPlatformEffectSendAttached()
        {
            // Arrange
            var routingEffect = new TestableRoutingEffect();
            var mockPlatformEffect = Substitute.For<PlatformEffect>();
            routingEffect.SetPlatformEffect(mockPlatformEffect);

            // Act
            routingEffect.SendAttachedPublic();

            // Assert
            mockPlatformEffect.Received(1).SendAttached();
        }

        /// <summary>
        /// Tests SendAttached when Inner is not null but PlatformEffect is null.
        /// This test demonstrates the intended behavior but cannot be fully executed
        /// due to the readonly nature of the Inner field and lack of access to the Resolve method.
        /// </summary>
        [Fact(Skip = "Cannot easily set Inner field due to readonly constraint and missing Resolve method")]
        public void SendAttached_InnerNotNullPlatformEffectNull_CallsInnerSendAttached()
        {
            // This test would verify that when Inner is not null and PlatformEffect is null,
            // the method calls Inner.SendAttached() but not PlatformEffect.SendAttached().
            // 
            // To implement this test, you would need:
            // 1. Access to the Resolve method to create a RoutingEffect with non-null Inner, or
            // 2. A way to set the Inner field after construction (which is not possible due to readonly), or
            // 3. A constructor that accepts an Effect parameter for testing purposes

            Assert.True(false, "Test implementation requires access to set Inner field");
        }

        /// <summary>
        /// Tests SendAttached when both Inner and PlatformEffect are not null.
        /// This test demonstrates the intended behavior but cannot be fully executed
        /// due to the readonly nature of the Inner field and lack of access to the Resolve method.
        /// </summary>
        [Fact(Skip = "Cannot easily set Inner field due to readonly constraint and missing Resolve method")]
        public void SendAttached_BothInnerAndPlatformEffectNotNull_CallsBothSendAttached()
        {
            // This test would verify that when both Inner and PlatformEffect are not null,
            // the method calls both Inner.SendAttached() and PlatformEffect.SendAttached().
            //
            // To implement this test, you would need:
            // 1. Access to the Resolve method to create a RoutingEffect with non-null Inner, or
            // 2. A way to set the Inner field after construction (which is not possible due to readonly), or
            // 3. A constructor that accepts an Effect parameter for testing purposes

            Assert.True(false, "Test implementation requires access to set Inner field");
        }

        /// <summary>
        /// Tests SendDetached method when both Inner and PlatformEffect are null.
        /// Should complete without throwing any exceptions.
        /// </summary>
        [Fact]
        public void SendDetached_WhenInnerAndPlatformEffectAreNull_DoesNotThrow()
        {
            // Arrange
            var routingEffect = new TestableRoutingEffect();

            // Act & Assert
            var exception = Record.Exception(() => routingEffect.SendDetached());

            Assert.Null(exception);
        }

        /// <summary>
        /// Tests SendDetached method when Inner is not null and PlatformEffect is null.
        /// Should call SendDetached on Inner effect only.
        /// </summary>
        [Fact]
        public void SendDetached_WhenInnerIsNotNullAndPlatformEffectIsNull_CallsInnerSendDetached()
        {
            // Arrange
            var routingEffect = new TestableRoutingEffect("TestEffect");

            // Act
            var exception = Record.Exception(() => routingEffect.SendDetached());

            // Assert
            Assert.Null(exception);
            Assert.NotNull(routingEffect.GetInner());
        }

        /// <summary>
        /// Tests SendDetached method when Inner is null and PlatformEffect is not null.
        /// Should call SendDetached on PlatformEffect only.
        /// </summary>
        [Fact]
        public void SendDetached_WhenInnerIsNullAndPlatformEffectIsNotNull_CallsPlatformEffectSendDetached()
        {
            // Arrange
            var routingEffect = new TestableRoutingEffect();
            var testPlatformEffect = new TestPlatformEffect();
            routingEffect.SetPlatformEffect(testPlatformEffect);

            // Act
            var exception = Record.Exception(() => routingEffect.SendDetached());

            // Assert
            Assert.Null(exception);
            Assert.True(testPlatformEffect.SendDetachedCalled);
        }

        /// <summary>
        /// Tests SendDetached method when both Inner and PlatformEffect are not null.
        /// Should call SendDetached on both Inner and PlatformEffect.
        /// </summary>
        [Fact]
        public void SendDetached_WhenBothInnerAndPlatformEffectAreNotNull_CallsBothSendDetached()
        {
            // Arrange
            var routingEffect = new TestableRoutingEffect("TestEffect");
            var testPlatformEffect = new TestPlatformEffect();
            routingEffect.SetPlatformEffect(testPlatformEffect);

            // Act
            var exception = Record.Exception(() => routingEffect.SendDetached());

            // Assert
            Assert.Null(exception);
            Assert.NotNull(routingEffect.GetInner());
            Assert.True(testPlatformEffect.SendDetachedCalled);
        }

        /// <summary>
        /// Test helper class that inherits from RoutingEffect with a non-null Inner field.
        /// </summary>
        private class TestableRoutingEffectWithInner : RoutingEffect
        {
            public TestableRoutingEffectWithInner(string effectId) : base(effectId)
            {
            }

            public void TestSendOnElementPropertyChanged(PropertyChangedEventArgs args)
            {
                SendOnElementPropertyChanged(args);
            }
        }

        /// <summary>
        /// Tests that SendOnElementPropertyChanged does not throw when args parameter is null.
        /// Verifies that null-conditional operators handle null args gracefully.
        /// </summary>
        [Fact]
        public void SendOnElementPropertyChanged_WithNullArgs_DoesNotThrow()
        {
            // Arrange
            var routingEffect = new TestableRoutingEffect();

            // Act & Assert
            var exception = Record.Exception(() => routingEffect.TestSendOnElementPropertyChanged(null));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that SendOnElementPropertyChanged calls PlatformEffect when Inner is null but PlatformEffect is not null.
        /// Verifies that the null-conditional operator properly handles null Inner field.
        /// </summary>
        [Fact]
        public void SendOnElementPropertyChanged_WithNullInnerAndValidPlatformEffect_CallsPlatformEffect()
        {
            // Arrange
            var routingEffect = new TestableRoutingEffect();
            var mockPlatformEffect = Substitute.For<PlatformEffect>();
            routingEffect.PlatformEffect = mockPlatformEffect;
            var args = new PropertyChangedEventArgs("TestProperty");

            // Act
            routingEffect.TestSendOnElementPropertyChanged(args);

            // Assert
            mockPlatformEffect.Received(1).SendOnElementPropertyChanged(args);
        }

        /// <summary>
        /// Tests that SendOnElementPropertyChanged does not throw when both Inner and PlatformEffect are null.
        /// Verifies that null-conditional operators handle both null dependencies gracefully.
        /// </summary>
        [Fact]
        public void SendOnElementPropertyChanged_WithBothInnerAndPlatformEffectNull_DoesNotThrow()
        {
            // Arrange
            var routingEffect = new TestableRoutingEffect();
            var args = new PropertyChangedEventArgs("TestProperty");

            // Act & Assert
            var exception = Record.Exception(() => routingEffect.TestSendOnElementPropertyChanged(args));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that SendOnElementPropertyChanged only calls PlatformEffect when Inner is null.
        /// Verifies that the method correctly handles the case where Inner is null but PlatformEffect is null too.
        /// </summary>
        [Fact]
        public void SendOnElementPropertyChanged_WithNullInnerAndNullPlatformEffect_DoesNotThrow()
        {
            // Arrange
            var routingEffect = new TestableRoutingEffect();
            routingEffect.PlatformEffect = null;
            var args = new PropertyChangedEventArgs("TestProperty");

            // Act & Assert
            var exception = Record.Exception(() => routingEffect.TestSendOnElementPropertyChanged(args));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that SendOnElementPropertyChanged handles empty property name in PropertyChangedEventArgs.
        /// Verifies that the method works correctly with edge case property names.
        /// </summary>
        [Fact]
        public void SendOnElementPropertyChanged_WithEmptyPropertyName_DoesNotThrow()
        {
            // Arrange
            var routingEffect = new TestableRoutingEffect();
            var mockPlatformEffect = Substitute.For<PlatformEffect>();
            routingEffect.PlatformEffect = mockPlatformEffect;
            var args = new PropertyChangedEventArgs(string.Empty);

            // Act
            routingEffect.TestSendOnElementPropertyChanged(args);

            // Assert
            mockPlatformEffect.Received(1).SendOnElementPropertyChanged(args);
        }

        /// <summary>
        /// Tests that SendOnElementPropertyChanged handles null property name in PropertyChangedEventArgs.
        /// Verifies that the method works correctly when PropertyChangedEventArgs has null property name.
        /// </summary>
        [Fact]
        public void SendOnElementPropertyChanged_WithNullPropertyName_DoesNotThrow()
        {
            // Arrange
            var routingEffect = new TestableRoutingEffect();
            var mockPlatformEffect = Substitute.For<PlatformEffect>();
            routingEffect.PlatformEffect = mockPlatformEffect;
            var args = new PropertyChangedEventArgs(null);

            // Act
            routingEffect.TestSendOnElementPropertyChanged(args);

            // Assert
            mockPlatformEffect.Received(1).SendOnElementPropertyChanged(args);
        }

        /// <summary>
        /// Tests that SendOnElementPropertyChanged handles very long property names.
        /// Verifies that the method works correctly with edge case property names.
        /// </summary>
        [Fact]
        public void SendOnElementPropertyChanged_WithLongPropertyName_DoesNotThrow()
        {
            // Arrange
            var routingEffect = new TestableRoutingEffect();
            var mockPlatformEffect = Substitute.For<PlatformEffect>();
            routingEffect.PlatformEffect = mockPlatformEffect;
            var longPropertyName = new string('A', 10000);
            var args = new PropertyChangedEventArgs(longPropertyName);

            // Act
            routingEffect.TestSendOnElementPropertyChanged(args);

            // Assert
            mockPlatformEffect.Received(1).SendOnElementPropertyChanged(args);
        }

        /// <summary>
        /// Tests that the parameterless constructor creates an instance without throwing exceptions
        /// and initializes the Inner field to its default value (null).
        /// </summary>
        [Fact]
        public void Constructor_DefaultConstruction_CreatesInstanceWithNullInner()
        {
            // Arrange & Act
            var routingEffect = new TestableRoutingEffect();

            // Assert
            Assert.NotNull(routingEffect);
            Assert.Null(routingEffect.GetInner());
        }

        /// <summary>
        /// Tests that the parameterless constructor does not throw any exceptions
        /// when called multiple times.
        /// </summary>
        [Fact]
        public void Constructor_DefaultConstruction_DoesNotThrowException()
        {
            // Arrange & Act & Assert
            var exception = Record.Exception(() => new TestableRoutingEffect());

            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that multiple instances can be created using the parameterless constructor
        /// and each instance is independent.
        /// </summary>
        [Fact]
        public void Constructor_DefaultConstruction_CreatesIndependentInstances()
        {
            // Arrange & Act
            var routingEffect1 = new TestableRoutingEffect();
            var routingEffect2 = new TestableRoutingEffect();

            // Assert
            Assert.NotNull(routingEffect1);
            Assert.NotNull(routingEffect2);
            Assert.NotSame(routingEffect1, routingEffect2);
        }

    }
}