#nullable disable

using System;
using System.ComponentModel;
using System.Reflection;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests
{
    /// <summary>
    /// Tests for the ResourceLoader static class, focusing on the ResourceProvider2 property.
    /// </summary>
    public class ResourceLoaderTests
    {
        /// <summary>
        /// Tests that the ResourceProvider2 getter returns null when no value has been set.
        /// </summary>
        [Fact]
        public void ResourceProvider2_Get_WhenNotSet_ReturnsNull()
        {
            // Arrange
            ResourceLoader.ResourceProvider2 = null;

            // Act
            var result = ResourceLoader.ResourceProvider2;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that setting ResourceProvider2 to a non-null value enables design mode and stores the value correctly.
        /// </summary>
        [Fact]
        public void ResourceProvider2_Set_WithNonNullValue_SetsDesignModeEnabledAndStoresValue()
        {
            // Arrange
            Func<ResourceLoader.ResourceLoadingQuery, ResourceLoader.ResourceLoadingResponse> testProvider =
                query => new ResourceLoader.ResourceLoadingResponse { ResourceContent = "test" };

            // Act
            ResourceLoader.ResourceProvider2 = testProvider;

            // Assert
            Assert.True(DesignMode.IsDesignModeEnabled);
            Assert.Equal(testProvider, ResourceLoader.ResourceProvider2);
        }

        /// <summary>
        /// Tests that setting ResourceProvider2 to null disables design mode and stores null.
        /// </summary>
        [Fact]
        public void ResourceProvider2_Set_WithNull_SetsDesignModeDisabledAndStoresNull()
        {
            // Arrange
            // First set a non-null value to ensure design mode is enabled
            ResourceLoader.ResourceProvider2 = query => new ResourceLoader.ResourceLoadingResponse();

            // Act
            ResourceLoader.ResourceProvider2 = null;

            // Assert
            Assert.False(DesignMode.IsDesignModeEnabled);
            Assert.Null(ResourceLoader.ResourceProvider2);
        }

        /// <summary>
        /// Tests that ResourceProvider2 can be set multiple times with different values,
        /// properly updating both the stored value and design mode state.
        /// </summary>
        [Fact]
        public void ResourceProvider2_Set_MultipleTimes_UpdatesBothFieldAndDesignMode()
        {
            // Arrange
            Func<ResourceLoader.ResourceLoadingQuery, ResourceLoader.ResourceLoadingResponse> firstProvider =
                query => new ResourceLoader.ResourceLoadingResponse { ResourceContent = "first" };
            Func<ResourceLoader.ResourceLoadingQuery, ResourceLoader.ResourceLoadingResponse> secondProvider =
                query => new ResourceLoader.ResourceLoadingResponse { ResourceContent = "second" };

            // Act & Assert - First assignment
            ResourceLoader.ResourceProvider2 = firstProvider;
            Assert.True(DesignMode.IsDesignModeEnabled);
            Assert.Equal(firstProvider, ResourceLoader.ResourceProvider2);

            // Act & Assert - Second assignment
            ResourceLoader.ResourceProvider2 = secondProvider;
            Assert.True(DesignMode.IsDesignModeEnabled);
            Assert.Equal(secondProvider, ResourceLoader.ResourceProvider2);

            // Act & Assert - Set to null
            ResourceLoader.ResourceProvider2 = null;
            Assert.False(DesignMode.IsDesignModeEnabled);
            Assert.Null(ResourceLoader.ResourceProvider2);
        }

        /// <summary>
        /// Tests the behavior when setting ResourceProvider2 from null to non-null and back to null,
        /// verifying design mode state changes correctly.
        /// </summary>
        [Fact]
        public void ResourceProvider2_Set_NullToNonNullToNull_TogglesDesignModeCorrectly()
        {
            // Arrange
            ResourceLoader.ResourceProvider2 = null;
            Assert.False(DesignMode.IsDesignModeEnabled);

            Func<ResourceLoader.ResourceLoadingQuery, ResourceLoader.ResourceLoadingResponse> testProvider =
                query => new ResourceLoader.ResourceLoadingResponse { UseDesignProperties = true };

            // Act & Assert - Set non-null
            ResourceLoader.ResourceProvider2 = testProvider;
            Assert.True(DesignMode.IsDesignModeEnabled);
            Assert.Equal(testProvider, ResourceLoader.ResourceProvider2);

            // Act & Assert - Set back to null
            ResourceLoader.ResourceProvider2 = null;
            Assert.False(DesignMode.IsDesignModeEnabled);
            Assert.Null(ResourceLoader.ResourceProvider2);
        }
    }
}
