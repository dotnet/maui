#nullable disable

using System;
using System.ComponentModel;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class EffectUtilitiesTests
    {
        /// <summary>
        /// Tests that UnregisterEffectControlProvider handles null element parameter gracefully.
        /// Should not throw exception due to null-conditional operator and should perform no operations.
        /// </summary>
        [Fact]
        public void UnregisterEffectControlProvider_NullElement_NoExceptionThrown()
        {
            // Arrange
            var self = Substitute.For<IEffectControlProvider>();

            // Act & Assert
            EffectUtilities.UnregisterEffectControlProvider(self, null);
            // No exception should be thrown and no operations should be performed
        }

        /// <summary>
        /// Tests that UnregisterEffectControlProvider does not modify element when EffectControlProvider is null.
        /// The condition (null == self) should be false when self is not null, so no action should be taken.
        /// </summary>
        [Fact]
        public void UnregisterEffectControlProvider_ElementWithNullEffectControlProvider_NoActionTaken()
        {
            // Arrange
            var self = Substitute.For<IEffectControlProvider>();
            var element = Substitute.For<IElementController>();
            element.EffectControlProvider.Returns((IEffectControlProvider)null);

            // Act
            EffectUtilities.UnregisterEffectControlProvider(self, element);

            // Assert
            element.DidNotReceive().EffectControlProvider = Arg.Any<IEffectControlProvider>();
        }

        /// <summary>
        /// Tests that UnregisterEffectControlProvider does not modify element when EffectControlProvider differs from self.
        /// The condition (differentProvider == self) should be false, so no action should be taken.
        /// </summary>
        [Fact]
        public void UnregisterEffectControlProvider_ElementWithDifferentEffectControlProvider_NoActionTaken()
        {
            // Arrange
            var self = Substitute.For<IEffectControlProvider>();
            var differentProvider = Substitute.For<IEffectControlProvider>();
            var element = Substitute.For<IElementController>();
            element.EffectControlProvider.Returns(differentProvider);

            // Act
            EffectUtilities.UnregisterEffectControlProvider(self, element);

            // Assert
            element.DidNotReceive().EffectControlProvider = Arg.Any<IEffectControlProvider>();
        }

        /// <summary>
        /// Tests that UnregisterEffectControlProvider sets EffectControlProvider to null when it matches self.
        /// The condition (self == self) should be true, so EffectControlProvider should be set to null.
        /// </summary>
        [Fact]
        public void UnregisterEffectControlProvider_ElementWithMatchingEffectControlProvider_SetsToNull()
        {
            // Arrange
            var self = Substitute.For<IEffectControlProvider>();
            var element = Substitute.For<IElementController>();
            element.EffectControlProvider.Returns(self);

            // Act
            EffectUtilities.UnregisterEffectControlProvider(self, element);

            // Assert
            element.Received(1).EffectControlProvider = null;
        }

        /// <summary>
        /// Tests that UnregisterEffectControlProvider sets EffectControlProvider to null when both self and EffectControlProvider are null.
        /// The condition (null == null) should be true, so EffectControlProvider should be set to null.
        /// </summary>
        [Fact]
        public void UnregisterEffectControlProvider_BothSelfAndEffectControlProviderNull_SetsToNull()
        {
            // Arrange
            IEffectControlProvider self = null;
            var element = Substitute.For<IElementController>();
            element.EffectControlProvider.Returns((IEffectControlProvider)null);

            // Act
            EffectUtilities.UnregisterEffectControlProvider(self, element);

            // Assert
            element.Received(1).EffectControlProvider = null;
        }

        /// <summary>
        /// Tests that UnregisterEffectControlProvider does not modify element when self is null but EffectControlProvider is not null.
        /// The condition (provider == null) should be false, so no action should be taken.
        /// </summary>
        [Fact]
        public void UnregisterEffectControlProvider_SelfNullButEffectControlProviderNotNull_NoActionTaken()
        {
            // Arrange
            IEffectControlProvider self = null;
            var provider = Substitute.For<IEffectControlProvider>();
            var element = Substitute.For<IElementController>();
            element.EffectControlProvider.Returns(provider);

            // Act
            EffectUtilities.UnregisterEffectControlProvider(self, element);

            // Assert
            element.DidNotReceive().EffectControlProvider = Arg.Any<IEffectControlProvider>();
        }

        /// <summary>
        /// Tests RegisterEffectControlProvider when both oldElement and newElement are null.
        /// Verifies that no operations are performed when both elements are null.
        /// Expected result: Method completes without throwing exceptions.
        /// </summary>
        [Fact]
        public void RegisterEffectControlProvider_BothElementsNull_CompletesWithoutErrors()
        {
            // Arrange
            var self = Substitute.For<IEffectControlProvider>();

            // Act & Assert
            EffectUtilities.RegisterEffectControlProvider(self, null, null);
        }

        /// <summary>
        /// Tests RegisterEffectControlProvider when oldElement is null and newElement is provided.
        /// Verifies that the effect control provider is set on the new element only.
        /// Expected result: newElement gets the effect control provider assigned.
        /// </summary>
        [Fact]
        public void RegisterEffectControlProvider_OldElementNullNewElementProvided_SetsProviderOnNewElement()
        {
            // Arrange
            var self = Substitute.For<IEffectControlProvider>();
            var newElement = Substitute.For<IElementController>();

            // Act
            EffectUtilities.RegisterEffectControlProvider(self, null, newElement);

            // Assert
            newElement.Received(1).EffectControlProvider = self;
        }

        /// <summary>
        /// Tests RegisterEffectControlProvider when oldElement is provided and newElement is null.
        /// Verifies that the old element's provider is cleared when it matches self, but nothing is set on newElement.
        /// Expected result: oldElement's provider is cleared if it matches self.
        /// </summary>
        [Fact]
        public void RegisterEffectControlProvider_OldElementProvidedNewElementNull_ClearsOldElementIfMatches()
        {
            // Arrange
            var self = Substitute.For<IEffectControlProvider>();
            var oldElement = Substitute.For<IElementController>();
            oldElement.EffectControlProvider.Returns(self);

            // Act
            EffectUtilities.RegisterEffectControlProvider(self, oldElement, null);

            // Assert
            oldElement.Received(1).EffectControlProvider = null;
        }

        /// <summary>
        /// Tests RegisterEffectControlProvider when oldElement has a different provider than self.
        /// Verifies that the old element's provider is not modified when it doesn't match self.
        /// Expected result: oldElement's provider remains unchanged, newElement gets the provider.
        /// </summary>
        [Fact]
        public void RegisterEffectControlProvider_OldElementHasDifferentProvider_DoesNotClearOldElement()
        {
            // Arrange
            var self = Substitute.For<IEffectControlProvider>();
            var differentProvider = Substitute.For<IEffectControlProvider>();
            var oldElement = Substitute.For<IElementController>();
            var newElement = Substitute.For<IElementController>();
            oldElement.EffectControlProvider.Returns(differentProvider);

            // Act
            EffectUtilities.RegisterEffectControlProvider(self, oldElement, newElement);

            // Assert
            oldElement.DidNotReceive().EffectControlProvider = null;
            newElement.Received(1).EffectControlProvider = self;
        }

        /// <summary>
        /// Tests RegisterEffectControlProvider when oldElement has null provider.
        /// Verifies that nothing is cleared from old element when its provider is already null.
        /// Expected result: oldElement is not modified, newElement gets the provider.
        /// </summary>
        [Fact]
        public void RegisterEffectControlProvider_OldElementHasNullProvider_DoesNotClearOldElement()
        {
            // Arrange
            var self = Substitute.For<IEffectControlProvider>();
            var oldElement = Substitute.For<IElementController>();
            var newElement = Substitute.For<IElementController>();
            oldElement.EffectControlProvider.Returns((IEffectControlProvider)null);

            // Act
            EffectUtilities.RegisterEffectControlProvider(self, oldElement, newElement);

            // Assert
            oldElement.DidNotReceive().EffectControlProvider = null;
            newElement.Received(1).EffectControlProvider = self;
        }

        /// <summary>
        /// Tests RegisterEffectControlProvider when oldElement matches self and newElement is provided.
        /// Verifies complete provider transfer from old to new element.
        /// Expected result: oldElement's provider is cleared and newElement gets the provider.
        /// </summary>
        [Fact]
        public void RegisterEffectControlProvider_OldElementMatchesSelfNewElementProvided_TransfersProvider()
        {
            // Arrange
            var self = Substitute.For<IEffectControlProvider>();
            var oldElement = Substitute.For<IElementController>();
            var newElement = Substitute.For<IElementController>();
            oldElement.EffectControlProvider.Returns(self);

            // Act
            EffectUtilities.RegisterEffectControlProvider(self, oldElement, newElement);

            // Assert
            oldElement.Received(1).EffectControlProvider = null;
            newElement.Received(1).EffectControlProvider = self;
        }

        /// <summary>
        /// Tests RegisterEffectControlProvider when self parameter is null.
        /// Verifies that null provider is properly handled and assigned.
        /// Expected result: Method completes and null is assigned where appropriate.
        /// </summary>
        [Fact]
        public void RegisterEffectControlProvider_SelfIsNull_HandlesNullProvider()
        {
            // Arrange
            var oldElement = Substitute.For<IElementController>();
            var newElement = Substitute.For<IElementController>();
            oldElement.EffectControlProvider.Returns((IEffectControlProvider)null);

            // Act
            EffectUtilities.RegisterEffectControlProvider(null, oldElement, newElement);

            // Assert
            oldElement.DidNotReceive().EffectControlProvider = null;
            newElement.Received(1).EffectControlProvider = null;
        }

        /// <summary>
        /// Tests RegisterEffectControlProvider when oldElement has null provider matching self (null).
        /// Verifies that null-to-null comparison works correctly.
        /// Expected result: oldElement's provider is cleared since null equals null, newElement gets null provider.
        /// </summary>
        [Fact]
        public void RegisterEffectControlProvider_SelfIsNullAndOldElementHasNullProvider_ClearsOldElement()
        {
            // Arrange
            var oldElement = Substitute.For<IElementController>();
            var newElement = Substitute.For<IElementController>();
            oldElement.EffectControlProvider.Returns((IEffectControlProvider)null);

            // Act
            EffectUtilities.RegisterEffectControlProvider(null, oldElement, newElement);

            // Assert
            oldElement.DidNotReceive().EffectControlProvider = null; // Since it's already null, no need to set it again
            newElement.Received(1).EffectControlProvider = null;
        }
    }
}