#nullable disable


namespace Microsoft.Maui.Controls.PlatformConfiguration.macOSSpecific.UnitTests
{
    public class NavigationPageTests
    {
        /// <summary>
        /// Tests that SetNavigationTransitionStyle extension method returns the same config object that was passed in.
        /// </summary>
        [Fact]
        public void SetNavigationTransitionStyle_ValidConfig_ReturnsSameConfigObject()
        {
            // Arrange
            var config = Substitute.For<IPlatformElementConfiguration<macOS, NavigationPage>>();
            var element = Substitute.For<BindableObject>();
            config.Element.Returns(element);

            // Act
            var result = config.SetNavigationTransitionStyle(NavigationTransitionStyle.SlideForward, NavigationTransitionStyle.SlideBackward);

            // Assert
            Assert.Same(config, result);
        }

        /// <summary>
        /// Tests that SetNavigationTransitionStyle extension method accesses the Element property of the config.
        /// </summary>
        [Fact]
        public void SetNavigationTransitionStyle_ValidConfig_AccessesElementProperty()
        {
            // Arrange
            var config = Substitute.For<IPlatformElementConfiguration<macOS, NavigationPage>>();
            var element = Substitute.For<BindableObject>();
            config.Element.Returns(element);

            // Act
            config.SetNavigationTransitionStyle(NavigationTransitionStyle.None, NavigationTransitionStyle.Crossfade);

            // Assert
            var unused = config.Received(1).Element;
        }

        /// <summary>
        /// Tests that SetNavigationTransitionStyle extension method throws ArgumentNullException when config is null.
        /// </summary>
        [Fact]
        public void SetNavigationTransitionStyle_NullConfig_ThrowsArgumentNullException()
        {
            // Arrange
            IPlatformElementConfiguration<macOS, NavigationPage> config = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                config.SetNavigationTransitionStyle(NavigationTransitionStyle.SlideUp, NavigationTransitionStyle.SlideDown));
        }

        /// <summary>
        /// Tests that SetNavigationTransitionStyle extension method works with all valid enum combinations.
        /// Input conditions: Various valid NavigationTransitionStyle enum values for both push and pop styles.
        /// Expected result: Method executes successfully and returns the config object.
        /// </summary>
        [Theory]
        [InlineData(NavigationTransitionStyle.None, NavigationTransitionStyle.None)]
        [InlineData(NavigationTransitionStyle.Crossfade, NavigationTransitionStyle.Crossfade)]
        [InlineData(NavigationTransitionStyle.SlideUp, NavigationTransitionStyle.SlideDown)]
        [InlineData(NavigationTransitionStyle.SlideLeft, NavigationTransitionStyle.SlideRight)]
        [InlineData(NavigationTransitionStyle.SlideForward, NavigationTransitionStyle.SlideBackward)]
        public void SetNavigationTransitionStyle_ValidEnumValues_ExecutesSuccessfully(
            NavigationTransitionStyle pushStyle,
            NavigationTransitionStyle popStyle)
        {
            // Arrange
            var config = Substitute.For<IPlatformElementConfiguration<macOS, NavigationPage>>();
            var element = Substitute.For<BindableObject>();
            config.Element.Returns(element);

            // Act
            var result = config.SetNavigationTransitionStyle(pushStyle, popStyle);

            // Assert
            Assert.Same(config, result);
            var unused = config.Received(1).Element;
        }

        /// <summary>
        /// Tests that SetNavigationTransitionStyle extension method handles invalid enum values.
        /// Input conditions: Out-of-range enum values for push and pop styles.
        /// Expected result: Method executes without throwing exceptions (enum validation is handled by underlying method).
        /// </summary>
        [Theory]
        [InlineData((NavigationTransitionStyle)(-1), NavigationTransitionStyle.None)]
        [InlineData(NavigationTransitionStyle.None, (NavigationTransitionStyle)(-1))]
        [InlineData((NavigationTransitionStyle)999, NavigationTransitionStyle.SlideForward)]
        [InlineData(NavigationTransitionStyle.SlideBackward, (NavigationTransitionStyle)999)]
        [InlineData((NavigationTransitionStyle)(-1), (NavigationTransitionStyle)999)]
        public void SetNavigationTransitionStyle_InvalidEnumValues_ExecutesSuccessfully(
            NavigationTransitionStyle pushStyle,
            NavigationTransitionStyle popStyle)
        {
            // Arrange
            var config = Substitute.For<IPlatformElementConfiguration<macOS, NavigationPage>>();
            var element = Substitute.For<BindableObject>();
            config.Element.Returns(element);

            // Act
            var result = config.SetNavigationTransitionStyle(pushStyle, popStyle);

            // Assert
            Assert.Same(config, result);
            var unused = config.Received(1).Element;
        }

        /// <summary>
        /// Tests that SetNavigationTransitionStyle extension method works with boundary enum values.
        /// Input conditions: First and last enum values in the NavigationTransitionStyle enumeration.
        /// Expected result: Method executes successfully and returns the config object.
        /// </summary>
        [Theory]
        [InlineData(NavigationTransitionStyle.None, NavigationTransitionStyle.SlideBackward)]
        [InlineData(NavigationTransitionStyle.SlideBackward, NavigationTransitionStyle.None)]
        public void SetNavigationTransitionStyle_BoundaryEnumValues_ExecutesSuccessfully(
            NavigationTransitionStyle pushStyle,
            NavigationTransitionStyle popStyle)
        {
            // Arrange
            var config = Substitute.For<IPlatformElementConfiguration<macOS, NavigationPage>>();
            var element = Substitute.For<BindableObject>();
            config.Element.Returns(element);

            // Act
            var result = config.SetNavigationTransitionStyle(pushStyle, popStyle);

            // Assert
            Assert.Same(config, result);
            var unused = config.Received(1).Element;
        }
    }
}
