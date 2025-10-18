using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Platform;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public partial class SemanticExtensionsTests
    {
        /// <summary>
        /// Tests that HasAccessibleTapGesture throws ArgumentNullException when virtualView is null.
        /// Input: null virtualView
        /// Expected: ArgumentNullException
        /// </summary>
        [Fact]
        public void HasAccessibleTapGesture_NullVirtualView_ThrowsArgumentNullException()
        {
            // Arrange
            View nullView = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => SemanticExtensions.HasAccessibleTapGesture(nullView, out _));
        }

        /// <summary>
        /// Tests that HasAccessibleTapGesture returns false when GestureRecognizers collection is empty.
        /// Input: View with empty GestureRecognizers collection
        /// Expected: false return value, null out parameter
        /// </summary>
        [Fact]
        public void HasAccessibleTapGesture_EmptyGestureRecognizers_ReturnsFalse()
        {
            // Arrange
            var view = new TestView();

            // Act
            bool result = view.HasAccessibleTapGesture(out TapGestureRecognizer tapGestureRecognizer);

            // Assert
            Assert.False(result);
            Assert.Null(tapGestureRecognizer);
        }

        /// <summary>
        /// Tests that HasAccessibleTapGesture returns false when no TapGestureRecognizers are present.
        /// Input: View with non-TapGestureRecognizer gestures
        /// Expected: false return value, null out parameter
        /// </summary>
        [Fact]
        public void HasAccessibleTapGesture_NoTapGestureRecognizers_ReturnsFalse()
        {
            // Arrange
            var view = new TestView();
            var nonTapGesture = Substitute.For<IGestureRecognizer>();
            view.GestureRecognizers.Add(nonTapGesture);

            // Act
            bool result = view.HasAccessibleTapGesture(out TapGestureRecognizer tapGestureRecognizer);

            // Assert
            Assert.False(result);
            Assert.Null(tapGestureRecognizer);
        }

        /// <summary>
        /// Tests that HasAccessibleTapGesture returns false when TapGestureRecognizer has invalid NumberOfTapsRequired.
        /// Input: TapGestureRecognizer with various invalid NumberOfTapsRequired values
        /// Expected: false return value, null out parameter
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(-1)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        public void HasAccessibleTapGesture_InvalidNumberOfTapsRequired_ReturnsFalse(int numberOfTaps)
        {
            // Arrange
            var view = new TestView();
            var tapGesture = new TapGestureRecognizer
            {
                NumberOfTapsRequired = numberOfTaps,
                Buttons = ButtonsMask.Primary
            };
            view.GestureRecognizers.Add(tapGesture);

            // Act
            bool result = view.HasAccessibleTapGesture(out TapGestureRecognizer tapGestureRecognizer);

            // Assert
            Assert.False(result);
            Assert.Null(tapGestureRecognizer);
        }

        /// <summary>
        /// Tests that HasAccessibleTapGesture returns false when TapGestureRecognizer doesn't have Primary button.
        /// Input: TapGestureRecognizer with NumberOfTapsRequired=1 but without Primary button
        /// Expected: false return value, null out parameter
        /// </summary>
        [Theory]
        [InlineData((ButtonsMask)0)]
        [InlineData(ButtonsMask.Secondary)]
        [InlineData((ButtonsMask)999)]
        public void HasAccessibleTapGesture_NoPrimaryButton_ReturnsFalse(ButtonsMask buttons)
        {
            // Arrange
            var view = new TestView();
            var tapGesture = new TapGestureRecognizer
            {
                NumberOfTapsRequired = 1,
                Buttons = buttons
            };
            view.GestureRecognizers.Add(tapGesture);

            // Act
            bool result = view.HasAccessibleTapGesture(out TapGestureRecognizer tapGestureRecognizer);

            // Assert
            Assert.False(result);
            Assert.Null(tapGestureRecognizer);
        }

        /// <summary>
        /// Tests that HasAccessibleTapGesture returns true when TapGestureRecognizer has valid configuration.
        /// Input: TapGestureRecognizer with NumberOfTapsRequired=1 and Primary button set
        /// Expected: true return value, TapGestureRecognizer out parameter
        /// </summary>
        [Theory]
        [InlineData(ButtonsMask.Primary)]
        [InlineData(ButtonsMask.Primary | ButtonsMask.Secondary)]
        public void HasAccessibleTapGesture_ValidTapGesture_ReturnsTrue(ButtonsMask buttons)
        {
            // Arrange
            var view = new TestView();
            var tapGesture = new TapGestureRecognizer
            {
                NumberOfTapsRequired = 1,
                Buttons = buttons
            };
            view.GestureRecognizers.Add(tapGesture);

            // Act
            bool result = view.HasAccessibleTapGesture(out TapGestureRecognizer tapGestureRecognizer);

            // Assert
            Assert.True(result);
            Assert.Same(tapGesture, tapGestureRecognizer);
        }

        /// <summary>
        /// Tests that HasAccessibleTapGesture returns first valid TapGestureRecognizer when multiple are present.
        /// Input: Multiple TapGestureRecognizers, some valid, some invalid
        /// Expected: true return value, first valid TapGestureRecognizer out parameter
        /// </summary>
        [Fact]
        public void HasAccessibleTapGesture_MultipleGestures_ReturnsFirstValid()
        {
            // Arrange
            var view = new TestView();

            var invalidTapGesture1 = new TapGestureRecognizer
            {
                NumberOfTapsRequired = 2,
                Buttons = ButtonsMask.Primary
            };

            var invalidTapGesture2 = new TapGestureRecognizer
            {
                NumberOfTapsRequired = 1,
                Buttons = ButtonsMask.Secondary
            };

            var validTapGesture = new TapGestureRecognizer
            {
                NumberOfTapsRequired = 1,
                Buttons = ButtonsMask.Primary
            };

            var anotherValidTapGesture = new TapGestureRecognizer
            {
                NumberOfTapsRequired = 1,
                Buttons = ButtonsMask.Primary
            };

            view.GestureRecognizers.Add(invalidTapGesture1);
            view.GestureRecognizers.Add(invalidTapGesture2);
            view.GestureRecognizers.Add(validTapGesture);
            view.GestureRecognizers.Add(anotherValidTapGesture);

            // Act
            bool result = view.HasAccessibleTapGesture(out TapGestureRecognizer tapGestureRecognizer);

            // Assert
            Assert.True(result);
            Assert.Same(validTapGesture, tapGestureRecognizer);
        }

        /// <summary>
        /// Tests that HasAccessibleTapGesture handles mixed gesture types correctly.
        /// Input: Collection with various gesture types including valid TapGestureRecognizer
        /// Expected: true return value, valid TapGestureRecognizer out parameter
        /// </summary>
        [Fact]
        public void HasAccessibleTapGesture_MixedGestureTypes_FindsValidTapGesture()
        {
            // Arrange
            var view = new TestView();
            var nonTapGesture1 = Substitute.For<IGestureRecognizer>();
            var nonTapGesture2 = Substitute.For<IGestureRecognizer>();

            var validTapGesture = new TapGestureRecognizer
            {
                NumberOfTapsRequired = 1,
                Buttons = ButtonsMask.Primary
            };

            view.GestureRecognizers.Add(nonTapGesture1);
            view.GestureRecognizers.Add(validTapGesture);
            view.GestureRecognizers.Add(nonTapGesture2);

            // Act
            bool result = view.HasAccessibleTapGesture(out TapGestureRecognizer tapGestureRecognizer);

            // Assert
            Assert.True(result);
            Assert.Same(validTapGesture, tapGestureRecognizer);
        }

        /// <summary>
        /// Helper class that inherits from View to enable testing.
        /// Provides a concrete implementation with accessible GestureRecognizers collection.
        /// </summary>
        private class TestView : View
        {
            public TestView()
            {
                GestureRecognizers = new List<IGestureRecognizer>();
            }

            public new IList<IGestureRecognizer> GestureRecognizers { get; set; }
        }

        /// <summary>
        /// Tests that HasAccessibleTapGesture returns false when View has no gesture recognizers.
        /// Input: View with empty GestureRecognizers collection
        /// Expected: Returns false
        /// </summary>
        [Fact]
        public void HasAccessibleTapGesture_NoGestureRecognizers_ReturnsFalse()
        {
            // Arrange
            var virtualView = new TestView();

            // Act
            var result = virtualView.HasAccessibleTapGesture();

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that HasAccessibleTapGesture returns false when View has gesture recognizers but no TapGestureRecognizer.
        /// Input: View with non-TapGestureRecognizer
        /// Expected: Returns false
        /// </summary>
        [Fact]
        public void HasAccessibleTapGesture_NoTapGestureRecognizer_ReturnsFalse()
        {
            // Arrange
            var virtualView = new TestView();
            var panGesture = Substitute.For<IGestureRecognizer>();
            virtualView.GestureRecognizers.Add(panGesture);

            // Act
            var result = virtualView.HasAccessibleTapGesture();

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that HasAccessibleTapGesture returns false when TapGestureRecognizer has NumberOfTapsRequired greater than 1.
        /// Input: View with TapGestureRecognizer having NumberOfTapsRequired = 2
        /// Expected: Returns false
        /// </summary>
        [Fact]
        public void HasAccessibleTapGesture_TapGestureRequiresMultipleTaps_ReturnsFalse()
        {
            // Arrange
            var virtualView = new TestView();
            var tapGesture = new TapGestureRecognizer { NumberOfTapsRequired = 2 };
            virtualView.GestureRecognizers.Add(tapGesture);

            // Act
            var result = virtualView.HasAccessibleTapGesture();

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that HasAccessibleTapGesture returns false when TapGestureRecognizer has NumberOfTapsRequired = 0.
        /// Input: View with TapGestureRecognizer having NumberOfTapsRequired = 0
        /// Expected: Returns false
        /// </summary>
        [Fact]
        public void HasAccessibleTapGesture_TapGestureRequiresZeroTaps_ReturnsFalse()
        {
            // Arrange
            var virtualView = new TestView();
            var tapGesture = new TapGestureRecognizer { NumberOfTapsRequired = 0 };
            virtualView.GestureRecognizers.Add(tapGesture);

            // Act
            var result = virtualView.HasAccessibleTapGesture();

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that HasAccessibleTapGesture returns false when TapGestureRecognizer doesn't have Primary button.
        /// Input: View with TapGestureRecognizer having NumberOfTapsRequired = 1 but Buttons = Secondary
        /// Expected: Returns false
        /// </summary>
        [Fact]
        public void HasAccessibleTapGesture_TapGestureNonPrimaryButton_ReturnsFalse()
        {
            // Arrange
            var virtualView = new TestView();
            var tapGesture = new TapGestureRecognizer
            {
                NumberOfTapsRequired = 1,
                Buttons = ButtonsMask.Secondary
            };
            virtualView.GestureRecognizers.Add(tapGesture);

            // Act
            var result = virtualView.HasAccessibleTapGesture();

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that HasAccessibleTapGesture returns true when TapGestureRecognizer has NumberOfTapsRequired = 1 and Primary button.
        /// Input: View with valid TapGestureRecognizer (NumberOfTapsRequired = 1, Buttons = Primary)
        /// Expected: Returns true
        /// </summary>
        [Fact]
        public void HasAccessibleTapGesture_ValidTapGesture_ReturnsTrue()
        {
            // Arrange
            var virtualView = new TestView();
            var tapGesture = new TapGestureRecognizer
            {
                NumberOfTapsRequired = 1,
                Buttons = ButtonsMask.Primary
            };
            virtualView.GestureRecognizers.Add(tapGesture);

            // Act
            var result = virtualView.HasAccessibleTapGesture();

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that HasAccessibleTapGesture returns true when TapGestureRecognizer has Primary and other buttons.
        /// Input: View with TapGestureRecognizer having NumberOfTapsRequired = 1 and Buttons including Primary
        /// Expected: Returns true
        /// </summary>
        [Fact]
        public void HasAccessibleTapGesture_TapGesturePrimaryAndOtherButtons_ReturnsTrue()
        {
            // Arrange
            var virtualView = new TestView();
            var tapGesture = new TapGestureRecognizer
            {
                NumberOfTapsRequired = 1,
                Buttons = ButtonsMask.Primary | ButtonsMask.Secondary
            };
            virtualView.GestureRecognizers.Add(tapGesture);

            // Act
            var result = virtualView.HasAccessibleTapGesture();

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that HasAccessibleTapGesture returns true for first valid TapGestureRecognizer when multiple exist.
        /// Input: View with multiple gesture recognizers including valid TapGestureRecognizer
        /// Expected: Returns true
        /// </summary>
        [Fact]
        public void HasAccessibleTapGesture_MultipleGesturesWithValidTap_ReturnsTrue()
        {
            // Arrange
            var virtualView = new TestView();
            var panGesture = Substitute.For<IGestureRecognizer>();
            var invalidTapGesture = new TapGestureRecognizer { NumberOfTapsRequired = 2 };
            var validTapGesture = new TapGestureRecognizer
            {
                NumberOfTapsRequired = 1,
                Buttons = ButtonsMask.Primary
            };

            virtualView.GestureRecognizers.Add(panGesture);
            virtualView.GestureRecognizers.Add(invalidTapGesture);
            virtualView.GestureRecognizers.Add(validTapGesture);

            // Act
            var result = virtualView.HasAccessibleTapGesture();

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that HasAccessibleTapGesture returns false when all TapGestureRecognizers are invalid.
        /// Input: View with multiple invalid TapGestureRecognizers
        /// Expected: Returns false
        /// </summary>
        [Fact]
        public void HasAccessibleTapGesture_MultipleInvalidTapGestures_ReturnsFalse()
        {
            // Arrange
            var virtualView = new TestView();
            var doubleTapGesture = new TapGestureRecognizer { NumberOfTapsRequired = 2 };
            var nonPrimaryTapGesture = new TapGestureRecognizer
            {
                NumberOfTapsRequired = 1,
                Buttons = ButtonsMask.Secondary
            };

            virtualView.GestureRecognizers.Add(doubleTapGesture);
            virtualView.GestureRecognizers.Add(nonPrimaryTapGesture);

            // Act
            var result = virtualView.HasAccessibleTapGesture();

            // Assert
            Assert.False(result);
        }

    }
}