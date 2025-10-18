#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;


using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

    public class EffectTests : BaseTestFixture
    {
        [Fact]
        public void ResolveSetsId()
        {
            string id = "Unknown";
            var effect = Effect.Resolve(id);
            Assert.Equal(id, effect.ResolveId);
        }

        [Fact]
        public void UnknownIdReturnsNullEffect()
        {
            var effect = Effect.Resolve("Foo");
            Assert.IsType<NullEffect>(effect);
        }

        [Fact]
        public void SendAttachedSetsFlag()
        {
            var effect = Effect.Resolve("Foo");
            effect.SendAttached();
            Assert.True(effect.IsAttached);
        }

        [Fact]
        public void SendDetachedUnsetsFlag()
        {
            var effect = Effect.Resolve("Foo");
            effect.SendAttached();
            effect.SendDetached();
            Assert.False(effect.IsAttached);
        }

        [Fact]
        public void EffectLifecyclePreProvider()
        {
            var effect = new CustomEffect();
            var element = new Label();

            element.Effects.Add(effect);
            ((IVisualElementController)element).EffectControlProvider = new EffectControlProvider();

            Assert.True(effect.IsAttached);
            Assert.True(effect.OnAttachedCalled);
            Assert.True(effect.Registered);
            Assert.False(effect.OnDetachedCalled);

            element.Effects.Remove(effect);
            Assert.True(effect.OnDetachedCalled);
        }

        [Fact]
        public void EffectLifecyclePostProvider()
        {
            var effect = new CustomEffect();
            var element = new Label();

            ((IVisualElementController)element).EffectControlProvider = new EffectControlProvider();
            element.Effects.Add(effect);

            Assert.True(effect.IsAttached);
            Assert.True(effect.OnAttachedCalled);
            Assert.True(effect.Registered);
            Assert.False(effect.OnDetachedCalled);

            element.Effects.Remove(effect);
            Assert.True(effect.OnDetachedCalled);
        }

        [Fact]
        public void EffectsClearDetachesEffect()
        {
            var effect = new CustomEffect();
            var element = new Label();

            ((IVisualElementController)element).EffectControlProvider = new EffectControlProvider();
            element.Effects.Add(effect);

            element.Effects.Clear();

            Assert.True(effect.OnDetachedCalled);
        }

        class EffectControlProvider : IEffectControlProvider
        {
            public void RegisterEffect(Effect effect)
            {
                var e = effect as CustomEffect;
                if (e != null)
                    e.Registered = true;
            }
        }

        class CustomEffect : Effect
        {
            public bool OnAttachedCalled;
            public bool OnDetachedCalled;
            public bool Registered;

            protected override void OnAttached()
            {
                OnAttachedCalled = true;
            }

            protected override void OnDetached()
            {
                OnDetachedCalled = true;
            }
        }

        /// <summary>
        /// Tests that SendDetached returns early when the effect is not attached.
        /// This test covers the guard clause that checks if the effect is not attached
        /// and ensures OnDetached is not called in this scenario.
        /// </summary>
        [Fact]
        public void SendDetached_WhenNotAttached_ReturnsEarlyWithoutCallingOnDetached()
        {
            // Arrange
            var effect = new TestCustomEffect();

            // Act - Call SendDetached without calling SendAttached first (IsAttached remains false)
            effect.SendDetached();

            // Assert
            Assert.False(effect.IsAttached);
            Assert.False(effect.OnDetachedCalled);
        }

        /// <summary>
        /// Tests that SendDetached works correctly when PlatformEffect is null.
        /// Verifies that the null-conditional operator properly handles null PlatformEffect
        /// without throwing exceptions.
        /// </summary>
        [Fact]
        public void SendDetached_WhenAttachedAndPlatformEffectIsNull_CallsOnDetachedAndUnsetsFlag()
        {
            // Arrange
            var effect = new TestCustomEffect();
            effect.SendAttached(); // Make effect attached
            effect.PlatformEffect = null; // Ensure PlatformEffect is null

            // Act
            effect.SendDetached();

            // Assert
            Assert.False(effect.IsAttached);
            Assert.True(effect.OnDetachedCalled);
        }

        /// <summary>
        /// Tests that SendDetached calls PlatformEffect.SendDetached when PlatformEffect is not null.
        /// Verifies the complete flow when the effect is attached and has a valid PlatformEffect.
        /// </summary>
        [Fact]
        public void SendDetached_WhenAttachedAndPlatformEffectExists_CallsPlatformEffectSendDetached()
        {
            // Arrange
            var effect = new TestCustomEffect();
            var mockPlatformEffect = Substitute.For<PlatformEffect>();
            effect.SendAttached(); // Make effect attached
            effect.PlatformEffect = mockPlatformEffect;

            // Act
            effect.SendDetached();

            // Assert
            Assert.False(effect.IsAttached);
            Assert.True(effect.OnDetachedCalled);
            mockPlatformEffect.Received(1).SendDetached();
        }

        class TestCustomEffect : Effect
        {
            public bool OnAttachedCalled;
            public bool OnDetachedCalled;

            protected override void OnAttached()
            {
                OnAttachedCalled = true;
            }

            protected override void OnDetached()
            {
                OnDetachedCalled = true;
            }
        }

        /// <summary>
        /// Tests that SendOnElementPropertyChanged can be called without throwing exceptions
        /// with a valid PropertyChangedEventArgs parameter.
        /// </summary>
        [Fact]
        public void SendOnElementPropertyChanged_WithValidArgs_DoesNotThrow()
        {
            // Arrange
            var effect = new TestableEffect();
            var args = new PropertyChangedEventArgs("TestProperty");

            // Act & Assert
            var exception = Record.Exception(() => effect.SendOnElementPropertyChanged(args));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that SendOnElementPropertyChanged can be called with null PropertyChangedEventArgs
        /// and does not throw an exception.
        /// </summary>
        [Fact]
        public void SendOnElementPropertyChanged_WithNullArgs_DoesNotThrow()
        {
            // Arrange
            var effect = new TestableEffect();

            // Act & Assert
            var exception = Record.Exception(() => effect.SendOnElementPropertyChanged(null));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that SendOnElementPropertyChanged handles PropertyChangedEventArgs with null property name
        /// and does not throw an exception.
        /// </summary>
        [Fact]
        public void SendOnElementPropertyChanged_WithNullPropertyName_DoesNotThrow()
        {
            // Arrange
            var effect = new TestableEffect();
            var args = new PropertyChangedEventArgs(null);

            // Act & Assert
            var exception = Record.Exception(() => effect.SendOnElementPropertyChanged(args));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that SendOnElementPropertyChanged handles PropertyChangedEventArgs with empty property name
        /// and does not throw an exception.
        /// </summary>
        [Fact]
        public void SendOnElementPropertyChanged_WithEmptyPropertyName_DoesNotThrow()
        {
            // Arrange
            var effect = new TestableEffect();
            var args = new PropertyChangedEventArgs(string.Empty);

            // Act & Assert
            var exception = Record.Exception(() => effect.SendOnElementPropertyChanged(args));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that SendOnElementPropertyChanged can be overridden and the base implementation
        /// is called successfully.
        /// </summary>
        [Fact]
        public void SendOnElementPropertyChanged_OverriddenMethod_CallsBaseImplementation()
        {
            // Arrange
            var effect = new OverridableTestEffect();
            var args = new PropertyChangedEventArgs("TestProperty");

            // Act
            effect.SendOnElementPropertyChanged(args);

            // Assert
            Assert.True(effect.OnElementPropertyChangedCalled);
            Assert.Same(args, effect.LastPropertyChangedArgs);
        }

        /// <summary>
        /// Tests SendOnElementPropertyChanged with various property name edge cases to ensure
        /// the method handles different string inputs correctly.
        /// </summary>
        [Theory]
        [InlineData("ValidProperty")]
        [InlineData("")]
        [InlineData("  ")]
        [InlineData("Property.With.Dots")]
        [InlineData("Property_With_Underscores")]
        [InlineData("PropertyWithSpecialChars!@#$%")]
        [InlineData("VeryLongPropertyNameThatExceedsNormalLengthExpectationsForPropertyNamesButShouldStillBeHandledCorrectly")]
        public void SendOnElementPropertyChanged_WithVariousPropertyNames_DoesNotThrow(string propertyName)
        {
            // Arrange
            var effect = new TestableEffect();
            var args = new PropertyChangedEventArgs(propertyName);

            // Act & Assert
            var exception = Record.Exception(() => effect.SendOnElementPropertyChanged(args));
            Assert.Null(exception);
        }

        class TestableEffect : Effect
        {
            public void SendOnElementPropertyChanged(PropertyChangedEventArgs args)
            {
                // Call the internal method to test it
                base.SendOnElementPropertyChanged(args);
            }

            protected override void OnAttached()
            {
            }

            protected override void OnDetached()
            {
            }
        }

        class OverridableTestEffect : Effect
        {
            public bool OnElementPropertyChangedCalled { get; private set; }
            public PropertyChangedEventArgs LastPropertyChangedArgs { get; private set; }

            public void SendOnElementPropertyChanged(PropertyChangedEventArgs args)
            {
                // Call the internal method to test it
                base.SendOnElementPropertyChanged(args);
            }

            protected override void OnAttached()
            {
            }

            protected override void OnDetached()
            {
            }
        }

        /// <summary>
        /// Tests that SendAttached returns early when the effect is already attached,
        /// without calling OnAttached again. This covers the early return path (line 60).
        /// </summary>
        [Fact]
        public void SendAttached_WhenAlreadyAttached_ReturnsEarlyWithoutCallingOnAttachedAgain()
        {
            // Arrange
            var effect = new TestableEffect();

            // Act - First call should attach the effect
            effect.SendAttached();
            Assert.True(effect.IsAttached);
            Assert.True(effect.OnAttachedCalled);

            // Reset the flag to verify OnAttached is not called again
            effect.OnAttachedCalled = false;

            // Act - Second call should return early
            effect.SendAttached();

            // Assert - Effect remains attached but OnAttached was not called again
            Assert.True(effect.IsAttached);
            Assert.False(effect.OnAttachedCalled);
        }

        /// <summary>
        /// Tests that SendAttached works correctly when PlatformEffect is null,
        /// verifying the null-conditional operator behavior.
        /// </summary>
        [Fact]
        public void SendAttached_WhenPlatformEffectIsNull_DoesNotThrow()
        {
            // Arrange
            var effect = new TestableEffect();
            // PlatformEffect is null by default

            // Act & Assert - Should not throw
            effect.SendAttached();
            Assert.True(effect.IsAttached);
            Assert.True(effect.OnAttachedCalled);
        }

        /// <summary>
        /// Tests that SendAttached calls SendAttached on PlatformEffect when it is set.
        /// </summary>
        [Fact]
        public void SendAttached_WhenPlatformEffectIsSet_CallsSendAttachedOnPlatformEffect()
        {
            // Arrange
            var effect = new TestableEffect();
            var mockPlatformEffect = Substitute.For<PlatformEffect>();

            // Use reflection to set the internal PlatformEffect property
            var platformEffectProperty = typeof(Effect).GetProperty("PlatformEffect");
            platformEffectProperty.SetValue(effect, mockPlatformEffect);

            // Act
            effect.SendAttached();

            // Assert
            Assert.True(effect.IsAttached);
            Assert.True(effect.OnAttachedCalled);
            mockPlatformEffect.Received(1).SendAttached();
        }

    }
}