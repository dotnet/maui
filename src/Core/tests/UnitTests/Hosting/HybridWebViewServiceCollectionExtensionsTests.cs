using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Hosting;
using NSubstitute;
using System;
using Xunit;


namespace Core.UnitTests.Hosting
{
    /// <summary>
    /// Unit tests for HybridWebViewServiceCollectionExtensions class.
    /// </summary>
    public partial class HybridWebViewServiceCollectionExtensionsTests
    {
        /// <summary>
        /// Tests that AddHybridWebViewDeveloperTools throws ArgumentNullException
        /// when called with null IServiceCollection parameter.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void AddHybridWebViewDeveloperTools_NullServiceCollection_ThrowsArgumentNullException()
        {
            // Arrange
            IServiceCollection nullServiceCollection = null;

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                HybridWebViewServiceCollectionExtensions.AddHybridWebViewDeveloperTools(nullServiceCollection));

            Assert.Equal("services", exception.ParamName);
        }

    }
}
