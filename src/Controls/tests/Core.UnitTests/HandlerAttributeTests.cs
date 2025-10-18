#nullable disable

using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class HandlerAttributeTests
    {
        /// <summary>
        /// Tests that the HandlerAttribute constructor with handler and target parameters properly initializes the instance.
        /// Input: Valid Type instances for handler and target parameters.
        /// Expected: HandlerType and TargetType properties are set correctly, SupportedVisuals defaults to DefaultVisual, Priority is 0.
        /// </summary>
        [Fact]
        public void HandlerAttribute_ValidHandlerAndTarget_InitializesPropertiesCorrectly()
        {
            // Arrange
            var handlerType = typeof(string);
            var targetType = typeof(int);

            // Act
            var attribute = new TestHandlerAttribute(handlerType, targetType);

            // Assert
            Assert.Equal(handlerType, attribute.HandlerType);
            Assert.Equal(targetType, attribute.TargetType);
            Assert.Single(attribute.SupportedVisuals);
            Assert.Equal(typeof(VisualMarker.DefaultVisual), attribute.SupportedVisuals[0]);
            Assert.Equal(0, attribute.Priority);
        }

        /// <summary>
        /// Tests that the HandlerAttribute constructor handles null handler parameter.
        /// Input: null handler and valid target parameters.
        /// Expected: HandlerType is null, TargetType is set correctly, SupportedVisuals defaults to DefaultVisual.
        /// </summary>
        [Fact]
        public void HandlerAttribute_NullHandler_SetsHandlerTypeToNull()
        {
            // Arrange
            Type handlerType = null;
            var targetType = typeof(int);

            // Act
            var attribute = new TestHandlerAttribute(handlerType, targetType);

            // Assert
            Assert.Null(attribute.HandlerType);
            Assert.Equal(targetType, attribute.TargetType);
            Assert.Single(attribute.SupportedVisuals);
            Assert.Equal(typeof(VisualMarker.DefaultVisual), attribute.SupportedVisuals[0]);
        }

        /// <summary>
        /// Tests that the HandlerAttribute constructor handles null target parameter.
        /// Input: Valid handler and null target parameters.
        /// Expected: HandlerType is set correctly, TargetType is null, SupportedVisuals defaults to DefaultVisual.
        /// </summary>
        [Fact]
        public void HandlerAttribute_NullTarget_SetsTargetTypeToNull()
        {
            // Arrange
            var handlerType = typeof(string);
            Type targetType = null;

            // Act
            var attribute = new TestHandlerAttribute(handlerType, targetType);

            // Assert
            Assert.Equal(handlerType, attribute.HandlerType);
            Assert.Null(attribute.TargetType);
            Assert.Single(attribute.SupportedVisuals);
            Assert.Equal(typeof(VisualMarker.DefaultVisual), attribute.SupportedVisuals[0]);
        }

        /// <summary>
        /// Tests that the HandlerAttribute constructor handles both null parameters.
        /// Input: null handler and null target parameters.
        /// Expected: Both HandlerType and TargetType are null, SupportedVisuals defaults to DefaultVisual.
        /// </summary>
        [Fact]
        public void HandlerAttribute_BothParametersNull_SetsBothTypesToNull()
        {
            // Arrange
            Type handlerType = null;
            Type targetType = null;

            // Act
            var attribute = new TestHandlerAttribute(handlerType, targetType);

            // Assert
            Assert.Null(attribute.HandlerType);
            Assert.Null(attribute.TargetType);
            Assert.Single(attribute.SupportedVisuals);
            Assert.Equal(typeof(VisualMarker.DefaultVisual), attribute.SupportedVisuals[0]);
        }

        /// <summary>
        /// Test helper class that inherits from HandlerAttribute to enable testing of the protected constructor.
        /// </summary>
        private class TestHandlerAttribute : HandlerAttribute
        {
            public TestHandlerAttribute(Type handler, Type target) : base(handler, target)
            {
            }

            // Expose internal properties for testing
            public new Type HandlerType => base.HandlerType;
            public new Type TargetType => base.TargetType;
            public new Type[] SupportedVisuals => base.SupportedVisuals;
        }
    }
}
