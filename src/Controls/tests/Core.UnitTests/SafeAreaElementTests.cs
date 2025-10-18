using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using NSubstitute;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public partial class SafeAreaElementTests
    {
        /// <summary>
        /// Tests that ShouldObeySafeAreaForEdge returns true when SafeAreaRegions.All is set for any edge.
        /// Input: BindableObject with SafeAreaEdges set to All, valid edge values (0-3).
        /// Expected: Returns true for all edges.
        /// </summary>
        [Theory]
        [InlineData(0)] // Left
        [InlineData(1)] // Top
        [InlineData(2)] // Right
        [InlineData(3)] // Bottom
        public void ShouldObeySafeAreaForEdge_SafeAreaRegionsAll_ReturnsTrue(int edge)
        {
            var layout = new Grid();
            layout.SafeAreaEdges = SafeAreaEdges.All;

            bool result = SafeAreaElement.ShouldObeySafeAreaForEdge(layout, edge);

            Assert.True(result);
        }

        /// <summary>
        /// Tests that ShouldObeySafeAreaForEdge returns true when SafeAreaRegions.Container is set for any edge.
        /// Input: BindableObject with SafeAreaEdges set to Container, valid edge values (0-3).
        /// Expected: Returns true for all edges.
        /// </summary>
        [Theory]
        [InlineData(0)] // Left
        [InlineData(1)] // Top
        [InlineData(2)] // Right
        [InlineData(3)] // Bottom
        public void ShouldObeySafeAreaForEdge_SafeAreaRegionsContainer_ReturnsTrue(int edge)
        {
            var layout = new Grid();
            layout.SafeAreaEdges = new SafeAreaEdges(SafeAreaRegions.Container);

            bool result = SafeAreaElement.ShouldObeySafeAreaForEdge(layout, edge);

            Assert.True(result);
        }

        /// <summary>
        /// Tests that ShouldObeySafeAreaForEdge returns true when SafeAreaRegions.SoftInput is set for any edge.
        /// Input: BindableObject with SafeAreaEdges set to SoftInput, valid edge values (0-3).
        /// Expected: Returns true for all edges.
        /// </summary>
        [Theory]
        [InlineData(0)] // Left
        [InlineData(1)] // Top
        [InlineData(2)] // Right
        [InlineData(3)] // Bottom
        public void ShouldObeySafeAreaForEdge_SafeAreaRegionsSoftInput_ReturnsTrue(int edge)
        {
            var layout = new Grid();
            layout.SafeAreaEdges = new SafeAreaEdges(SafeAreaRegions.SoftInput);

            bool result = SafeAreaElement.ShouldObeySafeAreaForEdge(layout, edge);

            Assert.True(result);
        }

        /// <summary>
        /// Tests that ShouldObeySafeAreaForEdge returns true when SafeAreaRegions.Default is set for any edge.
        /// Input: BindableObject with SafeAreaEdges set to Default, valid edge values (0-3).
        /// Expected: Returns true for all edges.
        /// </summary>
        [Theory]
        [InlineData(0)] // Left
        [InlineData(1)] // Top
        [InlineData(2)] // Right
        [InlineData(3)] // Bottom
        public void ShouldObeySafeAreaForEdge_SafeAreaRegionsDefault_ReturnsTrue(int edge)
        {
            var layout = new Grid();
            layout.SafeAreaEdges = new SafeAreaEdges(SafeAreaRegions.Default);

            bool result = SafeAreaElement.ShouldObeySafeAreaForEdge(layout, edge);

            Assert.True(result);
        }

        /// <summary>
        /// Tests that ShouldObeySafeAreaForEdge returns false when SafeAreaRegions.None is set for any edge.
        /// Input: BindableObject with SafeAreaEdges set to None, valid edge values (0-3).
        /// Expected: Returns false for all edges.
        /// </summary>
        [Theory]
        [InlineData(0)] // Left
        [InlineData(1)] // Top
        [InlineData(2)] // Right
        [InlineData(3)] // Bottom
        public void ShouldObeySafeAreaForEdge_SafeAreaRegionsNone_ReturnsFalse(int edge)
        {
            var layout = new Grid();
            layout.SafeAreaEdges = new SafeAreaEdges(SafeAreaRegions.None);

            bool result = SafeAreaElement.ShouldObeySafeAreaForEdge(layout, edge);

            Assert.False(result);
        }

        /// <summary>
        /// Tests that ShouldObeySafeAreaForEdge returns false when bindable implements ISafeAreaView with IgnoreSafeArea = true.
        /// Input: Mock BindableObject implementing ISafeAreaView with IgnoreSafeArea = true, edge = 0.
        /// Expected: Returns false (inverted logic from legacy behavior).
        /// </summary>
        [Fact]
        public void ShouldObeySafeAreaForEdge_LegacySafeAreaViewIgnoreTrue_ReturnsFalse()
        {
            var mockBindable = Substitute.For<BindableObject, ISafeAreaView>();
            var safeAreaView = (ISafeAreaView)mockBindable;
            safeAreaView.IgnoreSafeArea.Returns(true);
            mockBindable.GetValue(SafeAreaElement.SafeAreaEdgesProperty).Returns(new SafeAreaEdges(SafeAreaRegions.None));

            bool result = SafeAreaElement.ShouldObeySafeAreaForEdge(mockBindable, 0);

            Assert.False(result);
        }

        /// <summary>
        /// Tests that ShouldObeySafeAreaForEdge returns true when bindable implements ISafeAreaView with IgnoreSafeArea = false.
        /// Input: Mock BindableObject implementing ISafeAreaView with IgnoreSafeArea = false, edge = 0.
        /// Expected: Returns true (inverted logic from legacy behavior).
        /// </summary>
        [Fact]
        public void ShouldObeySafeAreaForEdge_LegacySafeAreaViewIgnoreFalse_ReturnsTrue()
        {
            var mockBindable = Substitute.For<BindableObject, ISafeAreaView>();
            var safeAreaView = (ISafeAreaView)mockBindable;
            safeAreaView.IgnoreSafeArea.Returns(false);
            mockBindable.GetValue(SafeAreaElement.SafeAreaEdgesProperty).Returns(new SafeAreaEdges(SafeAreaRegions.None));

            bool result = SafeAreaElement.ShouldObeySafeAreaForEdge(mockBindable, 0);

            Assert.True(result);
        }

        /// <summary>
        /// Tests that ShouldObeySafeAreaForEdge returns false as default fallback when none of the conditions match.
        /// Input: BindableObject that doesn't implement ISafeAreaView with SafeAreaEdges set to None, valid edge.
        /// Expected: Returns false as default behavior.
        /// </summary>
        [Fact]
        public void ShouldObeySafeAreaForEdge_DefaultFallback_ReturnsFalse()
        {
            var layout = new Grid();
            layout.SafeAreaEdges = new SafeAreaEdges(SafeAreaRegions.None);

            bool result = SafeAreaElement.ShouldObeySafeAreaForEdge(layout, 0);

            Assert.False(result);
        }

        /// <summary>
        /// Tests that ShouldObeySafeAreaForEdge handles edge boundary values correctly.
        /// Input: Valid BindableObject with valid edge boundary values (0 and 3).
        /// Expected: Returns expected boolean based on SafeAreaRegions configuration.
        /// </summary>
        [Theory]
        [InlineData(0)] // Left edge - minimum valid value
        [InlineData(3)] // Bottom edge - maximum valid value
        public void ShouldObeySafeAreaForEdge_EdgeBoundaryValues_HandlesCorrectly(int edge)
        {
            var layout = new Grid();
            layout.SafeAreaEdges = SafeAreaEdges.All;

            bool result = SafeAreaElement.ShouldObeySafeAreaForEdge(layout, edge);

            Assert.True(result);
        }

        /// <summary>
        /// Tests that ShouldObeySafeAreaForEdge throws ArgumentNullException when bindable is null.
        /// Input: null bindable, valid edge value.
        /// Expected: Throws ArgumentNullException.
        /// </summary>
        [Fact]
        public void ShouldObeySafeAreaForEdge_NullBindable_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => SafeAreaElement.ShouldObeySafeAreaForEdge(null, 0));
        }

        /// <summary>
        /// Tests that ShouldObeySafeAreaForEdge handles extreme edge values.
        /// Input: Valid BindableObject with extreme edge values (negative, very large).
        /// Expected: Method should handle gracefully without throwing exceptions.
        /// </summary>
        [Theory]
        [InlineData(-1)]
        [InlineData(4)]
        [InlineData(int.MinValue)]
        [InlineData(int.MaxValue)]
        public void ShouldObeySafeAreaForEdge_ExtremeEdgeValues_HandlesGracefully(int edge)
        {
            var layout = new Grid();
            layout.SafeAreaEdges = SafeAreaEdges.All;

            // Should not throw an exception, though behavior may vary based on GetEdgeValue implementation
            var exception = Record.Exception(() => SafeAreaElement.ShouldObeySafeAreaForEdge(layout, edge));

            // Allow either successful execution or specific exceptions, but not unexpected crashes
            if (exception != null)
            {
                Assert.True(exception is ArgumentOutOfRangeException || exception is ArgumentException || exception is IndexOutOfRangeException);
            }
        }
    }
}
