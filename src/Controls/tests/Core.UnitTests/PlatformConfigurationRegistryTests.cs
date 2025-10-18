#nullable disable

using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using NSubstitute;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests
{
    /// <summary>
    /// Unit tests for PlatformConfigurationRegistry constructor.
    /// </summary>
    public class PlatformConfigurationRegistryTests
    {
        /// <summary>
        /// Tests that the constructor successfully creates an instance when provided with a valid element.
        /// The test verifies that no exception is thrown during construction with a valid Element-derived parameter.
        /// Expected result: Constructor completes successfully without throwing exceptions.
        /// </summary>
        [Fact]
        public void Constructor_WithValidElement_CreatesInstanceSuccessfully()
        {
            // Arrange
            var element = Substitute.For<Element>();

            // Act & Assert
            var registry = new PlatformConfigurationRegistry<Element>(element);

            Assert.NotNull(registry);
        }

        /// <summary>
        /// Tests that the constructor accepts a null element parameter without throwing an exception.
        /// Since nullable reference types are disabled, null should be accepted but may cause issues in usage.
        /// Expected result: Constructor completes without throwing, allowing null assignment.
        /// </summary>
        [Fact]
        public void Constructor_WithNullElement_AcceptsNullParameter()
        {
            // Arrange
            Element element = null;

            // Act & Assert
            var registry = new PlatformConfigurationRegistry<Element>(element);

            Assert.NotNull(registry);
        }

        /// <summary>
        /// Tests that the constructor works with different Element-derived types.
        /// Verifies that the generic constraint TElement : Element is properly handled.
        /// Expected result: Constructor creates instances successfully for various Element types.
        /// </summary>
        [Fact]
        public void Constructor_WithDifferentElementTypes_CreatesInstancesSuccessfully()
        {
            // Arrange
            var view = Substitute.For<View>();
            var layout = Substitute.For<Layout>();

            // Act
            var viewRegistry = new PlatformConfigurationRegistry<View>(view);
            var layoutRegistry = new PlatformConfigurationRegistry<Layout>(layout);

            // Assert
            Assert.NotNull(viewRegistry);
            Assert.NotNull(layoutRegistry);
        }

        /// <summary>
        /// Tests that the constructor properly implements the IElementConfiguration interface.
        /// Verifies that the created instance can be cast to the expected interface type.
        /// Expected result: Instance implements IElementConfiguration<TElement> interface.
        /// </summary>
        [Fact]
        public void Constructor_CreatesInstanceImplementingIElementConfiguration()
        {
            // Arrange
            var element = Substitute.For<Element>();

            // Act
            var registry = new PlatformConfigurationRegistry<Element>(element);

            // Assert
            Assert.IsAssignableFrom<IElementConfiguration<Element>>(registry);
        }
    }
}
