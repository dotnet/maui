using System;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

    public class AdaptiveTriggerTests : BaseTestFixture
    {
        [Fact]
        public void ResizingWindowPageActivatesTrigger()
        {
            var redBrush = new SolidColorBrush(Colors.Red);
            var greenBrush = new SolidColorBrush(Colors.Green);
            var blueBrush = new SolidColorBrush(Colors.Blue);

            var label = new Label { Background = redBrush };

            VisualStateManager.SetVisualStateGroups(label, new VisualStateGroupList
            {
                new VisualStateGroup
                {
                    States =
                    {
                        new VisualState
                        {
                            Name = "Large",
                            StateTriggers = { new AdaptiveTrigger { MinWindowWidth = 300 } },
                            Setters = { new Setter { Property = Label.BackgroundProperty, Value = greenBrush } }
                        },
                        new VisualState
                        {
                            Name = "Small",
                            StateTriggers = { new AdaptiveTrigger { MinWindowWidth = 0 } },
                            Setters = { new Setter { Property = Label.BackgroundProperty, Value = blueBrush } }
                        }
                    }
                }
            });

            var page = new ContentPage
            {
                Frame = new Rect(0, 0, 100, 100),
                Content = label,
            };

            IWindow window = new Window
            {
                Page = page
            };

            window.FrameChanged(new Rect(0, 0, 100, 100));

            Assert.Equal(label.Background, blueBrush);

            window.FrameChanged(new Rect(0, 0, 500, 100));

            Assert.Equal(label.Background, greenBrush);

            window.FrameChanged(new Rect(0, 0, 100, 100));

            Assert.Equal(label.Background, blueBrush);
        }


        [Fact]
        public void ValidateAdaptiveTriggerDisconnects()
        {
            var greenBrush = new SolidColorBrush(Colors.Green);
            var label = new Label();
            var trigger = new AdaptiveTrigger { MinWindowWidth = 300 };


            VisualStateManager.SetVisualStateGroups(label, new VisualStateGroupList
            {
                new VisualStateGroup
                {
                    States =
                    {
                        new VisualState
                        {
                            Name = "Large",
                            StateTriggers = { trigger },
                            Setters = { new Setter { Property = Label.BackgroundProperty, Value = greenBrush } }
                        },
                    }
                }
            });

            var page = new ContentPage
            {
                Content = label,
            };

            Assert.False(trigger.IsAttached);
            _ = new Window
            {
                Page = page
            };

            Assert.True(trigger.IsAttached);

            page.Content = new Label();

            Assert.False(trigger.IsAttached);
        }

        /// <summary>
        /// Tests that the MinWindowHeight property setter correctly stores positive values.
        /// </summary>
        [Theory]
        [InlineData(0.0)]
        [InlineData(100.0)]
        [InlineData(500.5)]
        [InlineData(1920.0)]
        [InlineData(double.MaxValue)]
        public void MinWindowHeight_SetPositiveValues_StoresValueCorrectly(double value)
        {
            // Arrange
            var adaptiveTrigger = new AdaptiveTrigger();

            // Act
            adaptiveTrigger.MinWindowHeight = value;

            // Assert
            Assert.Equal(value, adaptiveTrigger.MinWindowHeight);
        }

        /// <summary>
        /// Tests that the MinWindowHeight property setter correctly stores negative values.
        /// </summary>
        [Theory]
        [InlineData(-1.0)]
        [InlineData(-100.0)]
        [InlineData(-500.5)]
        [InlineData(double.MinValue)]
        public void MinWindowHeight_SetNegativeValues_StoresValueCorrectly(double value)
        {
            // Arrange
            var adaptiveTrigger = new AdaptiveTrigger();

            // Act
            adaptiveTrigger.MinWindowHeight = value;

            // Assert
            Assert.Equal(value, adaptiveTrigger.MinWindowHeight);
        }

        /// <summary>
        /// Tests that the MinWindowHeight property setter correctly handles special double values.
        /// </summary>
        [Theory]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void MinWindowHeight_SetSpecialDoubleValues_StoresValueCorrectly(double value)
        {
            // Arrange
            var adaptiveTrigger = new AdaptiveTrigger();

            // Act
            adaptiveTrigger.MinWindowHeight = value;

            // Assert
            if (double.IsNaN(value))
            {
                Assert.True(double.IsNaN(adaptiveTrigger.MinWindowHeight));
            }
            else
            {
                Assert.Equal(value, adaptiveTrigger.MinWindowHeight);
            }
        }

        /// <summary>
        /// Tests that the MinWindowHeight property has the correct default value.
        /// </summary>
        [Fact]
        public void MinWindowHeight_DefaultValue_IsNegativeOne()
        {
            // Arrange & Act
            var adaptiveTrigger = new AdaptiveTrigger();

            // Assert
            Assert.Equal(-1.0, adaptiveTrigger.MinWindowHeight);
        }

        /// <summary>
        /// Tests that multiple set operations on MinWindowHeight property work correctly.
        /// </summary>
        [Fact]
        public void MinWindowHeight_MultipleSetOperations_LastValuePersists()
        {
            // Arrange
            var adaptiveTrigger = new AdaptiveTrigger();

            // Act
            adaptiveTrigger.MinWindowHeight = 100.0;
            adaptiveTrigger.MinWindowHeight = 200.0;
            adaptiveTrigger.MinWindowHeight = 300.5;

            // Assert
            Assert.Equal(300.5, adaptiveTrigger.MinWindowHeight);
        }
    }
}