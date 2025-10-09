using System;

using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.Handlers.UnitTests
{
    public partial class MenuFlyoutItemHandlerTests
    {
#if !WINDOWS && !IOS
        /// <summary>
        /// Test helper class that exposes the protected CreatePlatformElement method for testing.
        /// </summary>
        private class TestableMenuFlyoutItemHandler : MenuFlyoutItemHandler
        {
            public new object CreatePlatformElement()
            {
                return base.CreatePlatformElement();
            }
        }

        /// <summary>
        /// Tests that CreatePlatformElement throws NotImplementedException when called on non-Windows and non-iOS platforms.
        /// This verifies that the method correctly indicates the functionality is not implemented for these platforms.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void CreatePlatformElement_WhenCalled_ThrowsNotImplementedException()
        {
            // Arrange
            var handler = new TestableMenuFlyoutItemHandler();

            // Act & Assert
            Assert.Throws<NotImplementedException>(() => handler.CreatePlatformElement());
        }
#endif

        /// <summary>
        /// Tests that the MenuFlyoutItemHandler constructor successfully creates a valid instance
        /// and properly initializes the handler with the static Mapper and CommandMapper.
        /// </summary>
#if IOS
        [System.Runtime.Versioning.SupportedOSPlatform("ios13.0")]
#endif
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Constructor_WithStaticMappers_CreatesValidInstance()
        {
            // Act
            var handler = new MenuFlyoutItemHandler();

            // Assert
            Assert.NotNull(handler);
            Assert.IsType<MenuFlyoutItemHandler>(handler);
            Assert.IsAssignableFrom<IMenuFlyoutItemHandler>(handler);
            Assert.IsAssignableFrom<ElementHandler<IMenuFlyoutItem, object>>(handler);
        }

        /// <summary>
        /// Tests that the MenuFlyoutItemHandler constructor properly uses the static Mapper field
        /// by verifying the Mapper property is accessible and not null.
        /// </summary>
#if IOS
        [System.Runtime.Versioning.SupportedOSPlatform("ios13.0")]
#endif
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Constructor_WithStaticMappers_InitializesWithValidMapper()
        {
            // Act
            var handler = new MenuFlyoutItemHandler();

            // Assert
            Assert.NotNull(MenuFlyoutItemHandler.Mapper);
            Assert.IsAssignableFrom<IPropertyMapper<IMenuFlyoutItem, IMenuFlyoutItemHandler>>(MenuFlyoutItemHandler.Mapper);
        }

        /// <summary>
        /// Tests that the MenuFlyoutItemHandler constructor properly uses the static CommandMapper field
        /// by verifying the CommandMapper property is accessible and not null.
        /// </summary>
#if IOS
        [System.Runtime.Versioning.SupportedOSPlatform("ios13.0")]
#endif
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Constructor_WithStaticMappers_InitializesWithValidCommandMapper()
        {
            // Act
            var handler = new MenuFlyoutItemHandler();

            // Assert
            Assert.NotNull(MenuFlyoutItemHandler.CommandMapper);
            Assert.IsAssignableFrom<CommandMapper<IMenuFlyoutItem, IMenuFlyoutItemHandler>>(MenuFlyoutItemHandler.CommandMapper);
        }

        /// <summary>
        /// Tests that multiple instances of MenuFlyoutItemHandler can be created successfully,
        /// ensuring the constructor is stable and reusable.
        /// </summary>
#if IOS
        [System.Runtime.Versioning.SupportedOSPlatform("ios13.0")]
#endif
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Constructor_MultipleInstances_CreatesDistinctValidInstances()
        {
            // Act
            var handler1 = new MenuFlyoutItemHandler();
            var handler2 = new MenuFlyoutItemHandler();

            // Assert
            Assert.NotNull(handler1);
            Assert.NotNull(handler2);
            Assert.NotSame(handler1, handler2);
            Assert.IsType<MenuFlyoutItemHandler>(handler1);
            Assert.IsType<MenuFlyoutItemHandler>(handler2);
        }
    }
}