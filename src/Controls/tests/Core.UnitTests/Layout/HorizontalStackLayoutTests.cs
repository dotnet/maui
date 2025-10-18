#nullable disable

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Layouts;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class HorizontalStackLayoutTests
    {
        /// <summary>
        /// Tests that CreateLayoutManager returns a non-null ILayoutManager instance.
        /// Verifies the method fulfills its contract of returning a valid layout manager.
        /// </summary>
        [Fact]
        public void CreateLayoutManager_WhenCalled_ReturnsNonNullILayoutManager()
        {
            // Arrange
            var testableHorizontalStackLayout = new TestableHorizontalStackLayout();

            // Act
            var result = testableHorizontalStackLayout.ExposedCreateLayoutManager();

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<ILayoutManager>(result);
        }

        /// <summary>
        /// Tests that CreateLayoutManager returns specifically a HorizontalStackLayoutManager instance.
        /// Verifies the method returns the correct concrete implementation type.
        /// </summary>
        [Fact]
        public void CreateLayoutManager_WhenCalled_ReturnsHorizontalStackLayoutManagerInstance()
        {
            // Arrange
            var testableHorizontalStackLayout = new TestableHorizontalStackLayout();

            // Act
            var result = testableHorizontalStackLayout.ExposedCreateLayoutManager();

            // Assert
            Assert.IsType<HorizontalStackLayoutManager>(result);
        }

        /// <summary>
        /// Tests that CreateLayoutManager returns a new instance each time it is called.
        /// Verifies the method does not cache or reuse layout manager instances.
        /// </summary>
        [Fact]
        public void CreateLayoutManager_WhenCalledMultipleTimes_ReturnsNewInstanceEachTime()
        {
            // Arrange
            var testableHorizontalStackLayout = new TestableHorizontalStackLayout();

            // Act
            var result1 = testableHorizontalStackLayout.ExposedCreateLayoutManager();
            var result2 = testableHorizontalStackLayout.ExposedCreateLayoutManager();

            // Assert
            Assert.NotSame(result1, result2);
        }

        /// <summary>
        /// Helper class that exposes the protected CreateLayoutManager method for testing purposes.
        /// </summary>
        private class TestableHorizontalStackLayout : HorizontalStackLayout
        {
            public ILayoutManager ExposedCreateLayoutManager()
            {
                return CreateLayoutManager();
            }
        }
    }
}
