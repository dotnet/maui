#nullable disable

using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Platform;
using NSubstitute;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests
{
    /// <summary>
    /// Tests for PlatformEffect class focusing on the SendAttached method.
    /// </summary>
    public partial class PlatformEffectTests
    {
        /// <summary>
        /// Tests that SendAttached throws InvalidOperationException when Element is null.
        /// Verifies the null check validation at the beginning of the method.
        /// </summary>
        [Fact]
        public void SendAttached_ElementIsNull_ThrowsInvalidOperationException()
        {
            // Arrange
            var effect = new TestPlatformEffect();
            // Element is null by default

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => effect.SendAttached());
            Assert.Equal("Element cannot be null here", exception.Message);
        }

        /// <summary>
        /// Tests SendAttached when Handler implements IViewHandler and ContainerView is not null.
        /// Verifies that Container is set to ContainerView when available.
        /// </summary>
        [Fact]
        public void SendAttached_HandlerIsIViewHandlerWithContainerView_SetsContainerToContainerView()
        {
            // Arrange
            var effect = new TestPlatformEffect();
            var element = Substitute.For<Element>();
            var viewHandler = Substitute.For<IViewHandler>();
            var platformView = new object();
            var containerView = new object();

            element.Handler.Returns(viewHandler);
            viewHandler.PlatformView.Returns(platformView);
            viewHandler.ContainerView.Returns(containerView);

            effect.SetElement(element);

            // Act
            effect.SendAttached();

            // Assert
            Assert.Same(platformView, effect.Control);
            Assert.Same(containerView, effect.Container);
            Assert.True(effect.OnAttachedCalled);
            Assert.True(effect.IsAttached);
        }

        /// <summary>
        /// Tests SendAttached when Handler implements IViewHandler but ContainerView is null.
        /// Verifies that Container is set to PlatformView when ContainerView is unavailable.
        /// </summary>
        [Fact]
        public void SendAttached_HandlerIsIViewHandlerWithNullContainerView_SetsContainerToPlatformView()
        {
            // Arrange
            var effect = new TestPlatformEffect();
            var element = Substitute.For<Element>();
            var viewHandler = Substitute.For<IViewHandler>();
            var platformView = new object();

            element.Handler.Returns(viewHandler);
            viewHandler.PlatformView.Returns(platformView);
            viewHandler.ContainerView.Returns((object)null);

            effect.SetElement(element);

            // Act
            effect.SendAttached();

            // Assert
            Assert.Same(platformView, effect.Control);
            Assert.Same(platformView, effect.Container);
            Assert.True(effect.OnAttachedCalled);
            Assert.True(effect.IsAttached);
        }

        /// <summary>
        /// Tests SendAttached when Handler does not implement IViewHandler.
        /// Verifies that Container is set to Control when Handler is not an IViewHandler.
        /// </summary>
        [Fact]
        public void SendAttached_HandlerIsNotIViewHandler_SetsContainerToControl()
        {
            // Arrange
            var effect = new TestPlatformEffect();
            var element = Substitute.For<Element>();
            var handler = Substitute.For<IElementHandler>();
            var platformView = new object();

            element.Handler.Returns(handler);
            handler.PlatformView.Returns(platformView);

            effect.SetElement(element);

            // Act
            effect.SendAttached();

            // Assert
            Assert.Same(platformView, effect.Control);
            Assert.Same(platformView, effect.Container);
            Assert.True(effect.OnAttachedCalled);
            Assert.True(effect.IsAttached);
        }

        /// <summary>
        /// Tests that SendAttached properly calls base.SendAttached() which sets IsAttached to true and calls OnAttached.
        /// Verifies the complete flow including base class behavior.
        /// </summary>
        [Fact]
        public void SendAttached_ValidElement_CallsBaseSendAttachedAndSetsIsAttached()
        {
            // Arrange
            var effect = new TestPlatformEffect();
            var element = Substitute.For<Element>();
            var handler = Substitute.For<IElementHandler>();
            var platformView = new object();

            element.Handler.Returns(handler);
            handler.PlatformView.Returns(platformView);
            effect.SetElement(element);

            // Verify initial state
            Assert.False(effect.IsAttached);
            Assert.False(effect.OnAttachedCalled);

            // Act
            effect.SendAttached();

            // Assert - Verify base.SendAttached() effects
            Assert.True(effect.IsAttached);
            Assert.True(effect.OnAttachedCalled);
            Assert.Same(platformView, effect.Control);
        }

        /// <summary>
        /// Tests SendAttached with multiple calls to ensure idempotent behavior from base class.
        /// Verifies that multiple calls to SendAttached don't cause issues.
        /// </summary>
        [Fact]
        public void SendAttached_CalledMultipleTimes_DoesNotThrow()
        {
            // Arrange
            var effect = new TestPlatformEffect();
            var element = Substitute.For<Element>();
            var handler = Substitute.For<IElementHandler>();
            var platformView = new object();

            element.Handler.Returns(handler);
            handler.PlatformView.Returns(platformView);
            effect.SetElement(element);

            // Act - Call multiple times
            effect.SendAttached();
            effect.SendAttached();
            effect.SendAttached();

            // Assert - Should not throw and state should remain consistent
            Assert.True(effect.IsAttached);
            Assert.Same(platformView, effect.Control);
        }
    }
}
